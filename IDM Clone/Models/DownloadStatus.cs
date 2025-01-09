using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDM_Clone.Models;
public class DownloadStatus
{
    public long TotalFileSize
    {
        get; set;
    }
    public long DownloadedSize
    {
        get; set;
    }
    public double DownloadSpeed
    {
        get; set;
    }
    public TimeSpan RemainingTime
    {
        get; set;
    }
    public double Progress
    {
        get; set;
    }
    public string Status
    {
        get; set;
    }
    public string DownloadedPartSize
    {
        get; set;
    }
    public DownloadStatus()
    {
        TotalFileSize = 0;
        DownloadedSize = 0;
        DownloadSpeed = 0;
        RemainingTime = TimeSpan.Zero;
        Progress = 0;
        Status = "";
        DownloadedPartSize = "";
    }
}
