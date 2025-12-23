using System.Windows;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace DISK1
{
    public partial class MainWindow : Window
    {
        private string selectedFolderPath = string.Empty;
        private bool isScanning = false;
        private int totalFiles = 0;
        private int scannedFiles = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select a folder to analyze",   
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                selectedFolderPath = dialog.FolderName;
                txtPath.Text = selectedFolderPath;
                txtStatus.Text = $"✓ Selected: {selectedFolderPath}";
            }
        }

        private async void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrEmpty(selectedFolderPath) || !Directory.Exists(selectedFolderPath))
            {
                MessageBox.Show("Please select a valid folder first!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isScanning)
            {
                MessageBox.Show("Scanning is already in progress!", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Prepare UI
            isScanning = true;
            btnScan.IsEnabled = false;
            btnBrowse.IsEnabled = false;
            progressPanel.Visibility = Visibility.Visible;
            progressBar.Value = 0;
            treeViewFiles.Items.Clear();
            totalFiles = 0;
            scannedFiles = 0;

            try
            {
                txtStatus.Text = "🔍 Counting files...";
                txtProgressInfo.Text = "Preparing...";

                // Step 1: Count total files (for progress calculation)
                await Task.Run(() =>
                {
                    DirectoryInfo rootDir = new DirectoryInfo(selectedFolderPath);
                    totalFiles = CountFiles(rootDir);
                });

                Dispatcher.Invoke(() =>
                {
                    txtStatus.Text = $"Found {totalFiles} files. Starting deep scan...";
                    txtProgressInfo.Text = $"Scanning {totalFiles} files...";
                });

                // Step 2: Deep scan with progress
                FileSystemItem rootItem = null;
                await Task.Run(() =>
                {
                    DirectoryInfo rootDir = new DirectoryInfo(selectedFolderPath);
                    rootItem = ScanDirectory(rootDir);
                });

                // Step 3: Sort by size (largest first)
                if (rootItem?.Children != null)
                {
                    SortBySize(rootItem.Children);
                }

                // Step 4: Display in TreeView
                treeViewFiles.Items.Add(rootItem);

                // Expand root
                if (treeViewFiles.Items.Count > 0)
                {
                    var item = treeViewFiles.ItemContainerGenerator.ContainerFromIndex(0) as System.Windows.Controls.TreeViewItem;
                    if (item != null)
                        item.IsExpanded = true;
                }

                // Complete
                progressBar.Value = 100;
                txtProgressPercent.Text = "100%";
                txtProgressInfo.Text = "✓ Scan completed!";
                txtStatus.Text = $"✓ Completed: {scannedFiles} files scanned, Total size: {rootItem?.SizeFormatted ?? "0 B"}";

                MessageBox.Show(
                    $"Scan completed successfully!\n\n" +
                    $"Total files: {scannedFiles}\n" +
                    $"Total size: {rootItem?.SizeFormatted ?? "0 B"}\n" +
                    $"Root folder: {rootItem?.Name ?? "Unknown"}",
                    "Scan Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied to some folders.\n\n{ex.Message}",
                    "Permission Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStatus.Text = "⚠ Scan completed with errors";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during scan:\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "❌ Scan failed";
            }
            finally
            {
                isScanning = false;
                btnScan.IsEnabled = true;
                btnBrowse.IsEnabled = true;

                // Hide progress after 2 seconds
                await Task.Delay(2000);
                progressPanel.Visibility = Visibility.Collapsed;
            }
        }

        // Count total files (for progress bar)
        private int CountFiles(DirectoryInfo directory)
        {
            int count = 0;
            try
            {
                count += directory.GetFiles().Length;
                foreach (var subDir in directory.GetDirectories())
                {
                    count += CountFiles(subDir);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch { }
            return count;
        }

        // Deep scan directory recursively
        private FileSystemItem ScanDirectory(DirectoryInfo directory)
        {
            var item = new FileSystemItem
            {
                Name = directory.Name,
                FullPath = directory.FullName,
                IsDirectory = true,
                Children = new ObservableCollection<FileSystemItem>()
            };

            long totalSize = 0;

            try
            {
                // Scan all files
                foreach (var file in directory.GetFiles())
                {
                    try
                    {
                        var fileItem = new FileSystemItem
                        {
                            Name = file.Name,
                            FullPath = file.FullName,
                            Size = file.Length,
                            IsDirectory = false
                        };
                        item.Children.Add(fileItem);
                        totalSize += file.Length;

                        // Update progress
                        scannedFiles++;
                        UpdateProgress(file.Name);
                    }
                    catch { }
                }

                // Scan all subdirectories (RECURSIVE)
                foreach (var subDir in directory.GetDirectories())
                {
                    try
                    {
                        var subItem = ScanDirectory(subDir);  // 🔄 RECURSIVE CALL
                        item.Children.Add(subItem);
                        totalSize += subItem.Size;
                    }
                    catch { }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch { }

            item.Size = totalSize;
            return item;
        }

        // Update progress bar
        private void UpdateProgress(string currentFile)
        {
            if (totalFiles == 0) return;

            int progress = (int)((scannedFiles / (double)totalFiles) * 100);

            Dispatcher.Invoke(() =>
            {
                progressBar.Value = progress;
                txtProgressPercent.Text = $"{progress}%";
                txtProgressInfo.Text = $"Scanning: {currentFile}";
            });
        }

        // Sort children by size (descending)
        private void SortBySize(ObservableCollection<FileSystemItem> items)
        {
            var sorted = items.OrderByDescending(x => x.Size).ToList();
            items.Clear();
            foreach (var item in sorted)
            {
                items.Add(item);
                if (item.Children != null && item.Children.Count > 0)
                {
                    SortBySize(item.Children);  // Sort recursively
                }
            }
        }
    }
}