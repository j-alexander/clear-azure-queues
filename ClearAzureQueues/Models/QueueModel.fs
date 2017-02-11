namespace ClearAzureQueues.Models

open System
open System.ComponentModel
open System.Diagnostics
open System.Threading
open System.Windows
open System.Windows.Input
open Microsoft.WindowsAzure.Storage.Queue

open ClearAzureQueues.Settings

[<AllowNullLiteral>]
type QueueModel(queue:CloudQueue) =
    inherit DependencyObject()

    let isCancelling = ref false

    static let queueName =
        DependencyProperty.Register("QueueName", typeof<string>, typeof<QueueModel>)
    static let messages =
        DependencyProperty.Register("Messages", typeof<int>, typeof<QueueModel>)
    static let isExecuting =
        DependencyProperty.Register("IsExecuting", typeof<bool>, typeof<QueueModel>)
    static let isUpdating =
        DependencyProperty.Register("IsUpdating", typeof<bool>, typeof<QueueModel>)

    member public x.QueueName
        with get() = x.GetValue(queueName) :?> string
        and set(value:string) = x.SetValue(queueName, value)

    member public x.Messages
        with get() = x.GetValue(messages) :?> int
        and set(value:int) = x.SetValue(messages, value)

    member public x.IsExecuting
        with get() = x.GetValue(isExecuting) :?> bool
        and set(value:bool) = x.SetValue(isExecuting, value)

    member public x.IsUpdating
        with get() = x.GetValue(isUpdating) :?> bool
        and set(value:bool) = x.SetValue(isUpdating, value)

    member public x.IsCancelling
        with get() = !isCancelling
        and set(value:bool) = isCancelling := value
    
    member public x.Queue = queue

    member public x.UpdateStatus() =
        if (not x.IsUpdating && not x.IsExecuting) then
            x.IsUpdating <- true
            x.QueueName <- queue.Name

            let worker = new BackgroundWorker()
            worker.DoWork.Add(fun e ->
                queue.FetchAttributes()
                e.Result <- queue.ApproximateMessageCount)
            worker.RunWorkerCompleted.Add(fun e ->
                x.IsUpdating <- false
                if isNull e.Error && not e.Cancelled then
                    x.Messages <- e.Result :?> int)
            worker.RunWorkerAsync()

    member public x.Erase() =
        x.IsExecuting <- true
        let worker = new BackgroundWorker()
        worker.WorkerReportsProgress <- true
        worker.WorkerSupportsCancellation <- true
        worker.DoWork.Add(fun e ->
            let watch = Stopwatch.StartNew()
            while not x.IsCancelling do
                if watch.Elapsed > TimeSpan.FromSeconds 1. then
                    queue.FetchAttributes()
                    worker.ReportProgress(0, queue.ApproximateMessageCount)
                try queue.Clear(new QueueRequestOptions(MaximumExecutionTime=Nullable(TimeSpan.FromSeconds 3.)))
                with e -> Thread.Sleep(1000))
        worker.ProgressChanged.Add(fun e ->
            match e.UserState with
            | :? Nullable<int> as messages when messages.HasValue ->
                x.Messages <- messages.Value
            | _ -> ())
        worker.RunWorkerCompleted.Add(fun e ->
            x.IsCancelling <- false
            x.IsExecuting <- false
            CommandManager.InvalidateRequerySuggested())
        worker.RunWorkerAsync()

    member public x.Cancel() =
        if x.IsExecuting then
            x.IsCancelling <- true


