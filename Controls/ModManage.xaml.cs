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

namespace GBCLV2.Controls
{
    public partial class ModManage : Grid
    {
        private class Mod
        {
            public bool IsEnabled       { get; set; }
            public string FileName      { get; set; }
            public string Name          { get; set; }
            public string Description   { get; set; }
            public string Url           { get; set; }
        }

        private ObservableCollection<Mod> CurrentMods = new ObservableCollection<Mod>();
        private string ModsDir = App.Core.GameRootPath + @"\mods\";

        public ModManage()
        {
            InitializeComponent();

            ModList.ItemsSource = CurrentMods;
            ModList.Items.SortDescriptions.Add(new SortDescription("IsEnabled",ListSortDirection.Descending));

            Task.Run(() => GetModsFromDisk());

            refresh_button.Click    += (s, e) => GetModsFromDisk();
            delete_button.Click     += (s, e) => DeleteModsAsync();
            openfolder_button.Click += (s, e) => System.Diagnostics.Process.Start("explorer.exe",ModsDir);

            ModList.Drop            += (s, e) => Copy_New(e.Data.GetData(DataFormats.FileDrop) as string[]);
            ModList.PreviewKeyDown  += (s, e) =>
            {
                if(e.Key == System.Windows.Input.Key.Delete)
                {
                    DeleteModsAsync();
                }
            };

            NameBox.MouseLeftButtonDown += (s, e) => 
            System.Diagnostics.Process.Start((ModList.SelectedItem as Mod).Url);
        }

        private void GetModsFromDisk()
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                CurrentMods.Clear();
            });

            if (Directory.Exists(ModsDir))
            {
                foreach (string dir in Directory.EnumerateFiles(ModsDir)
                .Where(dir => dir.EndsWith(".jar") || dir.EndsWith(".zip") || dir.EndsWith(".disabled")))
                {
                    LoadModInfo(dir);
                }
            }
        }

        private void LoadModInfo(string dir)
        {
            Mod _mod = new Mod()
            {
                FileName = Path.GetFileNameWithoutExtension(dir),
                IsEnabled = dir.EndsWith(".disabled") ? false : true
            };

            using (FileStream ModToOpen = new FileStream(dir, FileMode.Open))
            using (ZipArchive archive = new ZipArchive(ModToOpen, ZipArchiveMode.Read))
            {
                ZipArchiveEntry entry = archive.GetEntry("mcmod.info");
                if (entry != null)
                {
                    string str = new StreamReader(entry.Open(), System.Text.Encoding.Default).ReadToEnd();
                    try
                    {
                        JsonData ModInfo = JsonMapper.ToObject(str.Substring(1, str.Length - 1));
                        archive.Dispose();

                        _mod.Name = ModInfo["name"]?.ToString();
                        _mod.Description = ModInfo["description"]?.ToString();
                        _mod.Url = ModInfo["url"]?.ToString();

                    }
                    catch
                    {

                    };
                }
            }

            if (dir.EndsWith(".zip")) FileSystem.RenameFile(dir, _mod.Name + ".jar");
            if (_mod.Name == null) _mod.Name = _mod.FileName;

            Dispatcher.BeginInvoke((Action)delegate ()
            {
                CurrentMods.Add(_mod);
            });
        }

        private void DeleteModsAsync()
        {
            if(ModList.SelectedIndex == -1)
            {
                return;
            }

            string[] paths = new string[ModList.SelectedItems.Count];
            int i = 0;

            while (ModList.SelectedIndex != -1)
            {
                Mod _mod = ModList.SelectedItem as Mod;
                paths[i++] = ModsDir + _mod.FileName + (_mod.IsEnabled ? ".jar" : ".disabled");
                CurrentMods.Remove(_mod);
            }

            Task.Run(() =>
            {
                foreach(var path in paths)
                {
                    FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            });
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Mod _mod = ModList.SelectedItem as Mod;
            NameBox.Text = _mod?.Name;
            DescriptionBox.Text =  _mod?.Description;

            if(string.IsNullOrEmpty(_mod?.Url))
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
            Mod _mod = (Mod)checkbox.DataContext;

            if (checkbox.IsChecked ?? false)
            {
                FileSystem.RenameFile(ModsDir + _mod.FileName + ".disabled", _mod.FileName + ".jar");
                _mod.IsEnabled = true;
            }
            else
            {
                FileSystem.RenameFile(ModsDir + _mod.FileName + ".jar", _mod.FileName + ".disabled");
                _mod.IsEnabled = false;
            }
            ModList.Items.SortDescriptions.Clear();
            ModList.Items.SortDescriptions.Add(new SortDescription("IsEnabled", ListSortDirection.Descending));

        }

        private void Copy_New(string[] filePaths)
        {
            Task.Run(() =>
            {
                foreach (string path in filePaths)
                {
                    if (path.EndsWith(".jar") || path.EndsWith(".zip"))
                    {
                        using (var fs = new FileStream(path, FileMode.Open))
                        using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
                        {
                            if(archive.GetEntry("META-INF/") == null)
                            {
                                MessageBox.Show(path + "\n不是有效的mod文件", "你可能选了假mod");
                                continue;
                            }
                        }
                        
                        string CopyTo = ModsDir + Path.GetFileNameWithoutExtension(path) + ".jar";

                        if (!File.Exists(CopyTo))
                        {
                            LoadModInfo(path);
                            File.Copy(path, CopyTo, true);
                        }
                    }
                }
            });
        }

        private void Add_New(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = true,
                Title = "请选择mod",
                Filter = "MOD文件| *.jar; *.zip",
            };

            if (dialog.ShowDialog() ?? false)
            {
                Copy_New(dialog.FileNames);
            }
        }
    }
}
