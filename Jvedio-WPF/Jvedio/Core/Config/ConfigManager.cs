﻿using Jvedio.Core.Config;
using Jvedio.Core.Config.Base;
using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.WindowConfig;
using Newtonsoft.Json;
using SuperUtils.Common;
using System;
using System.Collections.Generic;
using System.IO;
using static Jvedio.LogManager;

namespace Jvedio
{
    public static class ConfigManager
    {
        public const string RELEASE_DATE = "2023-07-08";

        public static bool Loaded = false;


        public static StartUp StartUp { get; set; }
        public static Core.WindowConfig.Main Main { get; set; }
        public static Filter Filter { get; set; }
        public static Edit Edit { get; set; }
        public static Detail Detail { get; set; }
        public static MetaData MetaData { get; set; }
        public static Settings Settings { get; set; }

        public static Jvedio.Core.Config.ServerConfig ServerConfig { get; set; }
        public static Jvedio.Core.Config.ProxyConfig ProxyConfig { get; set; }
        public static Jvedio.Core.Config.ScanConfig ScanConfig { get; set; }
        public static Jvedio.Core.Config.FFmpegConfig FFmpegConfig { get; set; }
        public static Jvedio.Core.Config.RenameConfig RenameConfig { get; set; }
        public static Jvedio.Core.Config.PluginConfig PluginConfig { get; set; }
        public static Jvedio.Core.Config.ThemeConfig ThemeConfig { get; set; }
        public static Jvedio.Core.Config.DownloadConfig DownloadConfig { get; set; }
        public static Jvedio.Core.Config.JavaServerConfig JavaServerConfig { get; set; }

        static ConfigManager()
        {
            StartUp = StartUp.CreateInstance();
            Main = Core.WindowConfig.Main.CreateInstance();
            Filter = Core.WindowConfig.Filter.CreateInstance();
            Edit = Edit.CreateInstance();
            Detail = Detail.CreateInstance();
            MetaData = MetaData.CreateInstance();
            Settings = Settings.CreateInstance();


            ServerConfig = Jvedio.Core.Config.ServerConfig.CreateInstance();
            ProxyConfig = Jvedio.Core.Config.ProxyConfig.CreateInstance();
            ScanConfig = Jvedio.Core.Config.ScanConfig.CreateInstance();
            FFmpegConfig = Jvedio.Core.Config.FFmpegConfig.CreateInstance();
            RenameConfig = Jvedio.Core.Config.RenameConfig.CreateInstance();
            PluginConfig = Jvedio.Core.Config.PluginConfig.CreateInstance();
            ThemeConfig = Jvedio.Core.Config.ThemeConfig.CreateInstance();
            DownloadConfig = Jvedio.Core.Config.DownloadConfig.CreateInstance();
            JavaServerConfig = Jvedio.Core.Config.JavaServerConfig.CreateInstance();

            ReadConfig();
        }

        public static void SaveAll()
        {
            StartUp.Save();
            Main.Save();
            Filter.Save();
            Edit.Save();
            Detail.Save();
            MetaData.Save();
            Settings.Save();


            ServerConfig.Save();
            ProxyConfig.Save();
            ScanConfig.Save();
            FFmpegConfig.Save();
            RenameConfig.Save();
            PluginConfig.Save();
            ThemeConfig.Save();
            DownloadConfig.Save();
            JavaServerConfig.Save();
        }

        public static void Restore()
        {
            Settings.TeenMode = true;
        }

        public static void ReadConfig()
        {
            System.Reflection.PropertyInfo[] propertyInfos = typeof(ConfigManager).GetProperties();

            foreach (var item in propertyInfos) {
                AbstractConfig config = item.GetValue(null) as AbstractConfig;
                if (config == null) {
                    Logger.Error(new Exception("无法识别的 AbstractConfig"));
                    continue;
                }

                config.Read();
            }
        }

        public static void Init(Action callBack)
        {
            if (Loaded)
                return;

            // 配置 ffmpeg 路径
            if (!File.Exists(ConfigManager.FFmpegConfig.Path) && File.Exists("ffmpeg.exe"))
                ConfigManager.FFmpegConfig.Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

            EnsurePicPaths();

            callBack?.Invoke();

            Loaded = true;
        }

        public static void EnsurePicPaths()
        {
            if (string.IsNullOrEmpty(Settings.PicPathJson)) {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add(PathType.Absolute.ToString(), PathManager.PicPath);
                dict.Add(PathType.RelativeToApp.ToString(), "./Pic");

                Dictionary<string, string> d = new Dictionary<string, string>();
                d.Add("BigImagePath", "./fanart");
                d.Add("SmallImagePath", "./poster");
                d.Add("PreviewImagePath", "./.preview");
                d.Add("ScreenShotPath", "./.screenshot");
                d.Add("ActorImagePath", "./.actor");
                dict.Add(PathType.RelativeToData.ToString(), d);
                Settings.PicPathJson = JsonConvert.SerializeObject(dict);
                Settings.PicPaths = dict;
            } else {
                Dictionary<string, object> dictionary = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(Settings.PicPathJson);
                if (dictionary == null)
                    return;
                string str = dictionary[PathType.RelativeToData.ToString()].ToString();
                dictionary[PathType.RelativeToData.ToString()] = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(str);
                Settings.PicPaths = dictionary;
            }
        }
    }
}
