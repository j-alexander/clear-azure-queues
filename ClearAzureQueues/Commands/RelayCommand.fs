namespace ClearAzureQueues.Commands

open System
open System.Windows.Input

type RelayCommand(canExecute, execute) =

    new (execute) = RelayCommand((fun _ -> true), execute)

    interface ICommand with
        
        [<CLIEvent>]
        member x.CanExecuteChanged = CommandManager.RequerySuggested
        member x.CanExecute(parameter) = canExecute parameter
        member x.Execute(parameter) = execute parameter