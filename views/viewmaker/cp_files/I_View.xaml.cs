using System.Windows;
using System.Windows.Markup;
using System.Windows.Input;
using JPMorrow.UI.ViewModels;
using JPMorrow.Revit.Documents;
using JPMorrow.Revit.ElementCollection;

namespace JPMorrow.UI.Views
{
	/// <summary>
	/// Code Behind landing for templateForm.xaml
	/// </summary>
	public partial class I_View : Window, IComponentConnector
	{
		/// <summary>
		/// Default Constructor.static Bind DataContext
		/// </summary>
		public I_View(ModelInfo info)
		{
			InitializeComponent();
			this.DataContext = new I_ViewModel(info);
		}

		/// <summary>
		/// /// Custom Window Drag on DockPanel
		/// </summary>
		private void WindowDrag(object o, MouseEventArgs e)
		{
			this.DragMove();
		}

		/// <summary>
		/// Custom Window Drag on DockPanel
		/// </summary>
		private void HelpClick(object o, RoutedEventArgs e)
		{

		}

		/// <summary>
		/// Custom Window Drag on DockPanel
		/// </summary>
		private void ExitClick(object o, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
