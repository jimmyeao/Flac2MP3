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
    /// Interaction logic for CustomDialog.xaml
    /// </summary>
    public partial class CustomDialog : Window
    {
        public enum UserChoice
        {
            Yes,
            No,
            YesToAll,
            NoToAll
        }

        public UserChoice Choice { get; private set; } = UserChoice.No; // Default choice

        public CustomDialog(string message)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Choice = UserChoice.Yes;
            this.DialogResult = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Choice = UserChoice.No;
            this.DialogResult = false;
        }

        private void YesToAllButton_Click(object sender, RoutedEventArgs e)
        {
            Choice = UserChoice.YesToAll;
            this.DialogResult = true;
        }

        private void NoToAllButton_Click(object sender, RoutedEventArgs e)
        {
            Choice = UserChoice.NoToAll;
            this.DialogResult = false;
        }
    }

}
