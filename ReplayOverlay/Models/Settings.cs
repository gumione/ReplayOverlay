namespace ReplayOverlay.Models
{
    public class Settings
    {
        public bool ShowQuickSetup { get; set; } = true;
        public int WebsocketPort { get; set; } = 4455;
        public string WebsocketPassword { get; set; } = string.Empty;
    }
}
