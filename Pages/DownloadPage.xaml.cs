using GBCLV2.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

        private List<DownloadInfo> Downloads;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(5000000) };

        
        private string title;

        private static int total;
        private static int complete;
        private static int failed;
        private static long speed;

        private static bool IsDownloading;

        public DownloadPage(List<DownloadInfo> FilesToDownload, string Title)
        {
            ServicePointManager.DefaultConnectionLimit = 256;

            title = Title;
            Downloads = FilesToDownload;
            total = FilesToDownload.Count;

            InitializeComponent();

            timer.Tick += (s, e) =>
            {
                if (complete == total)
                {
                    Succeeded = true;
                    IsDownloading = false;
                    timer.Stop();
                    Go_Back();
                    return;
                }

                if (complete + failed == total)
                {
                    IsDownloading = false;
                    timer.Stop();
                    titleBox.Text = "下载结束，某些文件下载失败";
                    speedBox.Text = null;
                    return;
                }

                progressBar.Value = complete;
                statusBox.Text = $"{complete}/{total}个文件下载成功";

                if (failed != 0)
                {
                    failsBox.Text = $"{failed}个文件下载失败";
                }

                if (speed > 524288L)
                {
                    speedBox.Text = $"{speed / 524288.0:F1} MB/s";
                }
                else if (speed > 512L)
                {
                    speedBox.Text = $"{speed / 512.0:F1} KB/s";
                }
                else
                {
                    speedBox.Text = $"{speed << 1} B/s";
                }
                speed = 0;
            };

            back_btn.Click += (s, e) => Go_Back();
        }

        private void StartDownload(object sender, RoutedEventArgs e)
        {
            IsDownloading = true;
            Succeeded = false;
            titleBox.Text = title;
            progressBar.Maximum = total;
            complete = 0;
            failed = 0;

            timer.Start();
            statusBox.Text = $"0/{total}个文件下载成功";

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = 32
            };

            Task.Run(() =>
            {
                try
                {
                    Parallel.ForEach(Downloads, parallelOptions, download => DownloadFile(download));
                }
                catch(OperationCanceledException)
                {

                }
            });

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
                request.KeepAlive = true;
                request.Timeout = 25000;
                request.ReadWriteTimeout = 36000;
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (compatible; Windows NT 10.0; .NET CLR 4.0.30319;)";

                cts.Token.Register(() => request.Abort());
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (cts.IsCancellationRequested)
                {
                    return;
                }

                using (var fileStream = new FileStream(download.Path, FileMode.Create, FileAccess.Write))
                {
                    var responseStream = response.GetResponseStream();
                    byte[] buffer = new byte[2048];
                    int size = responseStream.Read(buffer, 0, 2048);

                    while (!cts.IsCancellationRequested && size > 0)
                    {
                        fileStream.Write(buffer, 0, size);
                        size = responseStream.Read(buffer, 0, 2048);
                        Interlocked.Add(ref speed, size);
                    }
                    responseStream.Close();
                    response.Dispose();
                }
            }
            catch (WebException) when (cts.IsCancellationRequested)
            {
                File.Delete(download.Path);
                //你自己要取消的~
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(download.Url);
                Interlocked.Increment(ref failed);
                File.Delete(download.Path);
                return;
            }

            if (cts.IsCancellationRequested)
            {
                File.Delete(download.Path);
                return;
            }

            Interlocked.Increment(ref complete);
        }

        private void Go_Back()
        {
            if (IsDownloading)
            {
                if (MessageBox.Show("正在下载中，你确定要中止吗", "(●—●)", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk) == MessageBoxResult.OK)
                {
                    cts.Cancel();
                    timer.Stop();
                    IsDownloading = false;

                    NavigationService.GoBack();

                    DownloadComplete.Set();
                    DownloadComplete.Dispose();
                    cts.Dispose();
                }
                else return;
            }
            else
            {
                NavigationService.GoBack();

                DownloadComplete.Set();
                DownloadComplete.Dispose();
                cts.Dispose();
            }
        }
    }
}
