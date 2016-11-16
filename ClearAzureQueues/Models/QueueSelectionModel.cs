using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

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

        public QueueSelectionModel(AccountModel account) {
            Queues = new ObservableCollection<QueueModel>();
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
    }
}
