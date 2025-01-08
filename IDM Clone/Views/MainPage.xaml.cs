using IDM_Clone.Models;
using IDM_Clone.ViewModels;

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
    }

    private string convertBytesToMegabytes(long bytes)
    {
        return (bytes / 1024f / 1024f).ToString("0.00");
    }

    private string flexibleTime(TimeSpan time)
    {
        if (time.Hours > 0)
        {
            return $"{time.Hours} giờ {time.Minutes} phút";
        }
        else if (time.Minutes > 0)
        {
            return $"{time.Minutes} phút";
        }
        else
        {
            return $"{time.Seconds} giây";
        }
    }

    private string flexibleSize(long size)
    {
        if (size > 1024 * 1024 * 1024)
        {
            return $"{(size / 1024f / 1024f).ToString("0.00")} MB";
        }
        else if (size > 1024 * 1024)
        {
            return $"{(size / 1024f).ToString("0.00")} KB";
        }
        else
        {
            return $"{size} B";
        }
    }

    private void Download_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var progress = new Progress<DownloadStatus>(status =>
        {
            // 3,2 MB/s - 87,2 MB của 1,1 GB, còn 6 phút
            if (status.Status == "Downloading")
            {
                DownloadStatus.Text = $"{status.DownloadSpeed.ToString("0.0")} MB/s - {flexibleSize(status.DownloadedSize)} MB của {convertBytesToMegabytes(status.TotalFileSize)} MB, còn {flexibleTime(status.RemainingTime)}";
                ProgressBar.Value = status.Progress;
            }
            else
            {
                DownloadStatus.Text = $"{status.DownloadSpeed.ToString("0.0")} MB/s - {flexibleSize(status.DownloadedSize)} MB của {convertBytesToMegabytes(status.TotalFileSize)} MB, còn {flexibleTime(status.RemainingTime)}";
                ProgressBar.Value = status.Progress;
            }
        });
        ViewModel.DownloadFileAsync(URL.Text, "D:\\test\\", 4, progress);
    }
}
