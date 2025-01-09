using System.Net.WebSockets;
using IDM_Clone.Models;
using IDM_Clone.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Gaming.Input.ForceFeedback;

namespace IDM_Clone.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        DataContext = ViewModel; // Set the DataContext for data binding
    }

    private async void Download_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Choose a folder to save the file
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
        picker.FileTypeFilter.Add("*");

        // Thiết lập cho WinUI
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        Windows.Storage.StorageFolder folder = await picker.PickSingleFolderAsync();
        ViewModel.addDownloadItem(URL.Text, folder.Path);

        // Xóa text ở TextBox
        URL.Text = "";
    }

    private void ShowInFolder_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Show In Folder
        var button = (Button)sender;
        var item = (DownloadItem)button.DataContext;
        item.ShowInFolder();
    }

    private void CopyURL_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Copy URL
        var button = (Button)sender;
        var item = (DownloadItem)button.DataContext;
        item.CopyURL();
    }
}
