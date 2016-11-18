using ClearAzureQueues.Settings;
using ClearAzureQueues.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ClearAzureQueues.Views {

    public partial class MainWindow : Window {

        public MainViewModel ViewModel { get; set; }

        public MainWindow() {
            InitializeComponent();
            
            var model = FileSettings.Load();
            Closing += (s, e) => FileSettings.Save(model);
            
            DataContext = ViewModel = new MainViewModel() { Model = model };
        }

        private void AccountSelection_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ViewModel.ConnectToSelectedAccount();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }
    }
}
