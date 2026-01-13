using System;
using System.Collections.ObjectModel;
using System.IO;

namespace DISK1
{
    public class FileSystemItem
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; } = 0;  
        public string SizeFormatted => FormatBytes(Size);  
        public string FullPath { get; set; } = string.Empty;  
        public DateTime LastModified { get; set; } 
        public string LastModifiedDate => LastModified.ToString("yyyy-MM-dd");
        public bool IsDirectory { get; set; } = false;  
        public ObservableCollection<FileSystemItem>? Children { get; set; }
        public FileSystemItem? Parent { get; set; } 

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public double Percentage { get; set; } = 0;

        public string DisplayName => IsDirectory 
            ? $"📁 {Name} ({SizeFormatted} - {Percentage:0.00}%)" 
            : $"📄 {Name} ({SizeFormatted} - {Percentage:0.00}%)";
    }
}
