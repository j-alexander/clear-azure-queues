namespace ClearAzureQueues.Models

open System
open System.Collections.ObjectModel
open System.ComponentModel
open System.Linq
open System.Windows
open System.Windows.Data
open Microsoft.WindowsAzure.Storage.Queue

open ClearAzureQueues
open ClearAzureQueues.Settings

[<AllowNullLiteral>]
type QueueSelectionModel(account:AccountModel) =
    inherit DependencyObject()

    let queues = new ObservableCollection<QueueModel>()
    let filteredQueues = new ListCollectionView(queues)

    let populate() =
        match account.Client with
        | Some client when account.IsConnected ->
            let worker = new BackgroundWorker()
            worker.WorkerReportsProgress <- true
            worker.DoWork.Add(fun e ->
                for queue in client.ListQueues() do
                    worker.ReportProgress(0, queue))
            worker.ProgressChanged.Add(fun e ->
                let queue = e.UserState :?> CloudQueue
                let exists =
                    queues
                    |> Seq.exists (fun x -> x.QueueName = queue.Name)
                if not exists then
                    let queueModel = new QueueModel(queue)
                    queueModel.UpdateStatus()
                    queues.Add(queueModel))
            worker.RunWorkerAsync()
        | _ -> ()

    do populate()

    static let selectedQueue =
        DependencyProperty.Register("SelectedQueue", typeof<QueueModel>, typeof<QueueSelectionModel>)
    static let nameFilter =
        DependencyProperty.Register("NameFilter", typeof<string>, typeof<QueueSelectionModel>,
            new FrameworkPropertyMetadata(new PropertyChangedCallback(fun (sender:DependencyObject) _ ->
                match sender with
                | :? QueueSelectionModel as model -> model.Filter()
                | _ -> ())))

    new { Account = account
          NameFilter = nameFilter } as x =
        QueueSelectionModel(new AccountModel(account)) then
            x.NameFilter <- nameFilter

    member public x.Settings =
        { Account = account.Settings
          NameFilter = x.NameFilter }

    member public x.NameFilter
        with get() = x.GetValue(nameFilter) :?> string
        and set(value:string) = x.SetValue(nameFilter, value)

    member public x.SelectedQueue
        with get() = match x.GetValue(selectedQueue) with :? QueueModel as m -> m | _ -> null
        and set(value:QueueModel) = x.SetValue(selectedQueue, value)

    member public x.Account = account

    member public x.Queues = queues

    member public x.FilteredQueues = filteredQueues

    member public x.Populate() = populate()

    member public x.Filter() =
        if String.IsNullOrWhiteSpace(x.NameFilter) then
            x.FilteredQueues.Filter <- null
        else
            let matches : string->bool =
                let terms = x.NameFilter.Split(' ')
                function
                | x when String.IsNullOrWhiteSpace(x) -> false
                | x -> Seq.forall x.Contains terms
            x.FilteredQueues.Filter <-
                new Predicate<obj>(function
                    | :? QueueModel as model -> matches model.QueueName
                    | _ -> false)