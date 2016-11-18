using ClearAzureQueues.Models;
using ClearAzureQueues.ViewModels;
using System;
using System.Windows;

namespace ClearAzureQueues.Views {

    public partial class AccountWindow : Window {

        public AccountWindow() {
            InitializeComponent();

            Action onSuccess = () => { if (IsVisible) DialogResult = true; };
            Action onFailure = () => {};
            DataContext = new AccountViewModel(onSuccess, onFailure);
        }
    }
}
