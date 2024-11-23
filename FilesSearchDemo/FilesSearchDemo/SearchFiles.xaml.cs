using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FilesSearchDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchFiles : Page
    {
        ObservableCollection<FileDetails> DataGridFiles { get; set; } = new();
        public SearchFiles()
        {
            this.InitializeComponent();
        }
        private async void SelectDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new();
            InitializeWithWindow.Initialize(folderPicker, App.hwnd);
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is not null)
                DirectoryTextBox.Text = folder.Path;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string directory = DirectoryTextBox.Text;
            string searchString = SearchStringTextBox.Text;
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(searchString))
            {
                ContentDialog contentDialog = new()
                {
                    Title = "Invalid Input",
                    Content = "Please enter a valid directory and search string",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await contentDialog.ShowAsync();
                return;
            }

            progressBar.IsIndeterminate = true;
            searchButton.IsEnabled = false;
            DataGridFiles.Clear();

            await FindFilesAsync(directory,searchString,SearchOption.AllDirectories);
            searchButton.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (cancellationTokenSource is not null)
                cancellationTokenSource.Cancel();

            searchButton.IsEnabled = true;
        }
        private string GetFileSize(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length is 0)
                return "0";
            string[] suffixes = ["bytes", "KB", "MB", "GB", "TB", "PB"];
            int counter = 0;
            decimal number = fileInfo.Length;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
        private CancellationTokenSource cancellationTokenSource = null;
        public async Task FindFilesAsync(string path, string searchPattern, SearchOption searchOption)
        {
            List<string> files = new();
            Stack<string> directories = new();
            IEnumerable<string> dirFiles = Enumerable.Empty<string>();
            IEnumerable<string> subDirectories = Enumerable.Empty<string>();

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            directories.Push(path);

            while (directories.Count > 0)
            {
                var currentPath = directories.Pop();

                try
                {
                    dirFiles = await Task.Run(() =>
                        Directory.EnumerateFiles(currentPath, $"*{searchPattern}*"), cancellationToken);

                    files.AddRange(dirFiles);

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        textBlockFound.Text = $"Found files: {files.Count}";
                    });

                    if (searchOption is SearchOption.AllDirectories)
                    {
                        subDirectories = await Task.Run(() =>
                        Directory.EnumerateDirectories(currentPath), cancellationToken);
                        foreach (var directory in subDirectories)
                        {
                            directories.Push(directory);
                        }
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (UnauthorizedAccessException) { continue; }
                catch (PathTooLongException) { continue; }
                catch (OperationCanceledException ex)
                {
                    Debug.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An error occurred while enumerating files: {ex.Message}");
                    continue;
                }
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                progressBar.IsIndeterminate = false;
                progressBar.Maximum = files.Count;
            });

            await Task.Run(async () =>
            {
                int processedFiles = 0;
                int batchSize = 50;
                List<FileDetails> batch = new();
                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    processedFiles++;
                    try
                    {
                        FileDetails fileDetails = new()
                        {
                            Name = Path.GetFileName(file),
                            DateModified = File.GetLastWriteTime(file),
                            Size = GetFileSize(file),
                            Path = file,
                            Extension = Path.GetExtension(file)
                        };

                        batch.Add(fileDetails);
                        if (batch.Count > batchSize)
                        {
                            await AddFilesToDataGridAsync(batch);
                            batch.Clear();
                        }
                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            progressBar.Value = processedFiles;
                            textBlockProcessed.Text = $"Processed files: {processedFiles}";
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"An error occurred while enumerating files: {ex.Message}");
                        continue;
                    }
                }
                if (batch.Any())
                {
                    await AddFilesToDataGridAsync(batch);
                }
                DispatcherQueue.TryEnqueue(() =>
                progressBar.Value = 0);
            });
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            files.Clear();
            directories.Clear();

            Debug.WriteLine("The End");
        }
        private async Task AddFilesToDataGridAsync(List<FileDetails> files)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                foreach (var file in files)
                {
                    DataGridFiles.Add(file);
                }
            });
        }
    }
    public record FileDetails
    {
        public string Name { get; set; }
        public DateTime DateModified { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
    }
}
