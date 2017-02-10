namespace ClearAzureQueues.Models

open System.Collections.ObjectModel
open System.Linq
open System.Windows

open ClearAzureQueues.Settings

type AccountSelectionModel() =
    inherit DependencyObject()

    let accounts = new ObservableCollection<QueueSelectionModel>()

    static let selectedAccount =
        DependencyProperty.Register("SelectedAccount", typeof<QueueSelectionModel>, typeof<AccountSelectionModel>)

    new { Accounts = accounts } as x =
        AccountSelectionModel() then
            for account in accounts do
                x.Accounts.Add(new QueueSelectionModel(account))

    member public x.Settings =
        { Accounts = x.Accounts |> Seq.map (fun x -> x.Settings) |> Seq.toArray }

    member public x.SelectedAccount
        with get() = x.GetValue(selectedAccount) :?> QueueSelectionModel
        and set(value:QueueSelectionModel) = x.SetValue(selectedAccount, value)

    member public x.Accounts : ObservableCollection<QueueSelectionModel> = accounts

