namespace ClearAzureQueues.ViewModels

open System
open System.Windows.Input

open ClearAzureQueues.Commands
open ClearAzureQueues.Models

type AccountViewModel(onSuccess, onFailure) =
    
    let model = new AccountModel()
    let connect =
        new RelayCommand(
            (fun _ -> not model.IsConnecting),
            (fun _ -> model.Connect(onSuccess, onFailure)))

    member public x.Model = model
    member public x.Connect = connect