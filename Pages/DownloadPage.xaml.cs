using GBCLV2.Modules;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace GBCLV2.Pages
{
    public class DownloadStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isDownloading;
        private string _downloadTitle;
        private string _downloadCompletes;
        private string _downloadFails;
        private string _downloadSpeed;
        private double _downloadProgress;

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                _isDownloading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDownloading)));
            }
        }

        public string DownloadTitle
        {
            get => _downloadTitle;
            set
            {
                _downloadTitle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadTitle)));
            }
        }

        public string DownloadCompletes
        {
            get => _downloadCompletes;
            set
            {
                _downloadCompletes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadCompletes)));
            }
        }

        public string DownloadFails
        {
            get => _downloadFails;
            set
            {
                _downloadFails = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadFails)));
            }
        }

        public string DownloadSpeed
        {
            get => _downloadSpeed;
            set
            {
                _downloadSpeed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadSpeed)));
            }
        }

        public double DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                _downloadProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadProgress)));
            }
        }
    }

    public partial class DownloadPage : Page
    {
        private static List<DownloadInfo> _downloads;
        private static ConcurrentBag<DownloadInfo> _faildDownlods = new ConcurrentBag<DownloadInfo>();
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static DispatcherTimer _timer = new DispatcherTimer() { Interval = new TimeSpan(5000000) };

        private static DownloadStatus _status = new DownloadStatus();

        private static int _totalFiles;
        private static int _completeFiles;
        private static int _failedFiles;

        private static long _totalBytes;
        private static long _downloadBytes;
        private static long _downloadBytesPreviousTick;

        public DownloadPage()
        {
            ServicePointManager.DefaultConnectionLimit = 256;
            
            InitializeComponent();
            this.DataContext = _status;

            _timer.Tick += (s, e) =>
            {
                _status.DownloadCompletes = $"{_completeFiles}/{_totalFiles}个文件下载完成";
                _status.DownloadFails = (_failedFiles == 0) ? null : $"{_failedFiles}个文件下载失败";
                _status.DownloadSpeed = GetDownloadSpeed();
                _status.DownloadProgress = (double)_downloadBytes / _totalBytes;
            };

            back_btn.Click += (s, e) => GoBack();
        }

        public async Task<bool> StartDownloadAsync(IEnumerable<DownloadInfo> filesToDownload, string title)
        {
            _downloads = filesToDownload.ToList();

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = 16
            };

            while(_downloads.Count > 0)
            {
                _status.DownloadTitle = title;

                _totalFiles = _downloads.Count;
                _completeFiles = 0;
                _failedFiles = 0;

                _totalBytes = 0;
                foreach (var download in _downloads)
                {
                    _totalBytes += download.Size;
                }
                _downloadBytes = 0;
                _downloadBytesPreviousTick = 0;

                _timer.Start();
                _status.IsDownloading = true;

                await Task.Run(() =>
                {
                    try
                    {
                        Parallel.ForEach(_downloads, parallelOptions, download => DownloadFile(download));
                    }
                    catch (OperationCanceledException)
                    {

                    }
                });

                _timer.Stop();
                _status.IsDownloading = false;
                _downloads.Clear();

                if (_failedFiles != 0)
                {
                    _status.DownloadTitle = "下载结束，某些文件下载失败！";
                    if (MessageBox.Show("部分文件下载失败\n重试下载？", "(≖＿≖)", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                    {
                        while (!_faildDownlods.IsEmpty)
                        {
                            _faildDownlods.TryTake(out DownloadInfo download);
                            _downloads.Add(download);
                        }
                    }
                }
            }

            GoBack();
            return (_failedFiles == 0);
        }

        private static string GetDownloadSpeed()
        {
            if (!_status.IsDownloading) return null;

            var diffBytes = _downloadBytes - _downloadBytesPreviousTick;
            _downloadBytesPreviousTick = _downloadBytes;

            if (diffBytes > 524288L)
            {
                return $"{diffBytes / 524288.0:F1} MB/s";
            }
            else if (diffBytes > 512L)
            {
                return $"{diffBytes / 512.0:F1} KB/s";
            }
            else
            {
                return $"{diffBytes << 1} B/s";
            }
        }

        private static void DownloadFile(DownloadInfo download)
        {
            if (!Directory.Exists(Path.GetDirectoryName(download.Path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(download.Path));
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(download.Url);
                request.Proxy = null;
                request.KeepAlive = true;
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (compatible; Windows NT 10.0; .NET CLR 4.0.30319;)";

                _cts.Token.Register(() => request.Abort());
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if(download.Size == 0)
                {
                    Interlocked.Add(ref _totalBytes, response.ContentLength);
                }

                if (_cts.IsCancellationRequested)
                {
                    return;
                }

                using (var fileStream = new FileStream(download.Path, FileMode.Create, FileAccess.Write))
                {
                    var responseStream = response.GetResponseStream();

                    const int bufferSize = 2048;
                    byte[] buffer = new byte[bufferSize];
                    int size = responseStream.Read(buffer, 0, bufferSize);

                    while (!_cts.IsCancellationRequested && size > 0)
                    {
                        fileStream.Write(buffer, 0, size);
                        size = responseStream.Read(buffer, 0, bufferSize);
                        Interlocked.Add(ref _downloadBytes, size);
                    }
                    responseStream.Close();
                    response.Dispose();
                }
            }
            catch (WebException) when (_cts.IsCancellationRequested)
            {
                File.Delete(download.Path);
                //你自己要取消的~
            }
            catch
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(download.Url);
#endif
                Interlocked.Increment(ref _failedFiles);
                File.Delete(download.Path);
                _faildDownlods.Add(download);
                return;
            }
            if (_cts.IsCancellationRequested)
            {
                File.Delete(download.Path);
                return;
            }

            Interlocked.Increment(ref _completeFiles);
        }

        private void GoBack()
        {
            if (_status.IsDownloading)
            {
                if (MessageBox.Show("正在下载中，你确定要中止吗", "(●—●)", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk) == MessageBoxResult.OK)
                {
                    _cts.Cancel();
                    _timer.Stop();
                    _status.IsDownloading = false;

                    NavigationService.GoBack();
                }
                else return;
            }
            else
            {
                _timer.Stop();
                NavigationService.GoBack();
            }
        }
    }
}
