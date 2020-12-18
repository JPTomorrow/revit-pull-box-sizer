using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using JPMorrow.Revit.Documents;
using System;
using JPMorrow.Tools.Diagnostics;
using System.Linq;

namespace JPMorrow.Revit.PullBoxes
{
	/// <summary>
	/// Logically represents information about a pull box that has been collected from Revit.
	/// </summary>
	public class PullBox
	{
		public BoxDimension Dimensions { get; }

		public PullBox(
			string height, string conduit_dia,
			List<string> rem_dias, bool straight_pull = true)
		{
			double in_size = 0.0;
			static double get_in(string dia)
			{
				DimensionSwap.ConduitDiameterToInches.TryGetValue(dia, out double val);
				return val;
			}

			if(straight_pull)
			{
				in_size = get_in(conduit_dia) * 8.0;
			}
			else
			{
				in_size = get_in(conduit_dia) * 6.0;

				double rem_dia_ins = 0.0;
				rem_dias.ForEach(x => rem_dia_ins += get_in(x));
				in_size += rem_dia_ins;
			}

			var final_in = (int)Math.Ceiling(in_size);

			var h_num = int.Parse(height.TrimEnd('\"'));
			Dimensions = new BoxDimension(final_in, h_num);
		}

		/// <summary>
		/// This is the Event System for sizing pull boxes, A message will be packaged up and sent to the Revit application for processing
		/// </summary>
		private static PullBoxSizing handler_size_box = null;
		private static ExternalEvent exEvent_size_box = null;

		/// <summary>
		/// Pull box sizing Event Signup, must be called in Revit Execute loop
		/// </summary>
		public static void PullBoxSizingSignUp()
		{
			handler_size_box = new PullBoxSizing();
			exEvent_size_box = ExternalEvent.Create(handler_size_box);
		}

		/// <summary>
		/// Event Raiser for Sizing Pull Boxes. Call this function in your code when you would like to acutally size pull boxes
		/// </summary>
		public static void SizePullBoxEv(ModelInfo info, IEnumerable<Element> boxes, PullBox box_info)
		{
			handler_size_box.Info = info;
			handler_size_box.Boxes = boxes.ToList();
			handler_size_box.BoxInfo = box_info;

			exEvent_size_box.Raise();
		}

		/// <summary>
		/// Event that sizes pull box, This is the structure of the message and the algorithm inside of Execute() will be fired when the message is recieved by Revit.
		/// </summary>
		public class PullBoxSizing : IExternalEventHandler
		{
			public ModelInfo Info { get; set; }
			public List<Element> Boxes { get; set; }
			public PullBox BoxInfo { get; set; }

			public void Execute(UIApplication app)
			{
				// size pullbox post ui
				using( TransactionGroup transGroup = new TransactionGroup(Info.DOC, "Pull Box Sizing" ) )
				{
					transGroup.Start();
					try
					{
						using (Transaction tx = new Transaction(Info.DOC, "Size Pull Box"))
						{
							tx.Start();

							foreach(var box in Boxes)
							{
								void set(string pname, string preparse)
								{
									bool s = UnitFormatUtils.TryParse(Info.DOC.GetUnits(), UnitType.UT_Length, preparse, out double val);
									if(s) box.LookupParameter(pname).Set(val);
								}

								set("Width", BoxInfo.Dimensions.WidthInStr);
								set("Height", BoxInfo.Dimensions.HeightInStr);
								set("Depth", BoxInfo.Dimensions.DepthInStr);
							}

							tx.Commit();
						}
					}
					catch(Exception ex)
					{
						debugger.show(err:"Problems sizing box.\n " + ex.ToString());
						transGroup.RollBack();
					}

					transGroup.Assimilate();
				}
			}

			public string GetName()
			{
				return "Size Pull Boxes";
			}
		}
	}

	/// <summary>
	/// Represents the Dimensions of a Pull Box from Revit
	/// </summary>
	public class BoxDimension
	{
		public int WidthIn { get; }
		public int HeightIn { get; }
		public int DepthIn { get; }

		public string WidthInStr { get => WidthIn + "\""; }
		public string HeightInStr { get => HeightIn + "\""; }
		public string DepthInStr { get => DepthIn + "\""; }

		public string DimensionString { get => string.Format("W{0} x H{1} x D{2}",
			WidthInStr, HeightInStr, DepthInStr); }

		public BoxDimension(int width_height, int depth)
		{
			int w_standard_idx = DimensionSwap.StandardWidthHeight.BinarySearch(width_height);
			if(w_standard_idx < 0)
				w_standard_idx = ~w_standard_idx - 1;

			if(width_height > DimensionSwap.StandardWidthHeight.Last())
				width_height = DimensionSwap.StandardWidthHeight.Last();

			// WORKING ON STANDARDIZING THE WIDHT AND DEPTH

			var new_width = DimensionSwap.StandardWidthHeight[w_standard_idx];

			WidthIn = new_width;
			HeightIn = new_width;
			DepthIn = depth;
		}
	}

	/// <summary>
	/// Conversion Tables for pull box sizing
	/// </summary>
	public static class DimensionSwap
	{
		public static Dictionary<string, double> ConduitDiameterToInches { get; } = new Dictionary<string, double>() {
			{ "0 1/2\""		,	0.5 },
			{ "0 3/4\""		,	0.75 },
			{ "1\""			,	1.0 },
			{ "1 1/4\""		,	1.25 },
			{ "1 1/2\""		,	1.5 },
			{ "2\""			,	2.0 },
			{ "2 1/2\""		,	2.5 },
			{ "3\""			,	3.0 },
			{ "3 1/2\""		,	3.5 },
			{ "4\""			,	4.0 },
			{ "5\""			,	5.0 },
		};

		public static List<int> StandardWidthHeight { get; } = new List<int>() {
			4, 6, 8, 10, 12, 16, 18, 24, 30, 36
		};
	}
}