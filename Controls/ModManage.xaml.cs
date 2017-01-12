using LitJson;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GBCLV2.Controls
{
    public partial class ModManage : UserControl
    {
        private class Mod
        {
            public bool IsEnabled { get; set; }
            public string Path { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Version { get; set; }
            public string MCVersion { get; set; }
            public string Url { get; set; }
        }

        private ObservableCollection<Mod> CurrentMods = new ObservableCollection<Mod>();
        private string ModsDir = Config.GameRootPath + @"\mods\";

        public ModManage()
        {
            InitializeComponent();
            ModList.ItemsSource = CurrentMods;
            Task.Run(() => GetModsFromDisk());
            refresh_button.Click += (s, e) => GetModsFromDisk();
        }

        private void GetModsFromDisk()
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                CurrentMods.Clear();
            });
            foreach (string dir in Directory.EnumerateFiles(ModsDir)
            .Where(dir => dir.EndsWith(".jar") || dir.EndsWith(".zip") || dir.EndsWith(".disabled")))
            {
                LoadMod(dir);
            }
        }

        private void LoadMod(string dir)
        {
            Mod _mod = new Mod()
            {
                Path = Path.GetFileNameWithoutExtension(dir),
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

                        foreach (string a in ModInfo.Keys)
                            switch (a)
                            {
                                case "name":
                                    _mod.Name = ModInfo[a].ToString();
                                    break;

                                case "version":
                                    _mod.Version = ModInfo[a].ToString();
                                    break;

                                case "mcversion":
                                    _mod.MCVersion = ModInfo[a].ToString();
                                    break;

                                case "description":
                                    _mod.Description = ModInfo[a].ToString();
                                    break;

                                case "url":
                                    _mod.Url = ModInfo[a].ToString();
                                    break;
                            }

                    }
                    catch
                    {

                    };
                }
            }

            if (dir.EndsWith(".zip")) FileSystem.RenameFile(dir, _mod.Name + ".jar");
            if (_mod.Name == null) _mod.Name = _mod.Path;

            Dispatcher.BeginInvoke((Action)delegate ()
            {
                CurrentMods.Add(_mod);
            });
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Mod _mod = ModList.SelectedItem as Mod;
            DescriptionBox.Text = _mod?.Description;
        }

        private void RewriteExtension(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            Mod _mod = (Mod)checkbox.DataContext;

            if (checkbox.IsChecked ?? false)
            {
                FileSystem.RenameFile(ModsDir + _mod.Path + ".disabled", _mod.Path + ".jar");
                _mod.IsEnabled = false;
            }
            else
            {
                FileSystem.RenameFile(ModsDir + _mod.Path + ".jar", _mod.Path + ".disabled");
                _mod.IsEnabled = true;
            }
        }

        private void Drop_Mods(object sender, DragEventArgs e)
        {
            Task.Run(() =>
            {
                foreach (string _file in e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[])
                {
                    if (_file.EndsWith(".jar") || _file.EndsWith(".zip"))
                    {
                        string path = ModsDir + Path.GetFileNameWithoutExtension(_file) + ".jar";
                        if (!File.Exists(path))
                        {
                            LoadMod(_file);
                            File.Copy(_file, path, true);
                        }
                    }
                }
            });
        }

    }
}
