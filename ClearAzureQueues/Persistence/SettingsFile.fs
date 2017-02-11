namespace ClearAzureQueues.Persistence

open System
open System.IO
open Newtonsoft.Json

open ClearAzureQueues.Settings
open ClearAzureQueues.Models

type SettingsFile() =

    static let file =
        let application = typeof<SettingsFile>.Assembly.GetName().Name
        let environment = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        let folder = Path.Combine(environment, application)
        if not (Directory.Exists(folder)) then
            Directory.CreateDirectory(folder)
            |> ignore
        Path.Combine(folder, "settings.json")

    static member public Load() =
        try
            let json = File.ReadAllText(file)
            let settings = JsonConvert.DeserializeObject<AccountSelectionSettings>(json)
            new AccountSelectionModel(settings)
        with e ->
            new AccountSelectionModel()

    static member public Save(model:AccountSelectionModel) =
        try
            let settings = model.Settings
            let json = JsonConvert.SerializeObject(settings,Formatting.Indented)
            //File.WriteAllText(file, json)
            File.WriteAllText(@"C:\Users\Jonathan\Desktop\settings.json", json)
        with _ -> ()