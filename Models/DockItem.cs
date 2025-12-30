namespace LiquidGlassShell.Models
{
    public class DockItem
    {
        public string Name { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
    }
}

