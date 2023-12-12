using System.ComponentModel;

namespace WinAudioAssistant.ViewModels
{
    /// <summary>
    /// Base class for window-style view models.
    /// Exposes commands for the OK, Cancel, and Apply buttons.
    /// Defines methods for applying and discarding changes, checking if the viewmodel should close, and cleaning up resources.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region Constructors
        /// <summary>
        /// Initializes the base view model.
        /// </summary>
        public BaseViewModel()
        {
            OkCommand = new RelayCommand(OkAction);
            CancelCommand = new RelayCommand(CancelAction);
            ApplyCommand = new RelayCommand(ApplyAction);
        }
        #endregion

        #region Properties
        public Action CloseViewAction { get; set; } = () => { }; // References method to close the view. Set in BaseView.
        public Action FocusViewAction { get; set; } = () => { }; // References method to focus the view. Set in BaseView.
        public RelayCommand OkCommand { get; } // Command for the OK button.
        public RelayCommand CancelCommand { get; } // Command for the Cancel button.
        public RelayCommand ApplyCommand { get; } // Command for the Apply button.
        #endregion

        #region Events
        /// <summary>
        /// Raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Public Methods
        /// <summary>
        /// Method to apply pending changes for this viewmodel.
        /// </summary>
        /// <returns>True if changes were successfully applied.</returns>
        public abstract bool Apply();

        /// <summary>
        /// Method to discard pending changes for this viewmodel.
        /// </summary>
        /// <returns>True if changes were successfully discarded.</returns>
        public abstract bool Discard();

        /// <summary>
        /// Method to check if the viewmodel should close, usually verifying that there are no pending changes.
        /// </summary>
        /// <returns>True if the viewmodel should close.</returns>
        public abstract bool ShouldClose();

        /// <summary>
        /// Method to clean up resources used by this viewmodel on close.
        /// </summary>
        public abstract void Cleanup();
        #endregion

        #region Private Methods
        /// <summary>
        /// Called when the OK button is clicked.
        /// Informs the viewmodel to apply pending changes and close the view if appropriate.
        /// </summary>
        private void OkAction(object? parameter)
        {
            if (Apply() && ShouldClose())
                CloseViewAction();
        }

        /// <summary>
        /// Called when the Cancel button is clicked.
        /// Informs the viewmodel to discard pending changes and close the view if appropriate.
        /// </summary>
        private void CancelAction(object? parameter)
        {
            if (Discard() && ShouldClose())
                CloseViewAction();
        }

        /// <summary>
        /// Called when the Apply button is clicked.
        /// Informs the viewmodel to apply pending changes.
        /// </summary>
        private void ApplyAction(object? parameter)
        {
            Apply();
        }

        /// <summary>
        /// Called when a property is changed.
        /// Informs the view that the property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
