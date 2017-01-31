using GBCLV2.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace GBCLV2.Pages
{
    public partial class DownloadPage : Page
    {
        public bool Succeeded { get; internal set; }
        public readonly AutoResetEvent DownloadComplete = new AutoResetEvent(false);
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(5000000) };

        private List<DownloadInfo> Downloads;
        private string title;

        private int total;
        private int complete;
        private int failed;

        private long Bytes;

        private bool IsDownloading;

        public DownloadPage(List<DownloadInfo> FilesToDownload, string Title)
        {
            ServicePointManager.DefaultConnectionLimit = 128;

            title = Title;
            Downloads = FilesToDownload;
            total = FilesToDownload.Count;

            InitializeComponent();

            timer.Tick += (s, e) =>
            {
                if(complete == total)
                {
                    Succeeded = true;
                    timer.Stop();
                    cts.Dispose();
                    NavigationService.GoBack();
                    DownloadComplete.Set();
                    return;
                }

                if(complete + failed == total)
                {
                    IsDownloading = false;
                    timer.Stop();
                    cts.Dispose();
                    titleBox.Text = "下载结束，某些文件下载失败";
                    speedBox.Text = null;
                    return;
                }

                if(Bytes > 524288L)
                {
                    speedBox.Text = $"{Bytes / 524288.0:F1} MB/s";
                }
                else if(Bytes > 512L)
                {
                    speedBox.Text = $"{Bytes / 512.0:F1} KB/s";
                }
                else
                {
                    speedBox.Text = $"{Bytes << 1} B/s";
                }
                Bytes = 0;
            };
        }

        private void StartDownload(object sender, RoutedEventArgs e)
        {
            IsDownloading = true;
            Succeeded = false;
            titleBox.Text = title;
            progressBar.Maximum = total;

            timer.Start();
            statusBox.Text = $"0/{total}个文件下载成功";

            foreach(var download in Downloads)
            {
                ThreadPool.QueueUserWorkItem(t => DownloadFile(download));
            }

        }

        private void DownloadFile(DownloadInfo download)
        {
            if (!Directory.Exists(Path.GetDirectoryName(download.Path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(download.Path));
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(download.Url);
                request.Proxy = null;
                request.Timeout = 10000;
                request.ReadWriteTimeout = 25000;
                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";

                cts.Token.Register(() =>request.Abort());
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (cts.IsCancellationRequested)
                {
                    return;
                }

                using (var fileStream = new FileStream(download.Path, FileMode.Create, FileAccess.Write))
                {
                    var responseStream = response.GetResponseStream();
                    byte[] buffer = new byte[1024];
                    int size = responseStream.Read(buffer, 0, 1024);

                    while (!cts.IsCancellationRequested && size > 0)
                    {
                        fileStream.Write(buffer, 0, size);
                        size = responseStream.Read(buffer, 0, 1024);
                        Bytes += size;
                    }
                    responseStream.Close();
                    fileStream.Close();
                    response.Dispose();
                }
            }
            catch(WebException) when(cts.IsCancellationRequested)
            {
                File.Delete(download.Path);
                //你自己要取消的~
            }
            catch
            {
                Interlocked.Increment(ref failed);
                Dispatcher.Invoke(() => failsBox.Text = $"{failed}个文件下载失败" );
                File.Delete(download.Path);
                return;
            }

            if (cts.IsCancellationRequested)
            {
                File.Delete(download.Path);
                return;
            }

            Interlocked.Increment(ref complete);
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = complete;
                statusBox.Text = $"{complete}/{total}个文件下载成功";
            });
        }

        private void CancleDownload()
        {
            titleBox.Text = "取消中";
            speedBox.Text = null;
            cts.Cancel();
            timer.Stop();
            IsDownloading = false;
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            if(IsDownloading)
            {
                if (MessageBox.Show("正在下载中，你确定要中止吗", "(●—●)", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk) == MessageBoxResult.OK)
                {

                    CancleDownload();
                    NavigationService.GoBack();
                    DownloadComplete.Set();
                }
                else return;
            }
            else
            {
                cts.Dispose();
                NavigationService.GoBack();
                DownloadComplete.Set();
            }
        }
    }
}
