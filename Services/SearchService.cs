using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiquidGlassShell.Services;

namespace LiquidGlassShell.Services
{
    public class SearchService
    {
        private readonly AppLauncherService _appLauncher;
        private List<SearchResult> _indexedFiles = new();
        private bool _isIndexing = false;

        public SearchService(AppLauncherService appLauncher)
        {
            _appLauncher = appLauncher;
        }

        public Task<List<SearchResult>> SearchAsync(string query)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(query))
                    return new List<SearchResult>();

                var results = new List<SearchResult>();

                // Buscar en apps
                var apps = _appLauncher.SearchApps(query);
                results.AddRange(apps.Select(app => new SearchResult
                {
                    Title = app.Name,
                    Type = SearchResultType.Application,
                    Path = app.LnkPath,
                    Relevance = CalculateRelevance(app.Name, query)
                }));

                // Buscar en archivos indexados
                var fileResults = _indexedFiles
                    .Where(f => f.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                               f.Path.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(f => new SearchResult
                    {
                        Title = f.Title,
                        Type = SearchResultType.File,
                        Path = f.Path,
                        Relevance = CalculateRelevance(f.Title, query)
                    });

                results.AddRange(fileResults);

                // Ordenar por relevancia
                return results.OrderByDescending(r => r.Relevance).Take(20).ToList();
            });
        }

        public async Task IndexFilesAsync(string[] directories)
        {
            if (_isIndexing) return;
            _isIndexing = true;

            await Task.Run(() =>
            {
                var files = new List<SearchResult>();
                foreach (var directory in directories)
                {
                    if (Directory.Exists(directory))
                    {
                        try
                        {
                            var dirFiles = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                                .Where(f => !f.Contains("\\$") && !f.Contains("\\System Volume Information"))
                                .Take(1000); // Limitar para rendimiento

                            files.AddRange(dirFiles.Select(f => new SearchResult
                            {
                                Title = Path.GetFileName(f),
                                Type = SearchResultType.File,
                                Path = f
                            }));
                        }
                        catch
                        {
                            // Ignorar errores de acceso
                        }
                    }
                }
                _indexedFiles = files;
            });

            _isIndexing = false;
        }

        private int CalculateRelevance(string text, string query)
        {
            if (text.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                return 100;
            if (text.Contains(query, StringComparison.OrdinalIgnoreCase))
                return 50;
            return 0;
        }
    }

    public class SearchResult
    {
        public string Title { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public SearchResultType Type { get; set; }
        public int Relevance { get; set; }
    }

    public enum SearchResultType
    {
        Application,
        File,
        Folder,
        Web
    }
}

