using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace ClearAzureQueues.Models {

    public class QueueSelectionModel : DependencyObject {

        public static readonly DependencyProperty AccountProperty =
            DependencyProperty.Register("Account", typeof(AccountModel), typeof(QueueSelectionModel));
        public AccountModel Account {
            get { return (AccountModel)GetValue(AccountProperty); }
            set { SetValue(AccountProperty, value); }
        }

        public static readonly DependencyProperty QueuesProperty =
            DependencyProperty.Register("Queues", typeof(ObservableCollection<QueueModel>), typeof(QueueSelectionModel));
        public ObservableCollection<QueueModel> Queues {
            get { return (ObservableCollection<QueueModel>)GetValue(QueuesProperty); }
            set { SetValue(QueuesProperty, value); }
        }

        public static readonly DependencyProperty FilteredQueuesProperty =
            DependencyProperty.Register("FilteredQueues", typeof(ListCollectionView), typeof(QueueSelectionModel));
        public ListCollectionView FilteredQueues {
            get { return (ListCollectionView)GetValue(FilteredQueuesProperty); }
            set { SetValue(FilteredQueuesProperty, value); }
        }

        public static readonly DependencyProperty NameFilterProperty =
            DependencyProperty.Register("NameFilter", typeof(string), typeof(QueueSelectionModel),
                new FrameworkPropertyMetadata((sender,e) => {
                    var model = sender as QueueSelectionModel;
                    if (model != null) model.Filter();
                }));
        public string NameFilter {
            get { return (string)GetValue(NameFilterProperty); }
            set { SetValue(NameFilterProperty, value); }
        }

        public QueueSelectionModel(AccountModel account) {
            Queues = new ObservableCollection<QueueModel>();
            FilteredQueues = new ListCollectionView(Queues);
            Account = account;
            Populate();
        }

        public void Populate() {
            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) => {
                var client = (CloudQueueClient)e.Argument;
                foreach (var queue in client.ListQueues()) {
                    worker.ReportProgress(0, queue);
                }
            };
            worker.ProgressChanged += (sender, e) => {
                var queue = (CloudQueue)e.UserState;
                foreach (var existing in Queues)
                    if (existing.QueueName == queue.Name)
                        return;
                Queues.Add(new QueueModel(queue));
            };
            worker.RunWorkerCompleted += (sender, e) => {
            };
            worker.RunWorkerAsync(Account.Client);
        }

        private void Filter() {
            if (String.IsNullOrEmpty(NameFilter)) {
                FilteredQueues.Filter = null;
            } else {
                FilteredQueues.Filter = new Predicate<object>(x => {
                    var model = x as QueueModel;
                    return
                        model != null && 
                        model.QueueName != null &&
                        model.QueueName.Contains(NameFilter);
                });
            }
        }
    }
}
