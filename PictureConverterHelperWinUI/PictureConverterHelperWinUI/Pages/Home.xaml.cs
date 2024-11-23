using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PictureConverterHelperWinUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        private IntPtr m_hwnd;
        public Home()
        {
            this.InitializeComponent();
            m_hwnd = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App).m_window);
        }
        StorageFile _file;
        private async void backBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //var frame = (Application.Current as App).RootFrame;
            //if (frame.CanGoBack)
            //{
            //    frame.GoBack();
            //}
            FileOpenPicker picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };
            picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, m_hwnd);
            _file = await picker.PickSingleFileAsync();
            displayTb.Text = _file?.Name;
        }

        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string resultStr = default;
            if (_file != null && !string.IsNullOrEmpty(FileTypeTb.Text))
            {
                try
                {
                    using (IRandomAccessStream streamIn = await _file.OpenReadAsync())
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(streamIn);
                        PixelDataProvider pxprd = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight,
                            new BitmapTransform(), ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
                        byte[] data = pxprd.DetachPixelData();

                        FileSavePicker savePicker = new();
                        savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

                        savePicker.FileTypeChoices.Add("Image", new List<string> { $".{FileTypeTb.Text}" });
                        savePicker.SuggestedFileName = _file.DisplayName;

                        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, m_hwnd);
                        var file = await savePicker.PickSaveFileAsync();

                        //StorageFolder saveFolder = KnownFolders.SavedPictures;
                        //var file = await saveFolder.CreateFileAsync(_file.DisplayName+".png",CreationCollisionOption.ReplaceExisting);
                        if (file != null)
                        {
                            using (var streamOut = await file.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, streamOut);
                                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, decoder.PixelWidth, decoder.PixelHeight,
                                    decoder.DpiX, decoder.DpiY, data);
                                await encoder.FlushAsync();
                            }
                            resultDialog.Content = "×ª»»³É¹¦£¡";
                            if (await resultDialog.ShowAsync() is ContentDialogResult.Primary)
                            {
                                _ = await Launcher.LaunchFileAsync(file);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultDialog.Content = ex.Message;
                    _ = await resultDialog.ShowAsync();
                }
            }
        }

        private async void Button_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            //FileSavePicker savePicker = new FileSavePicker
            //{
            //    SuggestedStartLocation = PickerLocationId.PicturesLibrary
            //};
            //var file = await savePicker.PickSaveFileAsync();
            //if (file != null)
            //{
            //    await file.CopyAndReplaceAsync(_newfile);
            //}
        }
    }
}
