using System.Windows;
using System.Xml.Linq;
using WpfApp4.ViewModel;

namespace WpfApp4.ViewModel
{
    public partial class FolderNameWindow : Window
    {
        public string FolderName { get; private set; }

        public FolderNameWindow()
        {
            InitializeComponent();
            TxtName.Focus();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            FolderName = TxtName.Text?.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}