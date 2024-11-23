using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SimplePhotos.Models;
using SimplePhotos.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimplePhotos.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImagesView : Page
    {
        public ImagesView()
        {
            this.InitializeComponent();
            MainWindowVM = new();
            //this.DataContext = MainWindowVM;
            this.Loaded += ImagesView_Loaded;
        }

        private async void ImagesView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            
        }

        public MainWindowVM MainWindowVM { get; }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await MainWindowVM.GetItemsAsync();
            base.OnNavigatedTo(e);


        }

        private void ImageGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if(args.InRecycleQueue)
            {
                var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
                var image = templateRoot.FindName("ItemImage") as Image;
                image.Source = null;
            }
            if(args.Phase is 0)
            {
                args.RegisterUpdateCallback(ShowImage);
                args.Handled = true;
            }
        }
        private async void ShowImage(ListViewBase sender,ContainerContentChangingEventArgs args)
        {
            if(args.Phase is 1)
            {
                var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
                var image = templateRoot.FindName("ItemImage") as Image;
                var item = args.Item as ImageFileInfo;
                image.Source = await item.GetImageThumbnailAsync();
            }
        }
    }
}
