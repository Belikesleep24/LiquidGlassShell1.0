using System.Collections.Generic;
using System.Windows.Input;

namespace LiquidGlassShell.Models
{
    public class MenuItem
    {
        public string Title { get; set; } = string.Empty;
        public ICommand? Command { get; set; }
        public List<MenuItem>? SubItems { get; set; }
        public bool IsSeparator { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}

