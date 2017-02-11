namespace UI

open System
open FsXaml

type App = XAML<"App.xaml">

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module App =

    [<STAThread>]
    [<EntryPoint>]
    let main argv =
        0