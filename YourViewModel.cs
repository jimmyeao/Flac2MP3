using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;

namespace Flac2MP3
{
    // Define a public class named FileInfo
    public class FileInfo
    {
        #region Public Properties

        // Define a public property named FileName
        // This property returns the file name extracted from the FilePath
        public string FileName => System.IO.Path.GetFileName(FilePath);

        // Define a public property named FilePath
        // This property gets or sets the file path
        public string FilePath { get; set; }

        #endregion Public Properties
    }

    // This class implements the IDropTarget interface, which allows it to handle drop events
    public class YourDropHandler : IDropTarget
    {
        #region Private Fields

        private readonly YourViewModel _viewModel;

        #endregion Private Fields

        #region Public Constructors

        // Constructor that takes a YourViewModel object as a parameter
        public YourDropHandler(YourViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        #endregion Public Constructors

        #region Public Methods

        // Method that handles the drag over event
        public void DragOver(IDropInfo dropInfo)
        {
            // Set the effects of the drop to Move
            dropInfo.Effects = DragDropEffects.Move;
        }

        // Method that handles the drop event
        public void Drop(IDropInfo dropInfo)
        {
            // Check if the dropped data and target data are both of type FileInfo
            if (dropInfo.Data is FileInfo && dropInfo.TargetItem is FileInfo)
            {
                // Cast the dropped data and target data to FileInfo
                var droppedData = dropInfo.Data as FileInfo;
                var targetData = dropInfo.TargetItem as FileInfo;

                // Find the index of the target data in the file list
                var index = _viewModel.FileList.IndexOf(targetData);

                // If the target data is found in the file list
                if (index != -1)
                {
                    // Remove the dropped data from the file list
                    _viewModel.FileList.Remove(droppedData);

                    // Insert the dropped data at the target index in the file list
                    _viewModel.FileList.Insert(index, droppedData);
                }
            }
        }

        #endregion Public Methods
    }

    // This class represents a view model that implements the INotifyPropertyChanged interface
    public class YourViewModel : INotifyPropertyChanged
    {
        #region Public Constructors
        private string selectedBitrate = "128 kbps"; // Default value
        public string SelectedBitrate
        {
            get => selectedBitrate;
            set
            {
                if (selectedBitrate != value)
                {
                    selectedBitrate = value;
                    OnPropertyChanged(nameof(SelectedBitrate));
                }
            }
        }
        // Constructor for the view model
        public YourViewModel()
        {
            // Create a new instance of YourDropHandler and assign it to the DropHandler property
            DropHandler = new YourDropHandler(this);
        }

        #endregion Public Constructors

        #region Public Events

        // Event that is raised when a property value changes
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        // Property that represents the drop handler
        public YourDropHandler DropHandler { get; private set; }

        // Property that represents a collection of FileInfo objects
        public ObservableCollection<FileInfo> FileList { get; } = new ObservableCollection<FileInfo>();

        #endregion Public Properties

        #region Protected Methods

        // Method that raises the PropertyChanged event
        protected virtual void OnPropertyChanged(string propertyName)
        {
            // Invoke the PropertyChanged event with the specified property name
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Protected Methods
    }
}
