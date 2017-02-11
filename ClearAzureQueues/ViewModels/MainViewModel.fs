namespace ClearAzureQueues.ViewModels

open System
open System.Diagnostics
open System.Linq
open System.Windows
open System.Windows.Input
open System.Windows.Threading

open ClearAzureQueues.Commands
open ClearAzureQueues.Models
open ClearAzureQueues.Views
open ClearAzureQueues.Persistence

type MainViewModel(model:AccountSelectionModel) =
    
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

    let connect =
        new RelayCommand(
            ifSelected(fun selection ->
                selection.Account.Connect(selection.Populate, ignore)))

    let newAccount =
        new RelayCommand(
            fun x ->
                let dialog = new AccountWindow()
                dialog.Owner <- x :?> Window
                dialog.DataContext <- 
                    let onSuccess() = if dialog.IsVisible then dialog.DialogResult <- Nullable(true)
                    let onFailure() = ()
                    new AccountViewModel(onSuccess, onFailure)
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
            selectedAccount
            >> Option.map(model.Accounts.IndexOf >> (<) 0)
            >> Option.getValueOr false
            ,
            ifSelected(fun selection ->
                let i = model.Accounts.IndexOf(selection)
                if (i > 0) then
                    model.Accounts.RemoveAt(i)
                    model.Accounts.Insert(i-1, selection)
                    model.SelectedAccount <- selection))

    let moveAccountDown =
        new RelayCommand(
            selectedAccount
            >> Option.map(fun selection ->
                let i = model.Accounts.IndexOf(selection)
                i >= 0 && i + 1 < model.Accounts.Count)
            >> Option.getValueOr false
            ,
            ifSelected(fun selection ->
                let i = model.Accounts.IndexOf(selection)
                if i >= 0 && i + 1 < model.Accounts.Count then
                    model.Accounts.RemoveAt(i)
                    model.Accounts.Insert(i+1, selection)
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

    let browse =
        new RelayCommand(fun _ ->
            @"https://github.com/j-alexander/clear-azure-queues"
            |> Process.Start
            |> ignore)

    member public x.Model = model
    member public x.Timer = timer
    member public x.Connect = connect
    member public x.NewAccount = newAccount
    member public x.RemoveAccount = removeAccount
    member public x.MoveAccountUp = moveAccountUp
    member public x.MoveAccountDown = moveAccountDown
    member public x.EraseQueue = erase
    member public x.Abort = abort
    member public x.AbortAll = abortAll
    member public x.Browse = browse