/// <summary>
/// AS A NOTE, DO NOT COMMENT CODE TO THE EXTENT THAT I HAVE COMMENTED THIS CODE.
/// IT IS BAD TO COMMENT CODE TO THIS EXTENT. THE NAMES OF YOUR FUNCTIONS AND TYPES SHOULD EXPLAIN THE CODE BETTER THAN A LONG WINDED SERIES OF COMMENTS SUCH AS THE VERY COMMENT YOU ARE READING RIGHT NOW.
/// </summary>

using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using JPMorrow.Revit.Documents;
using System.Reflection;
using System.IO;
using JPMorrow.Tools.Diagnostics;
using JPMorrow.UI.Views;
using System.Diagnostics;
using JPMorrow.Revit.PullBoxes;

namespace MainApp
{
	/// <summary>
	/// Main Execution Method
	/// (THIS IS WHERE THE PROGRAM STARTS)
	/// </summary>
	[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
	[Autodesk.Revit.DB.Macros.AddInId("9BBF529B-520A-4877-B63B-BEF1238B6A05")]
    public partial class ThisApplication : IExternalCommand
    {
		// Directories to be geneerated in the root application file structure
		public static string[] Data_Dirs { get; } = new string[] {
			"action_log",
		};

		// Defining the root application directory
		public static string App_Base_Path { get; set; } = null;
		public static string Settings_Base_Path { get; private set; }

		// A debug switch to cause the output program to go to /TESTBED/ folder instead of the intended App_Base_Path, this is for testing of things without messing up the last build of this program, it may be buggy, do not use it or it may break the program right now.
		public static bool TestBed_Debug_Switch {get; set; } = false;

		// THIS IS THE MAIN EXECUTION FOR A REVIT APPLICATION!!!
		// this is equivilent to a "Main(void)" function you would find if you created a black c# project. This is just Revit's redirection of the main function to point towards Revits API  It is where the program officially "starts" There are some preprocess functions you can call that will come before this, such as OnModuleLoad() that will fire before this execute loop, for added granularity in the setup
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
			//set revit model info
			// ModelInfo is a very key data structure to understand. It is located in common_build_source/RevitDocumentManager.cs.
			// Packaging the relevent information to manupulate the Revit Model using the commandData in parameter from this Execute loop. I can then pass all of this information to a UI in order to get some shit done.
			ModelInfo revit_info = ModelInfo.StoreDocuments(commandData);
			IntPtr main_rvt_wind = Process.GetCurrentProcess().MainWindowHandle;

			// set app path
			Assembly assem = Assembly.GetExecutingAssembly();
			UriBuilder uri = new UriBuilder(assem.CodeBase);
			string module_path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
			App_Base_Path = RAP.GetApplicationBasePath(module_path, assem.GetName().Name, String.Join, TestBed_Debug_Switch);
			Settings_Base_Path = App_Base_Path + "settings\\";

			//create data directories
			RAP.GenAppStorageStruct(Settings_Base_Path, Data_Dirs, Directory.CreateDirectory, Directory.Exists);

			// SIGN UP FOR REVIT EVENT MESSAGING SYSTEM ON ALL RELEVANT CLASSES BEFORE WE LEAVE EXECUTE LOOP
			PullBox.PullBoxSizingSignUp();

			// Leaving this Execute function and entering into the UI code below will cause the commandData that we stored in revit_info to "become stale" meaning that it can no longer be used to perform Revit Transactions that change the model directly.


			// SPAWN THE UI MENU
			// This is where you get into the WPF UI. All of of the code that is fired from here will be abstracted away into ParentViewModel.xaml, ParentViewModel.xaml.cs, ParentViewModel.cs, and ParentViewCmds.cs. My suggestion is to right click on ParentView below in VSCode, and then use the "Go to Definition" option to follow the trail down the UI code until you reach the ParentViewCmds.cs, where the acutal functions that fire logical code are. Once you get to ParentViewCmds.cs, you are looking at what is called a "UI" implementation layer. The purpose of this layer is to have your logic code separated from the code that defines and draws the UI (ParentView.xaml, ParentView.xaml.cs, ParentViewModel.cs). So at ParentViewCmds.cs, you will start finding calls to libraries that represent the actual DATA being handled behind the scenes before being presented to the UI. An example of one such class in this program is (libs/PullBox.cs) which is the only library that is called in ParentViewCmds.cs so it should serve as a good isolated example of abstracting the non-UI data.

			try
			{
				ParentView pv = new ParentView(revit_info, main_rvt_wind);
				pv.Show();
			}
			catch(Exception ex)
			{
				debugger.show(err:ex.ToString());
			}

			// 	End of program execution, window will be open when this is reached so this is only the end of Revit's execution of this program. The window will live on and be able to talk with Revit externally.
			return Result.Succeeded;
        }
    }
}