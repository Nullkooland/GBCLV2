﻿using KMCCC.Launcher;
using LitJson;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GBCLV2.Modules
{
    public enum DownloadSource { 官方, BMCLAPI, }

    interface IDownloadBaseUrl
    {
        string VersionListUrl       { get; }
        string VersionJarBaseUrl    { get; }
        string LibraryBaseUrl       { get; }
        string AssetsIndexBaseUrl   { get; }
        string AssetsBaseUrl        { get; }
    }

    class BMCLAPIBaseUrl : IDownloadBaseUrl
    {
        public string VersionListUrl        { get; } = "http://bmclapi2.bangbang93.com/mc/game/version_manifest.json";
        public string VersionJarBaseUrl     { get; } = "http://bmclapi2.bangbang93.com/versions/";
        public string LibraryBaseUrl        { get; } = "http://bmclapi2.bangbang93.com/libraries/";
        public string AssetsIndexBaseUrl    { get; } = "http://bmclapi2.bangbang93.com/indexes/";
        public string AssetsBaseUrl         { get; } = "http://bmclapi2.bangbang93.com/assets/";
    }

    class OfficialBaseUrl : IDownloadBaseUrl
    {
        public string VersionListUrl        { get; } = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public string VersionJarBaseUrl     { get; } = "https://s3.amazonaws.com/Minecraft.Download/versions/";
        public string LibraryBaseUrl        { get; } = "https://libraries.minecraft.net/";
        public string AssetsIndexBaseUrl    { get; } = "https://s3.amazonaws.com/Minecraft.Download/indexes/";
        public string AssetsBaseUrl         { get; } = "https://resources.download.minecraft.net/";
    }

    public class DownloadInfo
    {
        public string Path { get; set; }
        public string Url { get; set; }
    }

    static class DownloadHelper
    {
        public static IDownloadBaseUrl BaseUrl;

        public static void SetDownloadSource()
        {
            switch(App.Config.DownloadSource)
            {
                case DownloadSource.官方:
                    BaseUrl = new OfficialBaseUrl();
                    break;

                case DownloadSource.BMCLAPI:
                    BaseUrl = new BMCLAPIBaseUrl();
                    break;
            }
        }

        public static IEnumerable<DownloadInfo> GetLostEssentials(LauncherCore core, Version version)
        {
            var lostEssentials = new List<DownloadInfo>();

            var libs = version.Libraries.Select(lib => core.GetLibPath(lib));
            var natives = version.Natives.Select(native => core.GetNativePath(native));

            foreach (var path in libs.Concat(natives))
            {
                if (!File.Exists(path))
                {
                    lostEssentials.Add(new DownloadInfo
                    {
                        Path = path,
                        Url = path.Replace(core.GameRootPath + @"\libraries\" ,BaseUrl.LibraryBaseUrl)
                    });
                }
            }
            return lostEssentials;
        }

        public static IEnumerable<DownloadInfo> GetLostAssets(LauncherCore core, Version version)
        {
            var lostAssets = new List<DownloadInfo>();

            var index = File.ReadAllText(string.Format(@"{0}\assets\indexes\{1}.json", core.GameRootPath, version.Assets));
            var assets = JsonMapper.ToObject(index)["objects"];

            for (int i = 0; i < assets.Count; i++)
            {
                var hash = assets[i][0].ToString();
                var relativePath = string.Format(@"{0}\{1}", hash.Substring(0, 2), hash);
                var absolutePath = string.Format(@"{0}\assets\objects\{1}", core.GameRootPath, relativePath);

                if (!File.Exists(absolutePath))
                {
                    lostAssets.Add(new DownloadInfo
                    {
                        Path = absolutePath,
                        Url = BaseUrl.AssetsBaseUrl + relativePath
                    });
                }
            }
            return lostAssets;
        }
    }
}