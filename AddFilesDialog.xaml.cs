using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Flac2MP3
{
    /// <summary>
    /// Interaction logic for AddFilesDialog.xaml
    /// </summary>
    public partial class AddFilesDialog : Window
    {
        public AddFilesDialog()
        {
            InitializeComponent();
        }

        public string DialogResult { get; private set; } = "Cancel";

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = "Add";
            this.Close();
        }

        private void btnNewList_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = "NewList";
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = "Cancel";
            this.Close();
        }
    }
}
