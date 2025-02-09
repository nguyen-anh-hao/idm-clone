﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IDM_Clone.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IDM_Clone.ViewModels
{
    public partial class MainViewModel : ObservableRecipient
    {
        private string FlexibleTime(TimeSpan time)
        {
            if (time.Hours > 0)
                return $"{time.Hours} giờ {time.Minutes} phút";
            else if (time.Minutes > 0)
                return $"{time.Minutes} phút";
            return $"{time.Seconds} giây";
        }

        private string FlexibleSize(double size)
        {
            if (size >= 1024 * 1024)
                return $"{(size / 1024f / 1024f):0.00} MB";
            else if (size >= 1024)
                return $"{(size / 1024f):0.00} KB";
            return $"{size} B";
        }

        private string FlexibleTime(double timeInSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(timeInSeconds);
            return FlexibleTime(time);
        }

        private readonly HttpClient _httpClient = new HttpClient();

        public async Task DownloadFileAsync(string url, string outputPath, int numberOfThreads, IProgress<DownloadStatus> progress)
        {
            // Lấy tên file và kích thước
            string fileName = Path.GetFileName(new Uri(url).AbsolutePath);
            outputPath = Path.Combine(outputPath, fileName);

            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            long fileSize = response.Content.Headers.ContentLength ?? throw new Exception("Không lấy được kích thước file.");

            long downloadedSize = 0;
            long partSize = fileSize / numberOfThreads;
            long[] dowloadedPartSize = new long[numberOfThreads];

            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(numberOfThreads, numberOfThreads);
            var stopwatch = Stopwatch.StartNew();

            // Tải từng phần
            for (int i = 0; i < numberOfThreads; i++)
            {
                int threadIndex = i; // Biến cục bộ lưu giá trị hiện tại của i
                long start = threadIndex * partSize;
                long end = (threadIndex == numberOfThreads - 1) ? fileSize - 1 : start + partSize - 1;

                tasks.Add(Task.Run(async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

                    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Lỗi tải phần {threadIndex + 1}: {response.StatusCode}");

                    await semaphore.WaitAsync();
                    try
                    {
                        using var fs = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                        fs.Seek(start, SeekOrigin.Begin);

                        using var stream = await response.Content.ReadAsStreamAsync();
                        var buffer = new byte[8192]; // Kích thước buffer tùy chỉnh
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, bytesRead);
                            Interlocked.Add(ref downloadedSize, bytesRead); // Cập nhật downloadedSize ngay lập tức
                            Interlocked.Add(ref dowloadedPartSize[threadIndex], bytesRead); // Cập nhật downloadedSize của phần
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }


            // Task báo cáo tiến độ
            var reportTask = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(200);
                    double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;
                    double speed = elapsedTimeInSeconds > 0 ? downloadedSize / elapsedTimeInSeconds : 0;

                    progress.Report(new DownloadStatus
                    {
                        TotalFileSize = fileSize,
                        DownloadedSize = downloadedSize,
                        DownloadSpeed = speed / (1024 * 1024), // Tính MB/s
                        RemainingTime = speed > 0 ? TimeSpan.FromSeconds((fileSize - downloadedSize) / speed) : TimeSpan.MaxValue,
                        Progress = (double)downloadedSize / fileSize * 100,
                        Status = downloadedSize < fileSize ? $"{FlexibleSize(speed)}/s - {FlexibleSize(downloadedSize)} của {FlexibleSize(fileSize)}, còn {FlexibleTime(elapsedTimeInSeconds)}" : "Đã tải xong",
                        DownloadedPartSize = $"[ {string.Join(" ] [ ", dowloadedPartSize.Select(size => FlexibleSize(size)))} ]"
                    });

                    if (downloadedSize >= fileSize)
                        break;
                }
            });

            // Chờ tất cả hoàn thành
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            await reportTask;

            // Báo cáo hoàn tất
            progress.Report(new DownloadStatus
            {
                TotalFileSize = fileSize,
                DownloadedSize = fileSize,
                DownloadSpeed = 0,
                RemainingTime = TimeSpan.Zero,
                Progress = 100,
                Status = "Đã tải xong"
            });
        }

        ObservableCollection<DownloadItem> _downloads = new ObservableCollection<DownloadItem>();
        public ObservableCollection<DownloadItem> Downloads
        {
            get => _downloads;
            set => SetProperty(ref _downloads, value);
        }

        public void addDownloadItem(string url, string outputPath)
        {
            var newDownloadItem = new DownloadItem(url, outputPath);
            Downloads.Add(newDownloadItem);
            OnPropertyChanged(nameof(Downloads));

            var progress = new Progress<DownloadStatus>(status =>
            {
                newDownloadItem.Status = status.Status;
                newDownloadItem.Progress = status.Progress;
                newDownloadItem.DownloadedPartSize = status.DownloadedPartSize;
            });

            _ = DownloadFileAsync(url, outputPath, 4, progress);
        }

        public MainViewModel()
        {
        }
    }
}

// Còn tạm dừng, hủy bỏ, responsive