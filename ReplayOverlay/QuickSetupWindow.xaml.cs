using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using ReplayOverlay.Models;
using ReplayOverlay.Utils;

namespace ReplayOverlay
{
    public partial class QuickSetupWindow : Window
    {
        private readonly Settings _settings;
        public bool DidSetup { get; private set; }

        public QuickSetupWindow(Settings settings)
        {
            InitializeComponent();
            _settings = settings;
            BtnYes.Click += OnYesClicked;
            BtnNo.Click += OnNoClicked;
        }

        private void OnNoClicked(object sender, RoutedEventArgs e)
        {
            _settings.ShowQuickSetup = !(DontShowAgain.IsChecked ?? false);
            SettingsService.Save(_settings);
            DidSetup = false;
            Close();
        }

        private void OnYesClicked(object sender, RoutedEventArgs e)
        {
            _settings.WebsocketPassword = Guid.NewGuid().ToString("N")[..16];
            _settings.ShowQuickSetup = false;
            SettingsService.Save(_settings);

            var cfgDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "obs-studio", "plugin_config", "obs-websocket");
            Directory.CreateDirectory(cfgDir);
            var cfgFile = Path.Combine(cfgDir, "config.json");

            var raw = File.Exists(cfgFile) ? File.ReadAllText(cfgFile) : "{}";
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;

            var dict = new Dictionary<string, JsonElement>();
            foreach (var prop in root.EnumerateObject())
                dict[prop.Name] = prop.Value;

            dict["server_enabled"] = JsonDocument.Parse("true").RootElement;
            dict["auth_required"] = JsonDocument.Parse("true").RootElement;
            dict["server_port"] = JsonDocument.Parse(_settings.WebsocketPort.ToString()).RootElement;
            dict["server_password"] = JsonDocument.Parse($"\"{_settings.WebsocketPassword}\"").RootElement;

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(cfgFile, JsonSerializer.Serialize(dict, options));

            DidSetup = true;
            Close();
        }
    }
}
