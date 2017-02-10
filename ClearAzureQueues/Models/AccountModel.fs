namespace ClearAzureQueues.Models

open System
open System.ComponentModel
open System.Diagnostics
open System.Windows
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Queue

open ClearAzureQueues.Settings

type AccountModel() =
    inherit DependencyObject()

    let client : ref<CloudQueueClient option> = ref None

    static let defaultAccountName, defaultConnectionString =
        if Debugger.IsAttached then "dev", "UseDevelopmentStorage=true;" else "", ""

    static let accountName =
        DependencyProperty.Register("AccountName", typeof<string>, typeof<AccountModel>, new PropertyMetadata(defaultAccountName))
    static let connectionString =
        DependencyProperty.Register("ConnectionString", typeof<string>, typeof<AccountModel>, new PropertyMetadata(defaultConnectionString))
    static let connectionResult =
        DependencyProperty.Register("ConnectionResult", typeof<string>, typeof<AccountModel>)
    static let isConnecting =
        DependencyProperty.Register("IsConnecting", typeof<bool>, typeof<AccountModel>)
    static let isConnected =
        DependencyProperty.Register("IsConnected", typeof<bool>, typeof<AccountModel>)

    new { AccountName = accountName
          ConnectionString = connectionString } as x =
        AccountModel() 
        then 
            x.AccountName <- accountName
            x.ConnectionString <- connectionString

    member public x.Settings =
        { AccountName = x.AccountName
          ConnectionString = x.ConnectionString }

    member public x.AccountName
        with get() = x.GetValue(accountName) :?> string
        and set(value:string) = x.SetValue(accountName, value)

    member public x.ConnectionString
        with get() = x.GetValue(connectionString) :?> string
        and set(value:string) = x.SetValue(connectionString, value)

    member public x.ConnectionResult
        with get() = x.GetValue(connectionResult) :?> string
        and set(value:string) = x.SetValue(connectionResult, value)

    member public x.IsConnecting
        with get() = x.GetValue(isConnecting) :?> bool
        and set(value:bool) = x.SetValue(isConnecting, value)

    member public x.IsConnected
        with get() = x.GetValue(isConnected) :?> bool
        and set(value:bool) = x.SetValue(isConnected, value)

    member public x.Client = !client

    member public x.Connect(onSuccess, onFailure) =
        if not x.IsConnecting && not x.IsConnecting then
            x.IsConnecting <- true

            let worker = new BackgroundWorker()
            worker.DoWork.Add(fun e ->
                let input = e.Argument :?> string
                let account = CloudStorageAccount.Parse(input)
                let client = account.CreateCloudQueueClient()
                e.Result <- client)
            worker.RunWorkerCompleted.Add(fun e ->
                x.IsConnecting <- false
                if not(isNull e.Error) then
                    x.ConnectionResult <- sprintf "Error: %s" e.Error.Message
                    x.IsConnected <- false
                    onFailure()
                elif e.Cancelled then
                    x.ConnectionResult <- "Your connection request was cancelled."
                    x.IsConnected <- false
                    onFailure()
                else
                    client := Some(e.Result :?> CloudQueueClient)
                    x.ConnectionResult <- null
                    x.IsConnected <- true
                    onSuccess())
            worker.RunWorkerAsync(x.ConnectionString)