using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GBCLV2.Modules;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace GBCLV2.Pages
{
    public partial class DownloadPage : Page
    {
        public List<DownloadInfo> FilesToDownload;

        private int total;
        private int succeeded;
        private int failed;

        private long Bytes;

        private CancellationTokenSource cts = new CancellationTokenSource();
        private static DispatcherTimer timer = new DispatcherTimer();

        public DownloadPage()
        {
            InitializeComponent();
            ServicePointManager.DefaultConnectionLimit = 256;

            cancle_btn.Click += (s, e) => CancleDownload();

            timer.Interval = new TimeSpan(2500000);
            timer.Tick += (s, e) =>
            {
                if(succeeded == total)
                {
                    timer.Stop();
                    NavigationService.GoBack();
                }

                if(Bytes > 262144L)
                {
                    speedBox.Text = string.Format("{0:F1} MB/s",(double)Bytes / 262144.0);
                }
                else if(Bytes > 256L)
                {
                    speedBox.Text = string.Format("{0:F1} KB/s", (double)Bytes / 256.0);
                }
                else
                {
                    speedBox.Text = string.Format("{0} B/s", Bytes * 4);
                }
                Bytes = 0;
            };
        }

        private async void StartDownload(object sender, RoutedEventArgs e)
        {
            total = FilesToDownload.Count;
            progressBar.Maximum = total;
            cts = new CancellationTokenSource();
            timer.Start();
            statusBox.Text = string.Format("{0}/{1}个文件下载成功", succeeded, total);

            for (int i = 0; i < total; i++)
            {
                await DownloadFileAsync(FilesToDownload[i]).ConfigureAwait(false);
            }
        }

        private void CancleDownload()
        {
            cts.Cancel();
            timer.Stop();
            statusBox.Text = "下载已取消";
            speedBox.Text = null;
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

                var responseStream = response.GetResponseStream();
                var fileStream = new FileStream(download.Path, FileMode.Create, FileAccess.Write);

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

                if (cts.IsCancellationRequested)
                {
                    return;
                }
            }
            catch(Exception e)
            {
                Interlocked.Increment(ref failed);
                Debug.WriteLine(download.Url);
                Debug.WriteLine(e.Message);
                Dispatcher.Invoke(() => failsBox.Text = string.Format("{0}个下载失败", failed));
                return;
            }

            Interlocked.Increment(ref succeeded);
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = succeeded;
                statusBox.Text = string.Format("{0}/{1}个文件下载成功", succeeded, total);
            });
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("正在下载中，你确定要中止吗", "(●—●)", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk) == MessageBoxResult.OK)
            {
                CancleDownload();
                NavigationService.GoBack();
            }
            else return;
        }
    }
}
