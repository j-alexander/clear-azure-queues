using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

namespace ClearAzureQueues.Models {

    public class AccountSelectionSettings {
        public QueueSelectionSettings[] Accounts { get; set; }
    }

    public class AccountSelectionModel : DependencyObject {

        public AccountSelectionSettings Settings {
            get {
                return new AccountSelectionSettings() {
                    Accounts = Accounts.Select(x => x.Settings).ToArray()
                };
            }
        }

        public static readonly DependencyProperty AccountsProperty =
            DependencyProperty.Register("Accounts", typeof(ObservableCollection<QueueSelectionModel>), typeof(AccountSelectionModel));
        public ObservableCollection<QueueSelectionModel> Accounts {
            get { return (ObservableCollection<QueueSelectionModel>)GetValue(AccountsProperty); }
            set { SetValue(AccountsProperty, value); }
        }

        public AccountSelectionModel() {
            Accounts = new ObservableCollection<QueueSelectionModel>();
        }

        public AccountSelectionModel(AccountSelectionSettings settings) {
            Accounts = new ObservableCollection<QueueSelectionModel>(
                settings.Accounts.Select(x => new QueueSelectionModel(x)));
        }
    }
}
