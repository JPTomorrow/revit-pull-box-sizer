namespace JPMorrow.UI.ViewModels
{
	#region using
	using System.Windows.Input;
	using System.Windows;
	using JPMorrow.Revit.Documents;
	#endregion

	public partial class I_ViewModel : Presenter
    {
        //Revit Model Info
        private ModelInfo Info { get; set; }

        public ICommand CloseCmd => new RelayCommand<Window>(SaveAndClose);

        /// <summary>
        /// The Main View Model for RunTableView.xaml
        /// </summary>
        public I_ViewModel(ModelInfo info)
        {
            //revit documents and pre converted elements
            Info = info;
        }

        //Class End
    }

}