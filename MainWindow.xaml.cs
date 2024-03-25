using System.ComponentModel;
using System.IO;
using NAudio.Lame;
using NAudio.Wave; // Import the NAudio.Wave namespace
using System.Text;
using System.Windows;
using Microsoft.Win32; // Import the Microsoft.Win32 namespace for OpenFileDialog and SaveFileDialog
using System; // Import the System namespace
using System.Collections.Generic; // Import the System.Collections.Generic namespace
using System.Linq; // Import the System.Linq namespace
using System.Windows.Media.Animation; // Import the System.Windows.Media.Animation namespace

namespace Flac2MP3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        private BackgroundWorker worker = new BackgroundWorker();
        private bool? overwriteAll = null;
        #endregion Private Fields

        #region Public Constructors

        // Create a BackgroundWorker object
        public MainWindow()
        {
            InitializeComponent();
            // Set the DataContext of the MainWindow to a new instance of YourViewModel
            this.DataContext = new YourViewModel();

            // Call the method to setup the BackgroundWorker
            SetupBackgroundWorker();

            // Create a new Uri for the theme file
            Uri themeUri;
            themeUri = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");

            // Check if the theme is already added to the application resources
            var existingTheme = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source == themeUri);

            // If the theme is not already added, add it to the application resources
            if (existingTheme == null)
            {
                existingTheme = new ResourceDictionary() { Source = themeUri };
                Application.Current.Resources.MergedDictionaries.Add(existingTheme);
            }
        }

        #endregion Public Constructors

        #region Private Methods

        // This method is used to animate a progress bar to zero value
        private void AnimateProgressBarToZero()
        {
            // Check if we need to invoke this method on the UI thread
            if (!progressBar.Dispatcher.CheckAccess())
            {
                // Marshal the execution of this method onto the UI thread
                progressBar.Dispatcher.Invoke(new Action(AnimateProgressBarToZero));
                return; // Exit the current execution path, as the operation will be continued on the UI thread
            }

            // Create a new DoubleAnimation object (this part now runs on the UI thread)
            var animation = new DoubleAnimation
            {
                From = progressBar.Value,  // Starting value of the animation
                To = 0,                     // Ending value of the animation
                Duration = TimeSpan.FromSeconds(1), // Duration of the animation
                FillBehavior = FillBehavior.Stop // Add this line
            };

            // Begin the animation on the ValueProperty of the progress bar
            progressBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, animation);

            // Once the animation is complete, remove it and manually set the value to 0.
            // This ensures the progress bar can be updated again in the future.
            animation.Completed += (s, e) =>
            {
                progressBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, null);
                progressBar.Value = 0; // Explicitly set the value to 0 to reflect the current state.
            };
        }



        private void btnAddFiles_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "FLAC Files|*.flac" // Changed from "MP3 Files|*.mp3" to "FLAC Files|*.flac"
            };

            // Show the file dialog and check if the user clicked the "OK" button
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the view model associated with the current DataContext
                var viewModel = DataContext as YourViewModel;

                // Check if the FileList in the view model is not empty
                if (viewModel.FileList.Any())
                {
                    // Create a new instance of AddFilesDialog
                    var dialog = new AddFilesDialog();

                    // Set the main window as the owner of the dialog
                    dialog.Owner = this;

                    // Show the dialog and wait for the user to close it
                    dialog.ShowDialog();

                    // Check the result of the dialog
                    if (dialog.DialogResult == "NewList")
                    {
                        viewModel.FileList.Clear(); // Clear the existing list in the view model
                    }
                    else if (dialog.DialogResult == "Cancel")
                    {
                        return; // Cancel the operation and return from the event handler
                    }
                }

                // Iterate over each selected file in the file dialog
                foreach (string file in openFileDialog.FileNames)
                {
                    // Create a new instance of FileInfo and add it to the FileList in the view model
                    viewModel.FileList.Add(new FileInfo { FilePath = file });
                }
            }
        }

        private void btnclearList_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as YourViewModel;
            viewModel.FileList.Clear();
        }

        // This method is called when the "Join Files" button is clicked
        private void btnConvertFiles_Click(object sender, RoutedEventArgs e)
        {
            overwriteAll = null;
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = 0; // Reset progress bar immediately without waiting for animation
                UpdateStatus("Starting conversion...");
            });
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var viewModel = DataContext as YourViewModel;
                string selectedPath = folderBrowserDialog.SelectedPath; // The selected folder path

                var flacFiles = viewModel?.FileList.Select(f => f.FilePath).ToList();
                string selectedBitrate = viewModel.SelectedBitrate; // Make sure this gets the right value

                var data = new { OutputFolder = selectedPath, FlacFiles = flacFiles, SelectedBitrate = selectedBitrate };
                worker.RunWorkerAsync(data);
            }
        }

        private int ConvertBitrate(string bitrateString)
        {
            // Extract the numerical part from the bitrate string and convert to integer
            return int.Parse(bitrateString.Split(' ')[0]) * 1000; // Convert "128 kbps" to 128000, for example
        }

        private LAMEPreset DeterminePresetFromBitrate(int bitrate)
        {
            switch (bitrate)
            {
                case 128000:
                    return LAMEPreset.ABR_128; // Corresponding to ~130kbps VBR for standard quality
                case 192000:
                    return LAMEPreset.STANDARD; // Corresponding to ~190kbps VBR for high quality
                case 256000:
                    return LAMEPreset.ABR_256; // Corresponding to ~250kbps VBR for higher quality
                case 320000:
                    return LAMEPreset.INSANE; // Corresponding to ~320kbps CBR for highest quality
                default:
                    return LAMEPreset.STANDARD; // Fallback preset
            }
        }

        private void HandleFileDrop(string[] files)
        {
            var viewModel = DataContext as YourViewModel;
            if (viewModel == null) return;
            var firstadded = true;
            // Your existing logic for handling file drops
            foreach (string file in files)
            {
                // Add each file to your file list, for example: Assuming 'fileList' is your
                // ObservableCollection<string> that's bound to the UI
                if (viewModel.FileList.Any() && viewModel.FileList.All(f => f.FilePath != file) && firstadded == true)
                {
                    // Create a new instance of AddFilesDialog
                    var dialog = new AddFilesDialog();

                    // Set the main window as the owner of the dialog
                    dialog.Owner = this;

                    // Show the dialog and wait for the user to close it
                    dialog.ShowDialog();

                    // Check the result of the dialog
                    if (dialog.DialogResult == "NewList")
                    {
                        viewModel.FileList.Clear(); // Clear the existing list in the view model
                        firstadded = false;
                    }
                    else if (dialog.DialogResult == "Cancel")
                    {
                        return; // Cancel the operation and return from the event handler
                    }
                }
                viewModel.FileList.Add(new FileInfo { FilePath = file });
                firstadded = false;
            }
        }

        private void SetupBackgroundWorker()
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted; // Attach the completion event handler
        }


        private void UpdateStatus(string message)
        {
            // Invoke the following code on the UI thread
            Dispatcher.Invoke(() =>
            {
                // Set the status text to the given message
                statusText.Text = message;
            });
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Show copy cursor
            }
            else
            {
                e.Effects = DragDropEffects.None; // Show no-entry cursor
            }

            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            // Check if the drop is from an external source (Windows Explorer)
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var mp3Files = files.Where(file => System.IO.Path.GetExtension(file).Equals(".flac", StringComparison.OrdinalIgnoreCase)).ToArray();
                HandleFileDrop(mp3Files);
                e.Handled = true; // Mark the event as handled
            }
            else
            {
                // If the drop is not from an external source, it might be an internal drag-and-drop
                // for reordering. Do not set e.Handled to true here. This allows
                // GongSolutions.Wpf.DragDrop to handle internal reordering.
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Retrieve the data passed to the background worker
            //AnimateProgressBarToZero(); // Animate the progress bar to zero before starting the conversion
            dynamic data = e.Argument;
            string outputFolder = data.OutputFolder;
            List<string> flacFiles = data.FlacFiles as List<string>;
            int bitrate = ConvertBitrate(data.SelectedBitrate); // Assuming you pass 'SelectedBitrate' and have a method to convert it to an int

            if (flacFiles != null)
            {
                int totalFiles = flacFiles.Count;
                int processedFiles = 0;

                foreach (string flacFile in flacFiles)
                {
                    if (worker.CancellationPending)  // Check if cancellation was requested
                    {
                        e.Cancel = true;  // Cancel the operation
                        return;  // Exit the loop
                    }
                    string outputFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(flacFile) + ".mp3");
                    bool skipFile = false;  // Add this line
                    // Check if output file exists and if user previously chose not to "Overwrite All"
                    if (File.Exists(outputFile) && overwriteAll != true)
                    {
                        if (overwriteAll == null) // Only ask the user if they haven't already selected an option
                        {
                            Dispatcher.Invoke(() =>
                            {
                                var dialog = new CustomDialog($"File {outputFile} already exists. Do you want to overwrite?");
                                var result = dialog.ShowDialog();  // Shows your custom dialog

                                switch (dialog.Choice)
                                {
                                    case CustomDialog.UserChoice.Yes:
                                        // No action needed; continue to overwrite the current file.
                                        break;
                                    case CustomDialog.UserChoice.No:
                                        skipFile = true;  // Skip this file.
                                        break;
                                    case CustomDialog.UserChoice.YesToAll:
                                        overwriteAll = true;  // Overwrite all files without asking.
                                        break;
                                    case CustomDialog.UserChoice.NoToAll:
                                        overwriteAll = false;  // Skip all remaining files.
                                        break;
                                }
                            });


                        }

                        // If user chooses not to overwrite and did not select "Yes to All", skip this file
                        if (skipFile || (overwriteAll == false && File.Exists(outputFile)))
                        {
                            continue;  // Now, this continue works correctly because it's not inside a lambda
                        }

                    }

                    Dispatcher.Invoke(() => UpdateStatus($"Converting {Path.GetFileName(flacFile)}..."));

                    using (var reader = new AudioFileReader(flacFile)) // Ensure this is suitable for reading FLAC if they are FLAC files.
                    {
                        LAMEPreset preset = DeterminePresetFromBitrate(bitrate);
                        using (var writer = new LameMP3FileWriter(outputFile, reader.WaveFormat, preset))
                        {
                            writer.MinProgressTime = 250; // Minimum time between progress updates
                            writer.OnProgress += (s, inputBytes, outputBytes, finished) =>
                            {
                                int progressPercentage = (int)((double)inputBytes / reader.Length * 100);
                                worker.ReportProgress(progressPercentage);
                            };

                            reader.CopyTo(writer);
                        }
                    }
                    using (var reader = new AudioFileReader(flacFile))
                    {
                        // Use TagLib# to read tags from the original FLAC file
                        var tfile = TagLib.File.Create(flacFile);

                        // Now the MP3 file is written, apply tags using TagLib#
                        var mp3File = TagLib.File.Create(outputFile);

                        // Copy basic tags (extend this based on your needs and available tags)
                        mp3File.Tag.Title = tfile.Tag.Title;
                        mp3File.Tag.Album = tfile.Tag.Album;
                        mp3File.Tag.AlbumArtists = tfile.Tag.AlbumArtists;
                        mp3File.Tag.Performers = tfile.Tag.Performers;
                        mp3File.Tag.Composers = tfile.Tag.Composers;
                        mp3File.Tag.Genres = tfile.Tag.Genres;
                        mp3File.Tag.Year = tfile.Tag.Year;
                        mp3File.Tag.Track = tfile.Tag.Track;
                        mp3File.Tag.TrackCount = tfile.Tag.TrackCount;
                        mp3File.Tag.Disc = tfile.Tag.Disc;
                        mp3File.Tag.DiscCount = tfile.Tag.DiscCount;
                        mp3File.Tag.Lyrics = tfile.Tag.Lyrics;
                        mp3File.Tag.Comment = tfile.Tag.Comment;
                        mp3File.Tag.Conductor = tfile.Tag.Conductor;
                        mp3File.Tag.BeatsPerMinute = tfile.Tag.BeatsPerMinute;
                        mp3File.Tag.Grouping = tfile.Tag.Grouping;
                        mp3File.Tag.Publisher = tfile.Tag.Publisher;

                        // Copy embedded pictures (e.g., album art)
                        if (tfile.Tag.Pictures != null && tfile.Tag.Pictures.Length > 0)
                        {
                            mp3File.Tag.Pictures = tfile.Tag.Pictures;
                        }


                        // Save the MP3 tags
                        mp3File.Save();
                    }

                    processedFiles++;
                    int progressPercentage = (int)((double)processedFiles / totalFiles * 100);
                    worker.ReportProgress(progressPercentage);
                }

                // Update the status on the UI thread
                Dispatcher.Invoke(() => UpdateStatus("Conversion complete."));
            }
            overwriteAll = null; // Reset the overwrite flag
            AnimateProgressBarToZero();
        }
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // Update the UI to reflect that the operation was canceled
                Dispatcher.Invoke(() =>
                {
                    UpdateStatus("Conversion aborted."); // Update the status bar text
                    progressBar.Value = 0; // Reset the progress bar
                    AnimateProgressBarToZero(); // Alternatively, if you want it to animate to zero
                });
            }
            else if (e.Error != null)
            {
                // Handle any errors that occurred during the operation
                Dispatcher.Invoke(() =>
                {
                    UpdateStatus("Error occurred."); // Update status with error message
                    progressBar.Value = 0; // Reset the progress bar
                });
            }
            else
            {
                // Operation completed successfully
                Dispatcher.Invoke(() =>
                {
                    UpdateStatus("Conversion complete."); // Update status for successful completion
                    progressBar.Value = 100; // Optionally set the progress bar to full if not already
                    AnimateProgressBarToZero(); // Reset the progress bar after a brief pause to show completion
                });
            }
        }


        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Set the value of the progressBar control to the progress percentage provided in the
            // event arguments.
            progressBar.Value = e.ProgressPercentage;
        }

        #endregion Private Methods

        private void btnabort_Click(object sender, RoutedEventArgs e)
        {
            if (worker.IsBusy)
                worker.CancelAsync(); // Request cancellation
        }
    }
}