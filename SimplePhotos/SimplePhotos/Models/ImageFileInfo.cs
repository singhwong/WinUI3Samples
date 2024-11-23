using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace SimplePhotos.Models;

public class ImageFileInfo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public ImageFileInfo(ImageProperties imageProperties,
        StorageFile imageFile,
        string name,
        string type)
    {
        ImageProperties = imageProperties;
        ImageName = name;
        ImageFileType = type;
        var rating = (int)imageProperties.Rating;
        var random = new Random();
        ImageRating = rating == 0 ? random.Next(1, 5) : rating;
        ImageFile = imageFile;
    }
    public StorageFile ImageFile { get; }
    public ImageProperties ImageProperties { get; }
    public string ImageName { get; }
    public string ImageFileType { get; }
    public string ImageDimensions => $"{ImageProperties.Width} x {ImageProperties.Height}";
    public string ImageTitle
    {
        get => string.IsNullOrEmpty(ImageProperties.Title) ? ImageName : ImageProperties.Title;
        set
        {
            if (ImageProperties.Title != value)
            {
                ImageProperties.Title = value;
                _ = ImageProperties.SavePropertiesAsync();
                OnPropertyChanged();
            }
        }
    }
    public int ImageRating
    {
        get => (int)ImageProperties.Rating;
        set
        {
            if (ImageProperties.Rating != value)
            {
                ImageProperties.Rating = (uint)value;
                _ = ImageProperties.SavePropertiesAsync();
                OnPropertyChanged();
            }
        }
    }
    public async Task<BitmapImage> GetImageSourceAsync()
    {
        using IRandomAccessStream fileStream = await ImageFile.OpenReadAsync();
        BitmapImage bitmapImage = new();
        bitmapImage.SetSource(fileStream);
        return bitmapImage;
    }
    public async Task<BitmapImage> GetImageThumbnailAsync()
    {
        StorageItemThumbnail thumbnail =
            await ImageFile.GetThumbnailAsync(ThumbnailMode.PicturesView);
        BitmapImage bitmapImage = new();
        bitmapImage.SetSource(thumbnail);
        thumbnail.Dispose();
        return bitmapImage;
    }
}
