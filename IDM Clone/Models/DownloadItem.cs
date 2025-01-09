using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IDM_Clone.Models
{
    public class DownloadItem : INotifyPropertyChanged
    {
        private string _status;
        private double _progress;
        private string _downloadedPartSize;

        public string Url
        {
            get;
        }
        public string OutputPath
        {
            get;
        }
        public string Name => System.IO.Path.GetFileName(new Uri(Url).LocalPath);

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsVisibilityProgress));
                }
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsVisibilityProgress));
                }
            }
        }

        public string DownloadedPartSize
        {
            get => _downloadedPartSize;
            set
            {
                if (_downloadedPartSize != value)
                {
                    _downloadedPartSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsVisibilityProgress => this.Progress < 100;

        public Progress<DownloadStatus> DownloadProgress
        {
            get; set;
        }

        public DownloadItem(string url, string outputPath)
        {
            Url = url;
            OutputPath = outputPath;
            _status = "Bắt đầu tải xuống";
            _downloadedPartSize = "[ ]";
            _progress = 0;
            DownloadProgress = new Progress<DownloadStatus>();
        }

        public void ShowInFolder()
        {
            // Show In Folder
            try
            {
                var success = Windows.Storage.StorageFolder.GetFolderFromPathAsync(OutputPath).AsTask().Result;
                if (success != null)
                {
                    Windows.System.Launcher.LaunchFolderAsync(success);
                }
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        public void CopyURL()
        {
            // Copy URL
            try
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(Url);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
