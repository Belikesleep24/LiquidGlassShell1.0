using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Reflection;
using LiquidGlassShell.Models;

namespace LiquidGlassShell.Services
{
    public class DockService
    {
        private string GetApplicationDirectory()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(location);
            return !string.IsNullOrEmpty(directory) ? directory : Directory.GetCurrentDirectory();
        }

        public List<DockItem> LoadDockItems()
        {
            var items = new List<DockItem>();
            var appDir = GetApplicationDirectory();
            var dockJsonPath = Path.Combine(appDir, "dock.json");
            var iconsBasePath = Path.Combine(appDir, "Assets", "Icons");

            if (!File.Exists(dockJsonPath))
            {
                return items;
            }

            try
            {
                var jsonContent = File.ReadAllText(dockJsonPath);
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("items", out var itemsElement))
                {
                    foreach (var itemElement in itemsElement.EnumerateArray())
                    {
                        var item = new DockItem();

                        if (itemElement.TryGetProperty("name", out var nameElement))
                        {
                            item.Name = nameElement.GetString() ?? string.Empty;
                        }

                        if (itemElement.TryGetProperty("id", out var idElement))
                        {
                            item.Id = idElement.GetString() ?? string.Empty;
                        }

                        if (itemElement.TryGetProperty("icon", out var iconElement))
                        {
                            var iconFileName = iconElement.GetString() ?? string.Empty;
                            
                            // Buscar el icono con la extensión especificada
                            var iconPath = Path.Combine(iconsBasePath, iconFileName);
                            
                            // Si no existe, intentar con otras extensiones comunes
                            if (!File.Exists(iconPath))
                            {
                                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(iconFileName);
                                
                                // Intentar .png
                                var pngPath = Path.Combine(iconsBasePath, fileNameWithoutExt + ".png");
                                if (File.Exists(pngPath))
                                {
                                    iconPath = pngPath;
                                }
                                // Intentar .ico
                                else
                                {
                                    var icoPath = Path.Combine(iconsBasePath, fileNameWithoutExt + ".ico");
                                    if (File.Exists(icoPath))
                                    {
                                        iconPath = icoPath;
                                    }
                                }
                            }
                            
                            if (File.Exists(iconPath))
                            {
                                item.IconPath = iconPath;
                            }
                            else
                            {
                                item.IconPath = string.Empty;
                            }
                        }

                        if (itemElement.TryGetProperty("executable", out var executableElement))
                        {
                            item.ExecutablePath = executableElement.GetString() ?? string.Empty;
                        }

                        if (itemElement.TryGetProperty("applicationName", out var appNameElement))
                        {
                            item.ApplicationName = appNameElement.GetString() ?? string.Empty;
                        }

                        if (itemElement.TryGetProperty("arguments", out var argsElement))
                        {
                            item.Arguments = argsElement.GetString() ?? string.Empty;
                        }

                        if (itemElement.TryGetProperty("workingDirectory", out var workDirElement))
                        {
                            item.WorkingDirectory = workDirElement.GetString() ?? string.Empty;
                        }

                        items.Add(item);
                    }
                }
            }
            catch
            {
            }

            return items;
        }
    }
}
