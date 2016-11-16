using ClearAzureQueues.Models;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ClearAzureQueues {

    public partial class MainWindow : Window {

        public AccountSelectionModel Model {
            get { return (AccountSelectionModel)DataContext; }
        }

        public MainWindow() {
            InitializeComponent();
            DataContext = new AccountSelectionModel();

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            foreach (QueueModel item in QueueSelection.Items) {
                item.UpdateStatus();
            }
        }

        private void NewAccount_Click(object sender, RoutedEventArgs e) {
            var newAccount = new AccountWindow();
            newAccount.Owner = this;

            var result = newAccount.ShowDialog();
            if (result == true) {
                var model = new QueueSelectionModel(newAccount.Model);
                Model.Accounts.Add(model);
                if (AccountSelection.SelectedItem == null) {
                    AccountSelection.SelectedItem = model;
                }
            }
        }

        private void Execute_Click(object sender, RoutedEventArgs e) {
            var model = QueueSelection.SelectedItem as QueueModel;
            if (model != null) {
                model.Erase();
            }
        }

        private void Abort_Click(object sender, RoutedEventArgs e) {
            var model = QueueSelection.SelectedItem as QueueModel;
            if (model != null) {
                model.Cancel();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }
    }
}
