using ClearAzureQueues.Models;
using System;
using System.Windows;

namespace ClearAzureQueues {

    public partial class AccountWindow : Window {

        public AccountModel Model {
            get { return (AccountModel)DataContext; }
        }

        public AccountWindow() {
            InitializeComponent();
            DataContext = new AccountModel();
        }

        private void connect_Click(object sender, RoutedEventArgs e) {
            Action onSuccess = () => {
                DialogResult = true;
                Close();
            };
            Action onFailure = () => {
            };
            Model.Connect(onSuccess, onFailure);
        }
    }
}
