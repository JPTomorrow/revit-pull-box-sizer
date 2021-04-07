namespace JPMorrow.UI.ViewModels
{
	using System.Windows;
	using System;
	using JPMorrow.Tools.Diagnostics;
	using Autodesk.Revit.DB;
	using System.Linq;
	using System.Collections.Generic;
	using JPMorrow.Revit.PullBoxes;

	public partial class ParentViewModel
    {
        /// <summary>
        /// prompt for save and exit
        /// </summary>
        public void MasterClose(Window window)
        {
            try
            {
                //  This window is a SPAWNED process it can be thought of as a standalone applicaiton, When this window is closed, control of the program is not returned to the Execute loop that spawned this window, located in ThisApplication.cs

                //  Therefore, closing this window signifies the termination of the program, you should think of it as you are killing a spawned process, because that is exactly what you are doing when you close this window.

                //  You may think to yourself, well why not have control return to the execute loop and continue executing code within the API context? which would allow you to directly interface with the current Revit process that spawned this window, instead of shouting messages at it to get Revit to do stuff in the model.

                //  The answer to that question is, yes, you can do that, but there is one fatal flaw that stops if from being useful. If you do pv.ShowDialog() instead of pv.Show() in the main execute loop, you will spawn this window as a what is called a "Modal" form. That means that it pauses the execution of its parent process. This means that revit will be frozen for the duration that the spawned window is open, and when the window is closed, only then will execution be returned to Revit and the application will continue functioning. This is a bad thing because it means you cannot fire code that effects the Revit document while the window is open. So nothing that has the effect of live updating anything in Revit can occur while this window is open. Basically, this is a long winded explaination to say DONT USE MODAL FORMS... ALWAYS SPAWN A WINDOW WITH pv.Show() AND HANDLE COMMUNICATION WITH REVIT THROUGH AN EVENT DRIVEN SYSTEM SUCH AS THE ONE DISPLAYED TOWARDS THE BOTTOM OF PullBox.cs

                window.Close();
            }
            catch(Exception ex)
            {
                debugger.show(err:ex.ToString());
            }
        }

        public void Exec(Window window)
        {
            try
            {
                var items = ConduitDiaItems.ToList();
                var rem_dias = RemDiasItems.Select(x => x.Value).ToList();
                PullBox pb = new PullBox(BoxDepthItems[SelBoxDepth], ConduitDiaItems[SelConduitDiaDiameter], rem_dias, StrPull);
                DimensionStr = pb.Dimensions.DimensionString;
                RaisePropertyChanged("DimensionStr");

                if(pb == null) return;

                var sel_pb_ids = Info.UIDOC.Selection.GetElementIds();

                if(!sel_pb_ids.Any()) return;

                var final_elements = new List<Element>();
                foreach(var id in sel_pb_ids)
                {
                    var el = Info.DOC.GetElement(id);

                    bool pparse(string param_name) => el.LookupParameter(param_name) == null;

                    if(	el == null || !el.Category.Name.Equals("Electrical Equipment") ||
                        pparse("Width") || pparse("Depth") || pparse("Height"))
                    {
                        return;
                    }

                    final_elements.Add(el);
                }

                PullBox.SizePullBoxEv(Info, final_elements, pb);

                Info.UIDOC.Selection.SetElementIds(sel_pb_ids.ToList());
            }
            catch(Exception ex)
            {
                debugger.show(err:"PullBox Generation Failure...\n\n" + ex.ToString());
            }
        }

        public void AddDia(Window window)
        {
            try
            {
                string dia = ConduitDiaItems[SelRemConduitDiaDiameter];
                RemDiasItems.Add(new DiameterPresenter(dia));
                RaisePropertyChanged("ConduitDiaItems");
            }
            catch(Exception ex)
            {
                debugger.show(err:ex.ToString());
            }
        }
    }
}
