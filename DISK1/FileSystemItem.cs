using System;
using System.Collections.ObjectModel;
using System.IO;

namespace DISK1
{
    public class FileSystemItem
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; } = 0;  // Dung lượng (bytes)
        public string SizeFormatted => FormatBytes(Size);  // Hiển thị đẹp (KB, MB, GB)
        public string FullPath { get; set; } = string.Empty;  // Đường dẫn đầy đủ
        public bool IsDirectory { get; set; } = false;  // Là folder hay file
        public ObservableCollection<FileSystemItem>? Children { get; set; }

        // Format dung lượng: 1024 bytes → 1 KB
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

        // Display name với icon và size
        public string DisplayName => IsDirectory 
            ? $"📁 {Name} ({SizeFormatted})" 
            : $"📄 {Name} ({SizeFormatted})";
    }
}
