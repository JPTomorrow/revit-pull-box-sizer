namespace JPMorrow.UI.ViewModels
{
	#region using
	using System;
	using System.Runtime.CompilerServices;
	using System.ComponentModel;
	using System.Windows.Input;
	using System.Windows;
	using JPMorrow.Revit.Documents;
	using System.Windows.Threading;
	using System.Collections.Generic;
	using JPMorrow.Revit.Tools;
	using Autodesk.Revit.DB;
	using System.Collections.ObjectModel;
	using MainApp;
	using System.Linq;
	using System.IO;
	#endregion

	public partial class ParentViewModel : Presenter
    {
        // Revit Model Info. There can be no comprimise here, if you are going to comunicate with Revit after spawning this window, you must have all of this model information from the commandData in the Execute Loop. Learn to love this ModelInfo, cause you will... whether you like it or not. You will be passing this shit around everywhere, sometimes carelessly.
        private static ModelInfo Info { get; set; }

        //////////////////
        //// UI /////////
        //////////////////

        // Collections that update combo boxes and list boxes visually. Items stored in these lists will show up in the UI elements they are bound to.
        public ObservableCollection<string> ConduitDiaItems { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> BoxDepthItems { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<DiameterPresenter> RemDiasItems { get; set; } = new ObservableCollection<DiameterPresenter>();

        // selectors for the collections above. These reflect the currently selected index of the items in the collections above. So ConduitDiaItems[SelConduitDiameter] would return the item the user has currently selected in the UI element (List Box, Combo Box, etc.).
        private int scdd;
        public int SelConduitDiaDiameter { get { return scdd; } set { scdd = value; RaisePropertyChanged("SelConduitDiaDiameter"); }}
        private int srcdd;
        public int SelRemConduitDiaDiameter { get { return srcdd; } set { srcdd = value; RaisePropertyChanged("SelConduitDiaDiameter"); }}

        private int bd;
        public int SelBoxDepth { get { return bd; } set { bd = value; RaisePropertyChanged("SelBoxDepth"); }}

        // Direct text and checkbox bindings do not require a ICommand relay
        public bool StrPull { get; set; } = true;
        public string DimensionStr { get; set; }

        //Commands that relay to the functions in ParentViewCmds.cs. this is a direct binding from the xaml of the view to this class, allowing you to execute code on button press.
        public ICommand MasterCloseCmd => new RelayCommand<Window>(MasterClose);
        public ICommand ExecCmd => new RelayCommand<Window>(Exec);
        public ICommand AddDiaCmd => new RelayCommand<Window>(AddDia);

        //////////////////////////////
        //// View Constructor ////////
        //////////////////////////////
        public ParentViewModel(ModelInfo info)
        {
            //revit documents and pre converted elements
            Info = info;

            scdd = 0;
            bd = 0;
            BoxDepthItems = new ObservableCollection<string>(new string[] { "2\"", "4\"", "6\"", "8\"", "10\"", "12\""});

            string[] conduit_dias = new string[] {
                "0 1/2\"", "0 3/4\"", "1\"", "1 1/4\"", "1 1/2\"", "2\"",
                "2 1/2\"", "3\"", "3 1/2\"", "4\"", "5\""
            };

            ConduitDiaItems = new ObservableCollection<string>(conduit_dias);
        }
    }

    /// <summary>
    /// A presneter for conduit diameter. These presenters are used by the observable collections at the top of this file to visually format a complex class or object's values for printing to the screen and updating the Items.
    /// </summary>
    public class DiameterPresenter : Presenter
    {
        public string Value;
        public DiameterPresenter(string value)
        {
            Value = value;
            Dia = value;
        }

        private string dia;
        public string Dia { get => dia;
            set {
                dia = value;
                RaisePropertyChanged("RemDiasItems");
            } }

        //Item Selection Bindings
        private bool _isSelected;
        public bool IsSelected { get => _isSelected;
            set {
                _isSelected = value;
                RaisePropertyChanged("RemDiasItems");
        }}
    }

    /// <summary>
    /// Default Presenter: Just Presents a string value as a listbox item,
    /// can replace with an object for more complex listbox databindings
    /// </summary>
    public class ItemPresenter : Presenter
    {
        private readonly string _value;
        public ItemPresenter(string value) => _value = value;
    }

    #region Inherited Classes
    public abstract class Presenter : INotifyPropertyChanged
    {
         public event PropertyChangedEventHandler PropertyChanged;

         protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
         {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
         }
    }
    #endregion


    /// <summary>
    /// This class helps fire off functions on Button press of the UI. Refer to a Button in ParentView.xaml to see how it is bound to the ICommands above.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T> _execute = null;
        readonly Predicate<T> _canExecute = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {}

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        #region ICommand Members

        ///<summary>
        ///Defines the method that determines whether the command can execute in its current state.
        ///</summary>
        ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        ///<returns>
        ///true if this command can be executed; otherwise, false.
        ///</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        ///<summary>
        ///Occurs when changes occur that affect whether or not the command should execute.
        ///</summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        ///<summary>
        ///Defines the method to be called when the command is invoked.
        ///</summary>
        ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        #endregion
    }

}