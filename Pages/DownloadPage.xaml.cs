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
        public readonly AutoResetEvent DownloadComplete = new AutoResetEvent(false);
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(2500000) };

        private List<DownloadInfo> Downloads;
        private string title;

        private int total;
        private int succeeded;
        private int failed;

        private long Bytes;

        private bool IsDownloading;

        public DownloadPage(List<DownloadInfo> FilesToDownload, string Title)
        {
            ServicePointManager.DefaultConnectionLimit = 256;

            title = Title;
            Downloads = FilesToDownload;
            total = FilesToDownload.Count;

            InitializeComponent();
            cancle_btn.Click += (s, e) => CancleDownload();

            timer.Tick += (s, e) =>
            {
                if(succeeded == total)
                {
                    timer.Stop();
                    cts.Dispose();
                    NavigationService.GoBack();
                    DownloadComplete.Set();
                    return;
                }

                if(succeeded + failed == total)
                {
                    IsDownloading = false;
                    cancle_btn.IsEnabled = false;
                    timer.Stop();
                    cts.Dispose();
                    titleBox.Text = "下载结束，某些文件下载失败";
                    speedBox.Text = null;
                    return;
                }

                if(Bytes > 262144L)
                {
                    speedBox.Text = $"{Bytes / 262144.0:F1} MB/s";
                }
                else if(Bytes > 256L)
                {
                    speedBox.Text = $"{Bytes / 256.0:F1} KB/s";
                }
                else
                {
                    speedBox.Text = $"{Bytes * 4} B/s";
                }
                Bytes = 0;
            };
        }

        private async void StartDownload(object sender, RoutedEventArgs e)
        {
            IsDownloading = true;
            titleBox.Text = title;
            progressBar.Maximum = total;

            timer.Start();
            statusBox.Text = $"0/{total}个文件下载成功";

            foreach (var download in Downloads)
            {
                if(cts.IsCancellationRequested) break;
                await DownloadFileAsync(download).ConfigureAwait(false);
            }
        }

        private void CancleDownload()
        {
            cts.Cancel();
            timer.Stop();
            titleBox.Text = "下载已取消";
            speedBox.Text = null;
            IsDownloading = false;
            cancle_btn.IsEnabled = false;
        }

        private async Task DownloadFileAsync(DownloadInfo download)
        {
            if (!Directory.Exists(Path.GetDirectoryName(download.Path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(download.Path));
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(download.Url);
                request.Proxy = null;
                request.Timeout = 6000;
                request.ReadWriteTimeout = 18000;
                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";

                cts.Token.Register(() =>request.Abort());
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

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
                        size = await responseStream.ReadAsync(buffer, 0, 1024);
                        Bytes += size;
                    }
                    responseStream.Close();
                    fileStream.Close();
                    response.Dispose();
                }

                if (cts.IsCancellationRequested)
                {
                    File.Delete(download.Path);
                    return;
                }
            }
            catch(WebException) when(cts.IsCancellationRequested)
            {
                File.Delete(download.Path);
                //你自己要取消的~
            }
            catch(Exception e)
            {
                Interlocked.Increment(ref failed);
                //Debug.WriteLine(download.Url);
                //Debug.WriteLine(e.Message);
                Dispatcher.Invoke(() => failsBox.Text = $"{failed}个文件下载失败" );
                File.Delete(download.Path);
                return;
            }

            Interlocked.Increment(ref succeeded);
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = succeeded;
                statusBox.Text = $"{succeeded}/{total}个文件下载成功";
            });
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            if(IsDownloading)
            {
                if (MessageBox.Show("正在下载中，你确定要中止吗", "(●—●)", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk) == MessageBoxResult.OK)
                {
                    CancleDownload();
                    cts.Dispose();
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
