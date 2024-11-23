using SimplePhotos.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Gaming.XboxLive;
using Windows.Storage;
using Windows.Storage.Search;

namespace SimplePhotos.ViewModels
{
    public class MainWindowVM
    {
        public ObservableCollection<ImageFileInfo> Images { get; } =
            new ObservableCollection<ImageFileInfo>();
        public MainWindowVM()
        {

        }
        public async Task GetItemsAsync()
        {
            //StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
            //Debug.WriteLine(appInstalledFolder.Path);
            //StorageFolder picturesFolder = await appInstalledFolder.GetFolderAsync("Assets\\Samples");
            StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
            var result = picturesFolder.CreateFileQueryWithOptions(new QueryOptions());
            IReadOnlyList<StorageFile> imageFiles = await result.GetFilesAsync();
            foreach (var file in imageFiles)
            {
                Images.Add(await LoadImageInfoAsync(file));
            }
        }
        public async Task<ImageFileInfo> LoadImageInfoAsync(StorageFile file)
        {
            var properties = await file.Properties.GetImagePropertiesAsync();
            ImageFileInfo imageInfo = new(properties, file, file.DisplayName, file.DisplayType);
            return imageInfo;
        }
    }
}
