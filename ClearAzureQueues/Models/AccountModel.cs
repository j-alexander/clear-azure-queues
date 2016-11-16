﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace ClearAzureQueues.Models {

    public class AccountSettings {
        public string AccountName { get; set; }
        public string ConnectionString { get; set; }
    }

    public class AccountModel : DependencyObject {

        public AccountSettings Settings {
            get {
                return new AccountSettings() {
                    AccountName = AccountName,
                    ConnectionString = ConnectionString
                };
            }
        }

        public static readonly DependencyProperty AccountNameProperty =
            DependencyProperty.Register("AccountName", typeof(string), typeof(AccountModel));
        public string AccountName {
            get { return (string)GetValue(AccountNameProperty); }
            set { SetValue(AccountNameProperty, value); }
        }

        public static readonly DependencyProperty ConnectionStringProperty =
            DependencyProperty.Register("ConnectionString", typeof(string), typeof(AccountModel));
        public string ConnectionString {
            get { return (string)GetValue(ConnectionStringProperty); }
            set { SetValue(ConnectionStringProperty, value); }
        }

        public static readonly DependencyProperty ConnectionResultProperty =
            DependencyProperty.Register("ConnectionResult", typeof(string), typeof(AccountModel));
        public string ConnectionResult {
            get { return (string)GetValue(ConnectionResultProperty); }
            set { SetValue(ConnectionResultProperty, value); }
        }

        public static readonly DependencyProperty IsConnectingProperty =
            DependencyProperty.Register("IsConnecting", typeof(bool), typeof(AccountModel), new UIPropertyMetadata(false));
        public bool IsConnecting {
            get { return (bool)GetValue(IsConnectingProperty); }
            set { SetValue(IsConnectingProperty, value); }
        }

        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof(bool), typeof(AccountModel), new UIPropertyMetadata(false));
        public bool IsConnected {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        public CloudQueueClient Client { get; set; }

        public AccountModel() {
            if (Debugger.IsAttached) {
                AccountName = "dev";
                ConnectionString = "UseDevelopmentStorage=true;";
            }
        }

        public AccountModel(AccountSettings settings) {
            AccountName = settings.AccountName;
            ConnectionString = settings.ConnectionString;
        }

        public void Connect(Action onSuccess, Action onFailure) {
            if (!IsConnected && !IsConnecting) {
                IsConnecting = true;
                var worker = new BackgroundWorker();
                worker.DoWork += (sender, e) => {
                    var input = (string)e.Argument;
                    var account = CloudStorageAccount.Parse(input);
                    var client = account.CreateCloudQueueClient();
                    e.Result = client;
                };
                worker.RunWorkerCompleted += (sender, e) => {
                    IsConnecting = false;
                    if (e.Error != null) {
                        ConnectionResult = String.Format("Error: {0}", e.Error.Message);
                        IsConnected = false;
                        onFailure();
                    } else if (e.Cancelled) {
                        ConnectionResult = "You connection request was cancelled.";
                        IsConnected = false;
                        onFailure();
                    } else {
                        Client = (CloudQueueClient)e.Result;
                        ConnectionResult = null;
                        IsConnected = true;
                        onSuccess();
                    }
                };
                worker.RunWorkerAsync(ConnectionString);
            }
        }
    }
}
