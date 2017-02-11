namespace ClearAzureQueues.ViewModels

open System
open System.Linq
open System.Windows
open System.Windows.Input
open System.Windows.Threading

open ClearAzureQueues.Commands
open ClearAzureQueues.Models
open ClearAzureQueues.Views
open ClearAzureQueues.Persistence

type MainViewModel() =

    let model = SettingsFile.Load()
    
    let selectedAccount _ =
        model.SelectedAccount
        |> Option.ofNull
    let selectedQueue _ =
        model.SelectedAccount
        |> Option.ofNull
        |> Option.map(fun x -> x.SelectedQueue)
        |> Option.bind Option.ofNull

    let isUnselected _ = selectedAccount() |> Option.isNone
    let isSelected _ = selectedAccount() |> Option.isSome
    let ifSelected fn = selectedAccount >> Option.iter fn

    let timer = 
        let timer = new DispatcherTimer()
        timer.Interval <- TimeSpan.FromSeconds 30.
        timer.Tick.Add(ifSelected(fun selection ->
            for queue in selection.Queues do
                queue.UpdateStatus()))
        timer.Start()
        timer

    let connectToSelectedAccount() =
        ifSelected(fun selection ->
            selection.Account.Connect(selection.Populate, ignore))

    let newAccount =
        new RelayCommand(
            fun _ -> true
            ,
            fun x ->
                let dialog = new AccountWindow()
                dialog.Owner <- x :?> Window
                let result = dialog.ShowDialog()
                if result.HasValue && result.Value then
                    let accountViewModel = dialog.DataContext :?> AccountViewModel
                    let accountModel = accountViewModel.Model
                    let queueSelectionModel = new QueueSelectionModel(accountModel)
                    model.Accounts.Add(queueSelectionModel)
                    if isUnselected() then
                        model.SelectedAccount <- queueSelectionModel)

    let removeAccount =
        new RelayCommand(
            isSelected
            ,
            ifSelected(fun selection ->
                let prompt = sprintf "Removing account '%s': are you sure?" selection.Account.AccountName
                let result = MessageBox.Show(prompt, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                if result = MessageBoxResult.Yes then
                    for queue in selection.Queues do
                        queue.Cancel()
                    model.Accounts.Remove(selection)
                    |> ignore))

    let moveAccountUp =
        new RelayCommand(
            isSelected
            ,
            ifSelected(fun selection ->
                let i = model.Accounts.IndexOf(selection)
                if (i > 0) then
                    model.Accounts.RemoveAt(i)
                    model.Accounts.Insert(i-1, selection)
                    model.SelectedAccount <- selection))

    let moveAccountDown =
        new RelayCommand(
            isSelected
            ,
            ifSelected(fun selection ->
                let i = model.Accounts.IndexOf(selection)
                if i >= 0 && i + 1 < model.Accounts.Count then
                    model.Accounts.RemoveAt(i)
                    model.Accounts.Insert(i-1, selection)
                    model.SelectedAccount <- selection))

    let erase =
        new RelayCommand(
            selectedQueue
            >> Option.map(fun queue -> not queue.IsExecuting)
            >> Option.getValueOr false
            ,
            selectedQueue
            >> Option.iter(fun queue -> queue.Erase()))

    let abort =
        new RelayCommand(
            selectedQueue
            >> Option.map(fun queue -> queue.IsExecuting)
            >> Option.getValueOr false
            ,
            selectedQueue
            >> Option.iter(fun queue -> queue.Cancel()))

    let abortAll =
        new RelayCommand(
            fun _ ->
                model.Accounts
                |> Seq.collect (fun x -> x.Queues)
                |> Seq.exists (fun x -> x.IsExecuting)
            ,
            fun _ ->
                model.Accounts
                |> Seq.filter (fun x -> x.Account.IsConnected)
                |> Seq.collect (fun x -> x.Queues)
                |> Seq.filter (fun x -> x.IsExecuting)
                |> Seq.iter (fun x -> x.Cancel()))