using ClearAzureQueues.Models;
using System;
using System.Windows.Input;

namespace ClearAzureQueues.ViewModels {

    public class AccountViewModel {

        public AccountModel Model { get; set; }

        public ICommand Connect { get; set; }

        public AccountViewModel(Action onSuccess, Action onFailure) {
            Model = new AccountModel();
            Connect = new RelayCommand(
                x => !Model.IsConnecting,
                x => Model.Connect(onSuccess, onFailure));
        }
    }
}
