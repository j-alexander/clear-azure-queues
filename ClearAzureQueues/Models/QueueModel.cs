using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace ClearAzureQueues.Models {

    public class QueueModel : DependencyObject {

        public static readonly DependencyProperty QueueNameProperty =
            DependencyProperty.Register("QueueName", typeof(string), typeof(QueueModel));
        public string QueueName {
            get { return (string)GetValue(QueueNameProperty); }
            set { SetValue(QueueNameProperty, value); }
        }

        public static readonly DependencyProperty MessagesProperty =
            DependencyProperty.Register("Messages", typeof(int), typeof(QueueModel));
        public int Messages {
            get { return (int)GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }

        public static readonly DependencyProperty IsExecutingProperty =
            DependencyProperty.Register("IsExecuting", typeof(Boolean), typeof(QueueModel), new UIPropertyMetadata(false));
        public bool IsExecuting {
            get { return (bool)GetValue(IsExecutingProperty); }
            set { SetValue(IsExecutingProperty, value); }
        }

        public static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.Register("IsUpdating", typeof(Boolean), typeof(QueueModel), new UIPropertyMetadata(false));
        public bool IsUpdating {
            get { return (bool)GetValue(IsUpdatingProperty); }
            set { SetValue(IsUpdatingProperty, value); }
        }

        public CloudQueue Queue { get; set; }

        public bool IsCancelling { get; set; }


        public QueueModel(CloudQueue queue) {
            Queue = queue;
            UpdateStatus();
        }

        public void UpdateStatus() {
            if (!IsUpdating && !IsExecuting) {
                IsUpdating = true;
                QueueName = Queue.Name;

                var worker = new BackgroundWorker();
                worker.DoWork += (sender, e) => {
                    var queue = (CloudQueue)e.Argument;
                    queue.FetchAttributes();
                    e.Result = queue.ApproximateMessageCount;
                };
                worker.RunWorkerCompleted += (sender, e) => {
                    IsUpdating = false;
                    if (e.Error != null) return;
                    if (e.Cancelled) return;
                    if (e.Result != null) Messages = (int)e.Result;
                };
                worker.RunWorkerAsync(Queue);
            }
        }

        public void Erase() {
            IsExecuting = true;
            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (sender, e) => {
                var watch = Stopwatch.StartNew();
                var queue = (CloudQueue)e.Argument;
                while (!IsCancelling) {
                    queue.FetchAttributes();
                    if (watch.Elapsed > TimeSpan.FromSeconds(1)) {
                        var count = queue.ApproximateMessageCount;
                        if (count != null) {
                            worker.ReportProgress(0, count);
                            watch.Restart();
                        }
                    }
                    try {
                        queue.Clear(new QueueRequestOptions() {
                            MaximumExecutionTime = TimeSpan.FromSeconds(3)
                        });
                    } catch (Exception) { Thread.Sleep(1000); }
                }
            };
            worker.ProgressChanged += (sender, e) => {
                if (e.UserState != null) {
                    Messages = (int)e.UserState;
                }
            };
            worker.RunWorkerCompleted += (sender, e) => {
                IsCancelling = false;
                IsExecuting = false;
            };
            worker.RunWorkerAsync(Queue);
        }

        public void Cancel() {
            IsCancelling = true;
        }
    }
}
