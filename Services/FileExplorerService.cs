using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LiquidGlassShell.Services
{
    public class FileExplorerService
    {
        [DllImport("shell32.dll")]
        private static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        private const int SW_SHOW = 5;

        public List<FileSystemItem> GetDirectoryContents(string path)
        {
            var items = new List<FileSystemItem>();

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                // Retornar unidades lógicas si no hay path
                return GetLogicalDrives();
            }

            try
            {
                // Directorios
                var directories = Directory.GetDirectories(path);
                items.AddRange(directories.Select(d => new FileSystemItem
                {
                    Name = Path.GetFileName(d),
                    FullPath = d,
                    Type = FileSystemItemType.Folder,
                    Size = 0,
                    LastModified = Directory.GetLastWriteTime(d)
                }));

                // Archivos
                var files = Directory.GetFiles(path);
                items.AddRange(files.Select(f => new FileSystemItem
                {
                    Name = Path.GetFileName(f),
                    FullPath = f,
                    Type = FileSystemItemType.File,
                    Size = new FileInfo(f).Length,
                    LastModified = File.GetLastWriteTime(f)
                }));

                return items.OrderBy(i => i.Type).ThenBy(i => i.Name).ToList();
            }
            catch
            {
                return new List<FileSystemItem>();
            }
        }

        public List<FileSystemItem> GetLogicalDrives()
        {
            return DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => new FileSystemItem
                {
                    Name = d.Name,
                    FullPath = d.RootDirectory.FullName,
                    Type = FileSystemItemType.Drive,
                    Size = d.TotalSize,
                    LastModified = DateTime.Now
                })
                .ToList();
        }

        public void OpenFile(string filePath)
        {
            try
            {
                ShellExecute(IntPtr.Zero, "open", filePath, "", "", SW_SHOW);
            }
            catch
            {
            }
        }

        public void OpenFolder(string folderPath)
        {
            try
            {
                ShellExecute(IntPtr.Zero, "open", folderPath, "", "", SW_SHOW);
            }
            catch
            {
            }
        }

        public string GetCurrentDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
    }

    public class FileSystemItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public FileSystemItemType Type { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
    }

    public enum FileSystemItemType
    {
        Drive,
        Folder,
        File
    }
}

