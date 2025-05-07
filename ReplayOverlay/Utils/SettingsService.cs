using System;
using System.IO;
using System.Text.Json;
using ReplayOverlay.Models;

namespace ReplayOverlay.Utils
{
    public static class SettingsService
    {
        private static readonly string FilePath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ReplayOverlay", "settings.json");

        public static Settings Load()
        {
            if (!File.Exists(FilePath))
                return new Settings();

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
        }

        public static void Save(Settings settings)
        {
            string directory = Path.GetDirectoryName(FilePath)!;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(FilePath, json);
        }
    }
}
