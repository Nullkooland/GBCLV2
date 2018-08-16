using KMCCC.Launcher;
using LitJson;
using System.Collections.Generic;
using System.IO;

namespace GBCLV2.Modules
{
    interface IDownloadBaseUrl
    {
        string VersionListUrl { get; }
        string VersionBaseUrl { get; }
        string LibraryBaseUrl { get; }
        string MavenBaseUrl { get; }
        string JsonBaseUrl { get; }
        string AssetsBaseUrl { get; }
        string ForgeBaseUrl { get; }
    }

    class BMCLAPIBaseUrl : IDownloadBaseUrl
    {
        public string VersionListUrl { get; } = "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json";
        public string VersionBaseUrl { get; } = "https://bmclapi2.bangbang93.com/";
        public string LibraryBaseUrl { get; } = "https://bmclapi2.bangbang93.com/libraries/";
        public string MavenBaseUrl { get; } = "https://bmclapi2.bangbang93.com/maven/";
        public string JsonBaseUrl { get; } = "https://bmclapi2.bangbang93.com/";
        public string AssetsBaseUrl { get; } = "https://bmclapi2.bangbang93.com/assets/";
        public string ForgeBaseUrl { get; } = "https://bmclapi2.bangbang93.com/maven/net/minecraftforge/forge/";
    }

    class OfficialBaseUrl : IDownloadBaseUrl
    {
        public string VersionListUrl { get; } = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public string VersionBaseUrl { get; } = "https://launcher.mojang.com/";
        public string LibraryBaseUrl { get; } = "https://libraries.minecraft.net/";
        public string MavenBaseUrl { get; } = "https://files.minecraftforge.net/maven/";
        public string JsonBaseUrl { get; } = "https://launchermeta.mojang.com/";
        public string AssetsBaseUrl { get; } = "https://resources.download.minecraft.net/";
        public string ForgeBaseUrl { get; } = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/";
    }

    public class DownloadInfo
    {
        public string Path { get; set; }
        public string Url { get; set; }
        public int Size { get; set; }
    }

    static class DownloadHelper
    {
        public static IDownloadBaseUrl BaseUrl { get; private set; }

        public static void SetDownloadSource(int sourceType)
        {
            switch (sourceType)
            {
                case 0:
                    BaseUrl = new OfficialBaseUrl();
                    break;

                case 1:
                    BaseUrl = new BMCLAPIBaseUrl();
                    break;
            }
        }

        public static IEnumerable<DownloadInfo> GetLostEssentials(Version version)
        {
            var lostEssentials = new List<DownloadInfo>();

            var JarPath = $"{App.Core.GameRootPath}\\versions\\{version.JarID}\\{version.JarID}.jar";
            if (!File.Exists(JarPath))
            {
                lostEssentials.Add(new DownloadInfo
                {
                    Path = JarPath,
                    Url = BaseUrl.VersionBaseUrl + version.Downloads.Client.Url.Substring(28),
                    Size = version.Downloads.Client.Size,
            });
            }

            foreach (var lib in version.Libraries)
            {
                var absolutePath = $"{App.Core.GameRootPath}\\libraries\\{lib.Path}";
                if (!File.Exists(absolutePath))
                {
                    lostEssentials.Add(new DownloadInfo
                    {
                        Path = absolutePath,
                        Url = (lib.IsForgeLib) ? (BaseUrl.MavenBaseUrl + lib.Path) : (BaseUrl.LibraryBaseUrl + lib.Path),
                        Size = lib.Size,
                    });
                }
            }

            foreach (var native in version.Natives)
            {
                var absolutePath = $"{App.Core.GameRootPath}\\libraries\\{native.Path}";
                if (!File.Exists(absolutePath))
                {
                    lostEssentials.Add(new DownloadInfo
                    {
                        Path = absolutePath,
                        Url = BaseUrl.LibraryBaseUrl + native.Path,
                        Size = native.Size,
                    });
                }
            }
            return lostEssentials;
        }

        public class Assets
        {
            [JsonPropertyName("objects")]
            public Dictionary<string, Asset> Objects { get; set; }
        }

        public class Asset
        {
            [JsonPropertyName("hash")]
            public string Hash { get; set; }

            [JsonIgnore]
            public string HashPrefix { get => Hash.Substring(0, 2); }

            [JsonPropertyName("size")]
            public int Size { get; set; }
        }

        public static IEnumerable<DownloadInfo> GetLostAssets(Version version)
        {
            var lostAssets = new List<DownloadInfo>();

            var indexPath = $"{App.Core.GameRootPath}\\assets\\indexes\\{version.AssetsID}.json";
            string indexJson;

            if (!File.Exists(indexPath))
            {
                try
                {
                    string indexUrl;
                    if (version.AssetsIndex.Url != null)
                    {
                        indexUrl = BaseUrl.JsonBaseUrl + version.AssetsIndex.Url.Substring(32);
                    }
                    else
                    {
                        indexUrl = $"{BaseUrl.JsonBaseUrl}indexs/{version.AssetsID}.json";
                    }

                    var client = new System.Net.Http.HttpClient() { Timeout = new System.TimeSpan(0, 0, 5) };
                    indexJson = client.GetStringAsync(indexUrl).Result;
                    client.Dispose();
                }
                catch
                {
                    System.Windows.MessageBox.Show("获取资源列表失败!");
                    return lostAssets;
                }

                if (!Directory.Exists(Path.GetDirectoryName(indexPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(indexPath));
                }
                File.WriteAllText(indexPath, indexJson);
            }
            else
            {
                indexJson = File.ReadAllText(indexPath);
            }

            foreach(var asset in JsonMapper.ToObject<Assets>(indexJson).Objects)
            {
                var relativePath = $"{asset.Value.HashPrefix}\\{asset.Value.Hash}";
                var absolutePath = (version.AssetsID == "legacy") ? $"{App.Core.GameRootPath}\\assets\\virtual\\legacy\\{asset.Key}" 
                                                                  : $"{App.Core.GameRootPath}\\assets\\objects\\{relativePath}";

                if (!File.Exists(absolutePath))
                {
                    lostAssets.Add(new DownloadInfo
                    {
                        Path = absolutePath,
                        Url = BaseUrl.AssetsBaseUrl + relativePath,
                        Size = asset.Value.Size,
                    });
                }
            }

            return lostAssets;
        }
    }
}
