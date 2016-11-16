using ClearAzureQueues.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ClearAzureQueues.Settings {

    public static class FileSettings {

        public static string FileName {
            get {
                var application = typeof(FileSettings).Assembly.GetName().Name;
                var environment = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var folder = Path.Combine(environment, application);
                if (Directory.Exists(folder) == false) {
                    Directory.CreateDirectory(folder);
                }
                return Path.Combine(folder, "settings.json");
            }
        }

        public static AccountSelectionModel Load() {
            try {
                var json = File.ReadAllText(FileName);
                var settings = JsonConvert.DeserializeObject<AccountSelectionSettings>(json);
                return new AccountSelectionModel(settings);
            } catch (Exception) {
                return new AccountSelectionModel();
            }
        }

        public static void Save(AccountSelectionModel model) {
            try {
                var settings = model.Settings;
                var json = JsonConvert.SerializeObject(settings,Formatting.Indented);
                File.WriteAllText(FileName, json);
            } catch (Exception) { }
        }
    }
}
