using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IDM_Clone.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace IDM_Clone.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly HttpClient _httpClient = new HttpClient();

    //public RelayCommand DownloadCommand
    //{
    //    get;
    //}

    public async Task DownloadFileAsync(string url, string outputPath, int numberOfThreads, IProgress<DownloadStatus> progress)
    {
        // Lấy tên file
        string fileName = Path.GetFileName(new Uri(url).AbsolutePath);
        outputPath = Path.Combine(outputPath, fileName);

        long downloadedSize = 0;
        DateTime startTime = DateTime.Now;

        // Lấy kích thước file
        var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
        var fileSize = response.Content.Headers.ContentLength ?? throw new Exception("Không lấy được kích thước file.");

        // Chia đoạn tải
        long partSize = fileSize / numberOfThreads;
        var tasks = new Task[numberOfThreads];
        var semaphore = new SemaphoreSlim(1, 1);

        for (int i = 0; i < numberOfThreads; i++)
        {
            long start = i * partSize;
            long end = (i == numberOfThreads - 1) ? fileSize - 1 : start + partSize - 1;

            tasks[i] = Task.Run(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var data = await response.Content.ReadAsByteArrayAsync();

                // Ghi vào file
                await semaphore.WaitAsync();
                try
                {
                    using var fs = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                    fs.Seek(start, SeekOrigin.Begin);
                    fs.Write(data, 0, data.Length);

                    // Cập nhật tải về
                    Interlocked.Add(ref downloadedSize, data.Length);
                }
                finally
                {
                    semaphore.Release();
                }
            });
        }

        // Chạy một task riêng biệt để báo cáo mỗi giây
        var reportTask = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000); // Chờ 1 giây
                TimeSpan elapsedTime = DateTime.Now - startTime;
                double speed = downloadedSize / elapsedTime.TotalSeconds;

                // Báo cáo tiến độ
                progress.Report(new DownloadStatus
                {
                    TotalFileSize = fileSize,
                    DownloadedSize = downloadedSize,
                    DownloadSpeed = speed,
                    RemainingTime = TimeSpan.FromSeconds((fileSize - downloadedSize) / speed),
                    Progress = (double)downloadedSize / fileSize * 100,
                    Status = downloadedSize < fileSize ? "Downloading" : "Downloaded"
                });

                // Kiểm tra xem tải xong chưa
                if (downloadedSize >= fileSize)
                    break;
            }
        });

        // Đợi tất cả các task hoàn thành
        await Task.WhenAll(tasks);

        // Chờ task báo cáo tiến độ kết thúc
        await reportTask;

        // Báo cáo khi tải xong
        progress.Report(new DownloadStatus
        {
            TotalFileSize = fileSize,
            DownloadedSize = fileSize,
            DownloadSpeed = 0,
            RemainingTime = TimeSpan.FromSeconds(0),
            Progress = 100,
            Status = "Downloaded"
        });
    }

    public MainViewModel()
    {
        //DownloadCommand = new RelayCommand(async () =>
        //{
        //    var progress = new Progress<DownloadStatus>(status =>
        //    {
        //        Console.WriteLine($"Downloaded: {status.DownloadedSize}/{status.TotalFileSize} ({status.Progress:P1}) - Speed: {status.DownloadSpeed / 1024 / 1024:F2} MB/s - Remaining: {status.RemainingTime}");
        //    });

        //    await DownloadFileAsync("https://png.pngtree.com/thumb_back/fh260/background/20230511/pngtree-nature-background-sunset-wallpaer-with-beautiful-flower-farms-image_2592160.jpg", "D:\\test\\", 4, progress);
        //});
    }
}
