using ClearAzureQueues.Models;
using ClearAzureQueues.Settings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ClearAzureQueues {

    public partial class MainWindow : Window {

        public AccountSelectionModel Model {
            get { return (AccountSelectionModel)DataContext; }
        }

        public MainWindow() {
            InitializeComponent();
            DataContext = FileSettings.Load();
            Closing += (s, e) => FileSettings.Save(Model);

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

        private void AccountSelection_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var model = AccountSelection.SelectedItem as QueueSelectionModel;
            if (model != null) {
                model.Account.Connect(model.Populate, () => { });
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

        private void AbortAll_Click(object sender, RoutedEventArgs e) {
            var queues = Model.Accounts.Where(x => x.Account.IsConnected).SelectMany(x => x.Queues);
            foreach (var queue in queues) {
                queue.Cancel();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }

        private void MoveAccountUp_Click(object sender, RoutedEventArgs e) {
            var i = AccountSelection.SelectedIndex;
            if (i > 0) {
                var model = Model.Accounts[i];
                Model.Accounts.RemoveAt(i);
                Model.Accounts.Insert(i - 1, model);
                AccountSelection.SelectedIndex = i - 1;
            }
        }

        private void MoveAccountDown_Click(object sender, RoutedEventArgs e) {
            var i = AccountSelection.SelectedIndex;
            if (i >= 0 && i + 1 < Model.Accounts.Count) {
                var model = Model.Accounts[i];
                Model.Accounts.RemoveAt(i);
                Model.Accounts.Insert(i + 1, model);
                AccountSelection.SelectedIndex = i + 1;
            }
        }

        private void RemoveAccount_Click(object sender, RoutedEventArgs e) {
            var model = AccountSelection.SelectedItem as QueueSelectionModel;
            if (model != null) {
                var prompt = String.Format("Removing account '{0}': are you sure?", model.Account.AccountName);
                var result = MessageBox.Show(prompt, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes) {
                    foreach (var queue in model.Queues) {
                        queue.Cancel();
                    }
                    Model.Accounts.Remove(model);
                }
            }
        }
    }
}
