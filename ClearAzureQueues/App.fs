namespace ClearAzureQueues

open System
open FsXaml

open ClearAzureQueues.Persistence
open ClearAzureQueues.Views
open ClearAzureQueues.ViewModels

type App = XAML<"App.xaml">

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module App =

    [<STAThread>]
    [<EntryPoint>]
    let main argv =
        let app = App()
        app.Startup.Add(fun _ ->
            let model = SettingsFile.Load()
            let window = new MainWindow()
            window.DataContext <- new MainViewModel(model)
            window.Closing.Add(fun _ -> SettingsFile.Save(model))
            app.MainWindow <- window
            app.MainWindow.Show())
        app.Run()