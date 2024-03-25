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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CompletionDialog : Window
    {
        #region Public Constructors

        public CompletionDialog()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Private Methods

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion Private Methods
    }
}
