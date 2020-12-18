using System.Windows;
using System;
using JPMorrow.Tools.Diagnostics;

namespace JPMorrow.UI.ViewModels
{
	public partial class I_ViewModel
    {

        public void SaveAndClose(Window window)
        {
            try
            {
                window.Close();
            }
            catch(Exception ex)
            {
                debugger.show(err:ex.ToString());
            }
        }
    }
}
