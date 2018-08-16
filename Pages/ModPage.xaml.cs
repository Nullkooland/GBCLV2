using LitJson;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GBCLV2.Modules;

namespace GBCLV2.Pages
{
    public partial class ModPage : Page
    {
        private class ModInfo
        {
            public bool IsEnabled { get; set; }
            public string FileName { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
        }

        private ObservableCollection<ModInfo> _currentMods = new ObservableCollection<ModInfo>();
        private string _modsDir;

        public ModPage()
        {
            InitializeComponent();

            if (Config.Args.HasAnyVersion && Config.Args.IsVersionSplit)
            {
                _modsDir = $"{App.Core.GameRootPath}\\versions\\{Config.Args.SelectedVersion.ID}\\mods\\";
            }
            else
            {
                _modsDir = $"{App.Core.GameRootPath}\\mods\\";
            }

            ModList.ItemsSource = _currentMods;
            ModList.Items.SortDescriptions.Add(new SortDescription("IsEnabled", ListSortDirection.Descending));

            Task.Run(() => GetModsFromDisk());

            _refreshButton.Click += (s, e) => GetModsFromDisk();
            _deleteButton.Click += (s, e) => DeleteMods();
            _backButton.Click += (s, e) => NavigationService.GoBack();
            _openFolderButton.Click += (s, e) =>
            {
                if (!Directory.Exists(_modsDir))
                {
                    Directory.CreateDirectory(_modsDir);
                }
                System.Diagnostics.Process.Start("explorer.exe", _modsDir);
            };

            ModList.Drop += (s, e) => CopyMods(e.Data.GetData(DataFormats.FileDrop) as string[]);
            ModList.PreviewKeyDown += (s, e) =>
            {
               if (e.Key == System.Windows.Input.Key.Delete)
               {
                   DeleteMods();
               }
            };

            NameBox.MouseLeftButtonDown += (s, e) =>
            {
                System.Diagnostics.Process.Start((ModList.SelectedItem as ModInfo).Url);
                e.Handled = true;
            };
        }

        private void GetModsFromDisk()
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                _currentMods.Clear();
            });

            if (Directory.Exists(_modsDir))
            {
                foreach (string dir in Directory.EnumerateFiles(_modsDir)
                .Where(dir => dir.EndsWith(".jar") || dir.EndsWith(".zip") || dir.EndsWith(".disabled")))
                {
                    LoadModInfo(dir);
                }
            }
        }

        private void LoadModInfo(string path)
        {
            ModInfo mod = new ModInfo()
            {
                FileName = Path.GetFileNameWithoutExtension(path),
                IsEnabled = path.EndsWith(".disabled") ? false : true
            };

            using (var archive = ZipFile.OpenRead(path))
            {
                ZipArchiveEntry entry = archive.GetEntry("mcmod.info");
                if (entry != null)
                {
                    string str = new StreamReader(entry.Open(), System.Text.Encoding.UTF8).ReadToEnd();
                    try
                    {
                        JsonData ModInfo = JsonMapper.ToObject(str.Substring(1, str.Length - 1));

                        mod.Name = ModInfo["name"]?.ToString();
                        mod.Description = ModInfo["description"]?.ToString();
                        mod.Url = ModInfo["url"]?.ToString();

                    }
                    catch
                    {

                    };
                }
            }

            if (path.EndsWith(".zip")) FileSystem.RenameFile(path, mod.Name + ".jar");
            if (mod.Name == null) mod.Name = mod.FileName;

            Dispatcher.BeginInvoke((Action)delegate ()
            {
                _currentMods.Add(mod);
            });
        }

        private void DeleteMods()
        {
            if (ModList.SelectedIndex == -1)
            {
                return;
            }

            string[] paths = new string[ModList.SelectedItems.Count];
            int i = 0;

            while (ModList.SelectedIndex != -1)
            {
                ModInfo mod = ModList.SelectedItem as ModInfo;
                paths[i++] = _modsDir + mod.FileName + (mod.IsEnabled ? ".jar" : ".disabled");
                _currentMods.Remove(mod);
            }

            Task.Run(() =>
            {
                foreach (var path in paths)
                {
                    FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            });
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModInfo mod = ModList.SelectedItem as ModInfo;
            NameBox.Text = mod?.Name;
            DescriptionBox.Text = mod?.Description;

            if (string.IsNullOrEmpty(mod?.Url))
            {
                NameBox.IsEnabled = false;
                NameBox.TextDecorations = null;
                NameBox.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                NameBox.IsEnabled = true;
                NameBox.TextDecorations = TextDecorations.Underline;
                NameBox.Foreground = System.Windows.Media.Brushes.DodgerBlue;

            }
        }

        private void RewriteExtension(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            ModInfo mod = (ModInfo)checkbox.DataContext;

            if (checkbox.IsChecked ?? false)
            {
                FileSystem.RenameFile(_modsDir + mod.FileName + ".disabled", mod.FileName + ".jar");
                mod.IsEnabled = true;
            }
            else
            {
                FileSystem.RenameFile(_modsDir + mod.FileName + ".jar", mod.FileName + ".disabled");
                mod.IsEnabled = false;
            }
            ModList.SelectedIndex = -1;
            ModList.Items.SortDescriptions.Clear();
            ModList.Items.SortDescriptions.Add(new SortDescription("IsEnabled", ListSortDirection.Descending));

        }

        private void AddNewMods(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = true,
                Title = "请选择mod",
                Filter = "MOD文件| *.jar; *.zip",
            };

            if (dialog.ShowDialog() ?? false)
            {
                CopyMods(dialog.FileNames);
            }
        }

        private void CopyMods(string[] filePaths)
        {
            Task.Run(() =>
            {
                foreach (string path in filePaths.Where(p => p.EndsWith(".jar") || p.EndsWith(".zip")))
                {
                    using (var archive = ZipFile.OpenRead(path))
                    {
                        if (archive.GetEntry("META-INF/") == null)
                        {
                            MessageBox.Show(path + "\n不是有效的mod文件", "你可能选了假mod", MessageBoxButton.OK, MessageBoxImage.Information);
                            continue;
                        }
                    }

                    string CopyTo = _modsDir + Path.GetFileNameWithoutExtension(path) + ".jar";

                    if (!File.Exists(CopyTo))
                    {
                        LoadModInfo(path);
                        File.Copy(path, CopyTo, true);
                    }
                }
            });
        }
    }
}
