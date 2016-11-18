using ClearAzureQueues.Models;
using ClearAzureQueues.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ClearAzureQueues.ViewModels {

    public class MainViewModel {

        public AccountSelectionModel Model { get; set; }

        public DispatcherTimer Timer { get; set; }

        public Action ConnectToSelectedAccount { get; set; }

        public ICommand NewAccount { get; set; }

        public ICommand RemoveAccount { get; set; }

        public ICommand MoveAccountUp { get; set; }

        public ICommand MoveAccountDown { get; set; }

        public ICommand EraseQueue { get; set; }

        public ICommand Abort { get; set; }

        public ICommand AbortAll { get; set; }

        public MainViewModel() {

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(30);
            Timer.Tick += (s,e) => {
                var account = Model?.SelectedAccount;
                if (account != null) {
                    foreach (QueueModel queue in account.Queues) {
                        queue.UpdateStatus();
                    }
                }
            };
            Timer.Start();

            ConnectToSelectedAccount = () => {
                var model = Model?.SelectedAccount;
                if (model != null) {
                    model.Account.Connect(model.Populate, () => { });
                }
            };

            NewAccount = new RelayCommand(
                x => true,
                x => {
                    var newAccount = new AccountWindow();
                    newAccount.Owner = x as Window;

                    var result = newAccount.ShowDialog();
                    if (result == true) {
                        var viewModel = newAccount.DataContext as AccountViewModel;
                        var model = new QueueSelectionModel(viewModel.Model);
                        Model.Accounts.Add(model);
                        if (Model.SelectedAccount == null) {
                            Model.SelectedAccount = model;
                        }
                    }
                });

            RemoveAccount = new RelayCommand(
                x => Model?.SelectedAccount != null,
                x => {
                    var model = Model.SelectedAccount;
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
                });

            MoveAccountUp = new RelayCommand(
                x => Model?.SelectedAccount != null,
                x => {
                    var i = Model.Accounts.IndexOf(Model.SelectedAccount);
                    if (i > 0) {
                        var model = Model.Accounts[i];
                        Model.Accounts.RemoveAt(i);
                        Model.Accounts.Insert(i - 1, model);
                        Model.SelectedAccount = model;
                    }
                });

            MoveAccountDown = new RelayCommand(
                x => Model?.SelectedAccount != null,
                x => {
                    var i = Model.Accounts.IndexOf(Model.SelectedAccount);
                    if (i >= 0 && i + 1 < Model.Accounts.Count) {
                        var model = Model.Accounts[i];
                        Model.Accounts.RemoveAt(i);
                        Model.Accounts.Insert(i + 1, model);
                        Model.SelectedAccount = model;
                    }
                });

            EraseQueue = new RelayCommand(
                x => !(Model?.SelectedAccount?.SelectedQueue?.IsExecuting ?? true),
                x => Model.SelectedAccount.SelectedQueue.Erase());
            
            Abort = new RelayCommand(
                x => Model?.SelectedAccount?.SelectedQueue?.IsExecuting ?? false,
                x => Model.SelectedAccount.SelectedQueue.Cancel());

            AbortAll = new RelayCommand(
                x => true,
                x => {
                    var accounts = Model?.Accounts;
                    if (accounts != null) {
                        var queues =
                            accounts.Where(model => model.Account.IsConnected)
                                    .SelectMany(account => account.Queues);
                        foreach (var queue in queues) {
                            queue.Cancel();
                        }
                    }
                });
        }
    }
}
