using System.Collections.ObjectModel;
using System.Windows;

namespace ClearAzureQueues.Models {

    public class AccountSelectionModel : DependencyObject {

        public static readonly DependencyProperty AccountsProperty =
            DependencyProperty.Register("Accounts", typeof(ObservableCollection<QueueSelectionModel>), typeof(AccountSelectionModel));
        public ObservableCollection<QueueSelectionModel> Accounts {
            get { return (ObservableCollection<QueueSelectionModel>)GetValue(AccountsProperty); }
            set { SetValue(AccountsProperty, value); }
        }

        public AccountSelectionModel() {
            Accounts = new ObservableCollection<QueueSelectionModel>();
        }
    }
}
