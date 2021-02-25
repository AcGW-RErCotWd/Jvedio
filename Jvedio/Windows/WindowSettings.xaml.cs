﻿using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static Jvedio.GlobalMethod;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;
using System.Windows.Controls.Primitives;
using FontAwesome.WPF;
using System.ComponentModel;
using DynamicData.Annotations;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text.RegularExpressions;
using Jvedio.Library.Encrypt;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Jvedio_BaseWindow
    {

        public const string ffmpeg_url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z";

        public DetailMovie SampleMovie = new DetailMovie()
        {
            id = "AAA-001",
            title = Jvedio.Language.Resources.SampleMovie_Title,
            vediotype = 1,
            releasedate = "2020-01-01",
            director = Jvedio.Language.Resources.SampleMovie_Director,
            genre = Jvedio.Language.Resources.SampleMovie_Genre,
            tag = Jvedio.Language.Resources.SampleMovie_Tag,
            actor = Jvedio.Language.Resources.SampleMovie_Actor,
            studio = Jvedio.Language.Resources.SampleMovie_Studio,
            rating = 9.0f,
            chinesetitle = Jvedio.Language.Resources.SampleMovie_TranslatedTitle,
            label = Jvedio.Language.Resources.SampleMovie_Label,
            year = 2020,
            runtime = 126,
            country = Jvedio.Language.Resources.SampleMovie_Country
        };
        public VieModel_Settings vieModel_Settings;
        public Settings()
        {
            InitializeComponent();

            vieModel_Settings = new VieModel_Settings();
            vieModel_Settings.Reset();

            this.DataContext = vieModel_Settings;


            //绑定中文字体
            //foreach (FontFamily _f in Fonts.SystemFontFamilies)
            //{
            //    LanguageSpecificStringDictionary _font = _f.FamilyNames;
            //    if (_font.ContainsKey(System.Windows.Markup.XmlLanguage.GetLanguage("zh-cn")))
            //    {
            //        string _fontName = null;
            //        if (_font.TryGetValue(System.Windows.Markup.XmlLanguage.GetLanguage("zh-cn"), out _fontName))
            //        {
            //            ComboBox_Ttile.Items.Add(_fontName);
            //        }
            //    }
            //}

            //bool IsMatch = false;
            //foreach (var item in ComboBox_Ttile.Items)
            //{
            //    if (Properties.Settings.Default.Font_Title_Family == item.ToString())
            //    {
            //        ComboBox_Ttile.SelectedItem = item;
            //        IsMatch = true;
            //        break;
            //    }
            //}

            //if (!IsMatch) ComboBox_Ttile.SelectedIndex = 0;


            ServersDataGrid.ItemsSource = vieModel_Settings.Servers;

            //绑定事件
            foreach (var item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList())
            {
                item.Click += AddToRename;
            }
            TabControl.SelectedIndex = Properties.Settings.Default.SettingsIndex;

        }



        #region "热键"





        private void hotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            Key currentKey = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift)
            {
                if (!funcKeys.Contains(currentKey)) funcKeys.Add(currentKey);
            }
            else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.NumPad0 && currentKey <= Key.NumPad9))
            {
                key = currentKey;
            }
            else
            {
                //Console.WriteLine("不支持");
            }

            string singleKey = key.ToString();
            if (key.ToString().Length > 1)
            {
                singleKey = singleKey.ToString().Replace("D", "");
            }

            if (funcKeys.Count > 0)
            {
                if (key == Key.None)
                {
                    hotkeyTextBox.Text = string.Join("+", funcKeys);
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = Key.None;
                }
                else
                {
                    hotkeyTextBox.Text = string.Join("+", funcKeys) + "+" + singleKey;
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = key;
                }

            }
            else
            {
                if (key != Key.None)
                {
                    hotkeyTextBox.Text = singleKey;
                    _funcKeys = new List<Key>();
                    _key = key;
                }
            }




        }

        private void hotkeyTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {

            Key currentKey = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift)
            {
                if (funcKeys.Contains(currentKey)) funcKeys.Remove(currentKey);
            }
            else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.F1 && currentKey <= Key.F12))
            {
                if (currentKey == key)
                {
                    key = Key.None;
                }

            }


        }

        private void ApplyHotKey(object sender, RoutedEventArgs e)
        {
            bool containsFunKey = _funcKeys.Contains(Key.LeftAlt) | _funcKeys.Contains(Key.LeftCtrl) | _funcKeys.Contains(Key.LeftShift) | _funcKeys.Contains(Key.CapsLock);


            if (!containsFunKey | _key == Key.None)
            {
                HandyControl.Controls.Growl.Error("必须为 功能键 + 数字/字母", "SettingsGrowl");
            }
            else
            {
                //注册热键
                if (_key != Key.None & IsProperFuncKey(_funcKeys))
                {
                    uint fsModifiers = (uint)Modifiers.None;
                    foreach (Key key in _funcKeys)
                    {
                        if (key == Key.LeftCtrl) fsModifiers = fsModifiers | (uint)Modifiers.Control;
                        if (key == Key.LeftAlt) fsModifiers = fsModifiers | (uint)Modifiers.Alt;
                        if (key == Key.LeftShift) fsModifiers = fsModifiers | (uint)Modifiers.Shift;
                    }
                    VK = (uint)KeyInterop.VirtualKeyFromKey(_key);


                    UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
                    bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, fsModifiers, VK);
                    if (!success) { MessageBox.Show("热键冲突！", "热键冲突"); }
                    {
                        //保存设置
                        Properties.Settings.Default.HotKey_Modifiers = fsModifiers;
                        Properties.Settings.Default.HotKey_VK = VK;
                        Properties.Settings.Default.HotKey_Enable = true;
                        Properties.Settings.Default.HotKey_String = hotkeyTextBox.Text;
                        Properties.Settings.Default.Save();
                        HandyControl.Controls.Growl.Success("设置热键成功", "SettingsGrowl");
                    }

                }



            }
        }

        #endregion





        public void AddPath(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = Jvedio.Language.Resources.ChooseDir;
            folderBrowserDialog.ShowNewFolderButton = true;


            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK & !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                if (vieModel_Settings.ScanPath == null) { vieModel_Settings.ScanPath = new ObservableCollection<string>(); }
                if (!vieModel_Settings.ScanPath.Contains(folderBrowserDialog.SelectedPath) && !vieModel_Settings.ScanPath.IsIntersectWith(folderBrowserDialog.SelectedPath))
                {
                    vieModel_Settings.ScanPath.Add(folderBrowserDialog.SelectedPath);
                    //保存
                    FileProcess.SaveScanPathToConfig(vieModel_Settings.DataBase, vieModel_Settings.ScanPath?.ToList());
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.FilePathIntersection, "SettingsGrowl");
                }


            }





        }

        public async void TestAI(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            CheckBox checkBox = stackPanel.Children.OfType<CheckBox>().First();
            ImageAwesome imageAwesome = stackPanel.Children.OfType<ImageAwesome>().First();
            imageAwesome.Icon = FontAwesomeIcon.Refresh;
            imageAwesome.Spin = true;
            imageAwesome.Foreground = (SolidColorBrush)Application.Current.Resources["ForegroundSearch"];
            if (checkBox.Content.ToString() == Jvedio.Language.Resources.BaiduFaceRecognition)
            {

                string base64 = Resource_String.BaseImage64;
                System.Drawing.Bitmap bitmap = ImageProcess.Base64ToBitmap(base64);
                Dictionary<string, string> result;
                Int32Rect int32Rect;
                (result, int32Rect) = await TestBaiduAI(bitmap);
                if (result != null && int32Rect != Int32Rect.Empty)
                {
                    imageAwesome.Icon = FontAwesomeIcon.CheckCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Color.FromRgb(32, 183, 89));
                    string clientId = Properties.Settings.Default.Baidu_API_KEY.Replace(" ", "");
                    string clientSecret = Properties.Settings.Default.Baidu_SECRET_KEY.Replace(" ", "");
                    SaveKeyValue(clientId, clientSecret, "BaiduAI.key");
                }
                else
                {
                    imageAwesome.Icon = FontAwesomeIcon.TimesCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        public static Task<(Dictionary<string, string>, Int32Rect)> TestBaiduAI(System.Drawing.Bitmap bitmap)
        {
            return Task.Run(() =>
            {
                string token = AccessToken.getAccessToken();
                string FaceJson = FaceDetect.faceDetect(token, bitmap);
                Dictionary<string, string> result;
                Int32Rect int32Rect;
                (result, int32Rect) = FaceParse.Parse(FaceJson);
                return (result, int32Rect);
            });

        }

        public async void TestTranslate(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            CheckBox checkBox = stackPanel.Children.OfType<CheckBox>().First();
            ImageAwesome imageAwesome = stackPanel.Children.OfType<ImageAwesome>().First();
            imageAwesome.Icon = FontAwesomeIcon.Refresh;
            imageAwesome.Spin = true;
            imageAwesome.Foreground = (SolidColorBrush)Application.Current.Resources["ForegroundSearch"];

            if (checkBox.Content.ToString() == "百度翻译")
            {

            }
            else if (checkBox.Content.ToString() == Jvedio.Language.Resources.Youdao)
            {
                string result = await Translate.Youdao("のマ○コに");
                if (result != "")
                {
                    imageAwesome.Icon = FontAwesomeIcon.CheckCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Color.FromRgb(32, 183, 89));

                    string Youdao_appKey = Properties.Settings.Default.TL_YOUDAO_APIKEY.Replace(" ", "");
                    string Youdao_appSecret = Properties.Settings.Default.TL_YOUDAO_SECRETKEY.Replace(" ", "");

                    //成功，保存在本地
                    SaveKeyValue(Youdao_appKey, Youdao_appSecret,"youdao.key");
                }
                else
                {
                    imageAwesome.Icon = FontAwesomeIcon.TimesCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Colors.Red);
                }
            }


        }

        public void SaveKeyValue(string key,string value,string filename)
        {
            string v = Encrypt.AesEncrypt(key +" " + value, EncryptKeys[0]);
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, append: false))
                {
                    sw.Write(v);
                }
            }catch(Exception ex)
            {
                Logger.LogF(ex);
            }      
        }

        public void DelPath(object sender, MouseButtonEventArgs e)
        {
            if (PathListBox.SelectedIndex != -1)
            {
                for (int i = PathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel_Settings.ScanPath.Remove(PathListBox.SelectedItems[i].ToString());
                }
            }
            if (vieModel_Settings.ScanPath != null)
                SaveScanPathToConfig(vieModel_Settings.DataBase, vieModel_Settings.ScanPath.ToList());

        }

        public void ClearPath(object sender, MouseButtonEventArgs e)
        {

            vieModel_Settings.ScanPath?.Clear();
            SaveScanPathToConfig(vieModel_Settings.DataBase, new List<string>());
        }





        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (new Msgbox(this, Jvedio.Language.Resources.Message_IsToReset).ShowDialog() == true)
            {
                //保存网址
                List<string> urlList = new List<string>();
                urlList.Add(Properties.Settings.Default.Bus);
                urlList.Add(Properties.Settings.Default.BusEurope);
                urlList.Add(Properties.Settings.Default.Library);
                urlList.Add(Properties.Settings.Default.DB);
                urlList.Add(Properties.Settings.Default.FC2);
                urlList.Add(Properties.Settings.Default.Jav321);
                urlList.Add(Properties.Settings.Default.DMM);
                    urlList.Add(Properties.Settings.Default.MOO);

                List<bool> enableList = new List<bool>();
                enableList.Add(Properties.Settings.Default.EnableBus);
                enableList.Add(Properties.Settings.Default.EnableBusEu);
                enableList.Add(Properties.Settings.Default.EnableLibrary);
                enableList.Add(Properties.Settings.Default.EnableDB);
                enableList.Add(Properties.Settings.Default.EnableFC2);
                enableList.Add(Properties.Settings.Default.Enable321);
                enableList.Add(Properties.Settings.Default.EnableDMM);
                enableList.Add(Properties.Settings.Default.EnableMOO);


                Properties.Settings.Default.Reset();

                Properties.Settings.Default.Bus = urlList[0];
                Properties.Settings.Default.BusEurope = urlList[1];
                Properties.Settings.Default.Library = urlList[2];
                Properties.Settings.Default.DB = urlList[3];
                Properties.Settings.Default.FC2 = urlList[4];
                Properties.Settings.Default.Jav321 = urlList[5];
                Properties.Settings.Default.DMM = urlList[6];
                Properties.Settings.Default.MOO = urlList[7];

                Properties.Settings.Default.EnableBus = enableList[0];
                Properties.Settings.Default.EnableBusEu = enableList[1];
                Properties.Settings.Default.EnableLibrary = enableList[2];
                Properties.Settings.Default.EnableDB = enableList[3];
                Properties.Settings.Default.EnableFC2 = enableList[4];
                Properties.Settings.Default.Enable321 = enableList[5];
                Properties.Settings.Default.EnableDMM = enableList[6];
                Properties.Settings.Default.EnableMOO = enableList[7];


                Properties.Settings.Default.FirstRun = false;
                Properties.Settings.Default.Save();
            }

        }

        private void DisplayNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 500)
                {
                    Properties.Settings.Default.DisplayNumber = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void FlowTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 30)
                {
                    Properties.Settings.Default.FlowNum = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void ActorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 50)
                {
                    Properties.Settings.Default.ActorDisplayNum = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void ScreenShotNumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 20)
                {
                    Properties.Settings.Default.ScreenShotNum = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void ScanMinFileSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num >= 0 & num <= 2000)
                {
                    Properties.Settings.Default.ScanMinFileSize = num;
                    Properties.Settings.Default.Save();
                }
            }

        }






        private void ListenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsVisible == false) return;
            if ((bool)checkBox.IsChecked)
            {
                //测试是否能监听
                if (!TestListen())
                    checkBox.IsChecked = false;
                else
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.RebootToTakeEffect, "SettingsGrowl");
            }
        }


        FileSystemWatcher[] watchers;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool TestListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            watchers = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++)
            {
                try
                {

                    if (drives[i] == @"C:\") { continue; }
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = drives[i];
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Filter = "*.*";
                    watcher.EnableRaisingEvents = true;
                    watchers[i] = watcher;
                    watcher.Dispose();
                }
                catch
                {
                    HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.NoPermissionToListen} {drives[i]}", "SettingsGrowl");
                    return false;
                }
            }
            return true;
        }

        private void SetVediaPlaterPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.Choose;
            OpenFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            OpenFileDialog1.Filter = "exe|*.exe";
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string exePath = OpenFileDialog1.FileName;
                if (File.Exists(exePath))
                    Properties.Settings.Default.VedioPlayerPath = exePath;

            }
        }

        private void SetBasePicPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = Jvedio.Language.Resources.ChooseDir;
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Pic\\")) dialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory + "Pic\\";
            dialog.ShowNewFolderButton = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    System.Windows.MessageBox.Show(this, Jvedio.Language.Resources.Message_CanNotBeNull, Jvedio.Language.Resources.Hint);
                    return;
                }
                else
                {
                    string path = dialog.SelectedPath;
                    if (path.Substring(path.Length - 1, 1) != "\\") { path = path + "\\"; }
                    Properties.Settings.Default.BasePicPath = path;

                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Opacity_Main >= 0.5)
                App.Current.Windows[0].Opacity = Properties.Settings.Default.Opacity_Main;
            else
                App.Current.Windows[0].Opacity = 1;

            ////UpdateServersEnable();

            GlobalVariable.InitVariable();
            Scan.InitSearchPattern();
            Net.Init();
            HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "SettingsGrowl");
        }

        private void SetFFMPEGPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.ChooseFFmpeg;
            OpenFileDialog1.FileName = "ffmpeg.exe";
            OpenFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            OpenFileDialog1.Filter = "ffmpeg.exe|*.exe";
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string exePath = OpenFileDialog1.FileName;
                if (File.Exists(exePath))
                {
                    if (new FileInfo(exePath).Name.ToLower() == "ffmpeg.exe")
                        Properties.Settings.Default.FFMPEG_Path = exePath;
                }
            }
        }

        private void SetSkin(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Themes = (sender as RadioButton).Content.ToString();
            Properties.Settings.Default.Save();
            Main main = App.Current.Windows[0] as Main;
            main?.SetSkin();
            main?.SetSelected();
            main?.ActorSetSelected();
        }



        private void SetLanguage(object sender, RoutedEventArgs e)
        {
            //https://blog.csdn.net/fenglailea/article/details/45888799
            Properties.Settings.Default.Language = (sender as RadioButton).Content.ToString();
            Properties.Settings.Default.Save();
            string language = Properties.Settings.Default.Language;
            string hint = "";
            if (language == "English")
                hint = "Take effect after restart";
            else if (language == "日本語")
                hint = "再起動後に有効になります";
            else
                hint = "重启后生效";
            HandyControl.Controls.Growl.Success(hint, "SettingsGrowl");


            //SetLanguageDictionary();


        }

        private void SetLanguageDictionary()
        {
            //设置语言
            string language = Jvedio.Properties.Settings.Default.Language;
            switch (language)
            {
                case "日本語":
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("ja-JP");
                    break;
                case "中文":
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("zh-CN");
                    break;
                case "English":
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;
                default:
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;
            }
            //Jvedio.Language.Resources.Culture.ClearCachedData();
        }

        private void Border_MouseLeftButtonUp1(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(Properties.Settings.Default.Selected_BorderBrush);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.Selected_BorderBrush = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
                Properties.Settings.Default.Save();
            }


        }

        private void Border_MouseLeftButtonUp2(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(Properties.Settings.Default.Selected_BorderBrush);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.Selected_Background = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
                Properties.Settings.Default.Save();
            }

        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            vieModel_Settings.DataBase = e.AddedItems[0].ToString();
            vieModel_Settings.Reset();




        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //设置当前数据库
            for (int i = 0; i < vieModel_Settings.DataBases.Count; i++)
            {
                if (vieModel_Settings.DataBases[i].ToLower() == Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
                {
                    DatabaseComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (vieModel_Settings.DataBases.Count == 1) DatabaseComboBox.Visibility = Visibility.Hidden;

            ShowViewRename(Properties.Settings.Default.RenameFormat);

            SetCheckedBoxChecked();


            foreach (ComboBoxItem item in OutComboBox.Items)
            {
                if (item.Content.ToString() == Properties.Settings.Default.OutSplit)
                {
                    OutComboBox.SelectedIndex = OutComboBox.Items.IndexOf(item);
                    break;
                }
            }


            foreach (ComboBoxItem item in InComboBox.Items)
            {
                if (item.Content.ToString() == Properties.Settings.Default.InSplit)
                {
                    InComboBox.SelectedIndex = InComboBox.Items.IndexOf(item);
                    break;
                }
            }

            //设置皮肤选中
            var rbs = SkinWrapPanel.Children.OfType<RadioButton>().ToList();
            foreach (RadioButton item in rbs)
            {
                if (item.Content.ToString() == Properties.Settings.Default.Themes)
                {
                    item.IsChecked = true;
                    return;
                }
            }

        }

        private void SetCheckedBoxChecked()
        {
            foreach (ToggleButton item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList())
            {
                if (Properties.Settings.Default.RenameFormat.IndexOf(item.Content.ToString().ToSqlField()) >= 0)
                {
                    item.IsChecked = true;
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/hitchao/Jvedio/wiki/HowToSetYoudaoTranslation");
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/hitchao/Jvedio/wiki/HowToSetBaiduAI");
        }

        private void PathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void PathListBox_Drop(object sender, DragEventArgs e)
        {
            if (vieModel_Settings.ScanPath == null) { vieModel_Settings.ScanPath = new ObservableCollection<string>(); }
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var item in dragdropFiles)
            {
                if (!IsFile(item))
                {
                    if (!vieModel_Settings.ScanPath.Contains(item) && !vieModel_Settings.ScanPath.IsIntersectWith(item)) 
                    { 
                        vieModel_Settings.ScanPath.Add(item);
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.FilePathIntersection, "SettingsGrowl");
                    }
                }

            }
            //保存
            FileProcess.SaveScanPathToConfig(vieModel_Settings.DataBase, vieModel_Settings.ScanPath.ToList());

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //选择NFO存放位置
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = Jvedio.Language.Resources.ChooseDir;
            dialog.ShowNewFolderButton = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    System.Windows.MessageBox.Show(this, Jvedio.Language.Resources.Message_CanNotBeNull, Jvedio.Language.Resources.Hint);
                    return;
                }
                else
                {
                    string path = dialog.SelectedPath;
                    if (path.Substring(path.Length - 1, 1) != "\\") { path = path + "\\"; }
                    Properties.Settings.Default.NFOSavePath = path;
                }
            }

        }

        private void ServersDataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (vieModel_Settings.Servers.Count >= 10) return;

            //ServersDataGrid.ItemsSource = null;
            vieModel_Settings.Servers.Add(new Server()
            {
                IsEnable = true,
                Url = "https://",
                Cookie = Jvedio.Language.Resources.Nothing,
                Available = 0,
                ServerTitle = "",
                LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }); ;
            ServersDataGrid.ItemsSource = vieModel_Settings.Servers;

            //TextBox tb = GetVisualChild<TextBox>(GetCell(vieModel_Settings.Servers.Count-1, 1));
            //tb.Focus();
            //tb.SelectAll();

        }


        private int ServersDataGrid_RowIndex = 0;
        private void test_Click(object sender, RoutedEventArgs e)
        {
            FocusTextBox.Focus();
            //UpdateServersEnable();
            var button = (FrameworkElement)sender;
            var row = (DataGridRow)button.Tag;
            int rowIndex = ServersDataGrid_RowIndex;
            Server server = vieModel_Settings.Servers[rowIndex];
            CheckBox cb = GetVisualChild<CheckBox>(GetCell(rowIndex, 0));
            CheckUrl(server, cb);

        }

        //TODO
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            FocusTextBox.Focus();

            Server server = vieModel_Settings.Servers[ServersDataGrid_RowIndex];

            //保存到文件
            if (server.ServerTitle == "JavBus" | server.Url.ToLower() == Properties.Settings.Default.Bus.ToLower())
            {
                Properties.Settings.Default.Bus = "";
                DeleteServerInfoFromConfig(WebSite.Bus);
            }
            else if (server.ServerTitle == "JavBus Europe" | server.Url.ToLower() == Properties.Settings.Default.BusEurope.ToLower())
            {
                Properties.Settings.Default.BusEurope = "";
                DeleteServerInfoFromConfig(WebSite.BusEu);
            }

            else if (server.ServerTitle == "JavDB" | server.Url.ToLower() == Properties.Settings.Default.DB.ToLower())
            {
                Properties.Settings.Default.DB = "";
                Properties.Settings.Default.DBCookie = "";
                DeleteServerInfoFromConfig(WebSite.DB);
            }

            else if (server.ServerTitle == "JavLibrary" | server.Url.ToLower() == Properties.Settings.Default.Library.ToLower())
            {
                Properties.Settings.Default.Library = "";
                DeleteServerInfoFromConfig(WebSite.Library);
            }

            else if (server.ServerTitle == "FANZA" | server.Url.ToLower() == Properties.Settings.Default.DMM.ToLower())
            {
                Properties.Settings.Default.DMM = "";
                Properties.Settings.Default.DMMCookie = "";
                DeleteServerInfoFromConfig(WebSite.DMM);
            }

            else if (server.ServerTitle == "FC2官网" || server.ServerTitle == "FC2" | server.Url.ToLower() == Properties.Settings.Default.FC2.ToLower())
            {
                Properties.Settings.Default.FC2 = "";
                DeleteServerInfoFromConfig(WebSite.FC2);
            }

            else if (server.ServerTitle == "JAV321" | server.Url.ToLower() == Properties.Settings.Default.Jav321.ToLower())
            {
                Properties.Settings.Default.Jav321 = "";
                DeleteServerInfoFromConfig(WebSite.Jav321);
            }

            else if (server.ServerTitle == "AVMOO" | server.Url.ToLower() == Properties.Settings.Default.MOO.ToLower())
            {
                Properties.Settings.Default.MOO = "";
                Properties.Settings.Default.MOOCookie = "";
                DeleteServerInfoFromConfig(WebSite.MOO);
            }

            //如果本地没有，则判断 properties



            vieModel_Settings.Servers.RemoveAt(ServersDataGrid_RowIndex);
            ServersDataGrid.ItemsSource = vieModel_Settings.Servers;
            ServersDataGrid.Items.Refresh();










        }


        private void Previe_Mouse_LBtnDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null) { return; }

            ServersDataGrid_RowIndex = dgr.GetIndex();
        }

        //TODO
        private async void CheckUrl(Server server, CheckBox checkBox)
        {
            ServersDataGrid.ItemsSource = vieModel_Settings.Servers;
            server.LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            server.Available = 2;
            ServersDataGrid.Items.Refresh();
            if (server.Url.IndexOf("http") < 0) server.Url = "https://" + server.Url;
            if (!server.Url.EndsWith("/")) server.Url = server.Url + "/";

            bool enablecookie = false;
            if (server.ServerTitle == "FANZA" || server.ServerTitle == "JavDB" || server.ServerTitle=="AVMOO") enablecookie = true;

            (bool result, string title) = await Net.TestAndGetTitle(server.Url, enablecookie, server.Cookie, server.ServerTitle);
            if (!result && title.IndexOf("JavDB") >= 0)
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_TestError, "SettingsGrowl");
                return;
            }
            if (result && title != "")
            {
                server.Available = 1;

                if (title.IndexOf("JavBus") >= 0 && title.IndexOf("歐美") < 0)
                {
                    server.ServerTitle = "JavBus";
                }
                else if (title.IndexOf("JavBus") >= 0 && title.IndexOf("歐美") >= 0)
                {
                    server.ServerTitle = "JavBus Europe";
                }
                else if (title.IndexOf("JavDB") >= 0)
                {
                    server.ServerTitle = "JavDB";
                }
                else if (title.IndexOf("JavLibrary") >= 0)
                {
                    server.ServerTitle = "JavLibrary";
                }
                else if (title.IndexOf("FANZA") >= 0)
                {
                    server.ServerTitle = "FANZA";
                    if (server.Url.EndsWith("top/")) server.Url = server.Url.Replace("top/", "");
                }
                else if (title.IndexOf("FC2コンテンツマーケット") >= 0 || title.IndexOf("FC2电子市场") >= 0)
                {
                    server.ServerTitle = "FC2";
                }
                else if (title.IndexOf("JAV321") >= 0)
                {
                    server.ServerTitle = "JAV321";
                }
                else if (title.IndexOf("AVMOO") >= 0)
                {
                    server.ServerTitle = "AVMOO";
                }
                else
                {
                    server.ServerTitle = title;
                }


            }
            else
            {
                server.Available = -1;
            }
            ServersDataGrid.Items.Refresh();

            //保存，重复的会覆盖
            if (server.ServerTitle == "JavBus")
            {
                Properties.Settings.Default.Bus = server.Url;
                Properties.Settings.Default.EnableBus = (bool)checkBox.IsChecked;

                SaveServersInfoToConfig(WebSite.Bus, new List<string>() { server.Url, server.ServerTitle, server.LastRefreshDate });
            }
            else if (server.ServerTitle == "JavBus Europe")
            {
                Properties.Settings.Default.BusEurope = server.Url;
                Properties.Settings.Default.EnableBusEu = (bool)checkBox.IsChecked;
                SaveServersInfoToConfig(WebSite.BusEu, new List<string>() { server.Url, server.ServerTitle, server.LastRefreshDate });
            }
            else if (server.ServerTitle == "JavDB")
            {
                //是否包含 cookie
                if (server.Cookie == Jvedio.Language.Resources.Nothing || server.Cookie == "")
                {
                    new Msgbox(this, Jvedio.Language.Resources.Message_NeedCookies).ShowDialog();
                }
                else
                {
                    Properties.Settings.Default.DB = server.Url;
                    Properties.Settings.Default.EnableDB = (bool)checkBox.IsChecked;
                    Properties.Settings.Default.DBCookie = server.Cookie;
                    SaveServersInfoToConfig(WebSite.DB, new List<string>() { server.Url, server.ServerTitle, server.LastRefreshDate });
                }

            }
            else if (server.ServerTitle == "JavLibrary")
            {
                Properties.Settings.Default.Library = server.Url;
                Properties.Settings.Default.EnableLibrary = (bool)checkBox.IsChecked;
                SaveServersInfoToConfig(WebSite.Library, new List<string>() { server.Url, server.ServerTitle, server.LastRefreshDate });
            }
            else if (server.ServerTitle == "FANZA")
            {
                //是否包含 cookie
                if (server.Cookie == Jvedio.Language.Resources.Nothing || server.Cookie == "")
                {
                    new Msgbox(this, Jvedio.Language.Resources.Message_NeedCookies).ShowDialog();
                }
                else
                {
                    Properties.Settings.Default.DMM = server.Url;
                    Properties.Settings.Default.EnableDMM = (bool)checkBox.IsChecked;
                    Properties.Settings.Default.DMMCookie = server.Cookie;
                    SaveServersInfoToConfig(WebSite.DMM, new List<string>() { server.Url, server.ServerTitle, server.LastRefreshDate });
                }
            }
            else if (server.ServerTitle == "FC2官网" || server.ServerTitle == "FC2")
            {
                Properties.Settings.Default.FC2 = server.Url;
                Properties.Settings.Default.EnableFC2 = (bool)checkBox.IsChecked;
                if (server.Cookie != Jvedio.Language.Resources.Nothing && server.Cookie != "") Properties.Settings.Default.FC2Cookie = server.Cookie;
                SaveServersInfoToConfig(WebSite.FC2, new List<string>() { server.Url, "FC2", server.LastRefreshDate });
            }
            else if (server.ServerTitle == "JAV321")
            {
                Properties.Settings.Default.Jav321 = server.Url;
                Properties.Settings.Default.Enable321 = (bool)checkBox.IsChecked;
                SaveServersInfoToConfig(WebSite.Jav321, new List<string>() { server.Url, server.ServerTitle, server.LastRefreshDate });
            }
            else if (server.ServerTitle == "AVMOO")
            {
                //是否包含 cookie
                if (server.Cookie == Jvedio.Language.Resources.Nothing || server.Cookie == "")
                {
                    new Msgbox(this, Jvedio.Language.Resources.Message_NeedCookies).ShowDialog();
                }
                else
                {
                    Properties.Settings.Default.MOO = server.Url;
                    Properties.Settings.Default.EnableMOO = (bool)checkBox.IsChecked;
                    Properties.Settings.Default.MOOCookie = server.Cookie;
                    SaveServersInfoToConfig(WebSite.MOO, new List<string>() { server.Url, server.ServerTitle, server.LastRefreshDate });
                }
            }
            Properties.Settings.Default.Save();

        }




        public static T GetVisualChild<T>(Visual parent) where T : Visual

        {

            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < numVisuals; i++)

            {

                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);

                child = v as T;

                if (child == null)

                {

                    child = GetVisualChild<T>

                    (v);

                }

                if (child != null)

                {

                    break;

                }

            }

            return child;

        }

        public DataGridCell GetCell(int row, int column)

        {

            DataGridRow rowContainer = GetRow(row);

            if (rowContainer != null)

            {

                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                if (presenter == null)

                {

                    ServersDataGrid.ScrollIntoView(rowContainer, ServersDataGrid.Columns[column]);

                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);

                return cell;

            }

            return null;

        }

        public DataGridRow GetRow(int index)

        {

            DataGridRow row = (DataGridRow)ServersDataGrid.ItemContainerGenerator.ContainerFromIndex(index);

            if (row == null)

            {

                ServersDataGrid.UpdateLayout();

                ServersDataGrid.ScrollIntoView(ServersDataGrid.Items[index]);

                row = (DataGridRow)ServersDataGrid.ItemContainerGenerator.ContainerFromIndex(index);

            }

            return row;

        }

        //TODO
        private void CheckBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            bool enable = !(bool)((CheckBox)sender).IsChecked;
            vieModel_Settings.Servers[ServersDataGrid_RowIndex].IsEnable = enable;

            if (vieModel_Settings.Servers[ServersDataGrid_RowIndex].ServerTitle == "JavBus")
                Properties.Settings.Default.EnableBus = enable;
            else if (vieModel_Settings.Servers[ServersDataGrid_RowIndex].ServerTitle == "JavBus Europe")
                Properties.Settings.Default.EnableBusEu = enable;
            else if (vieModel_Settings.Servers[ServersDataGrid_RowIndex].ServerTitle == "JavDB")
                Properties.Settings.Default.EnableDB = enable;
            else if (vieModel_Settings.Servers[ServersDataGrid_RowIndex].ServerTitle == "JavLibrary")
                Properties.Settings.Default.EnableLibrary = enable;
            else if (vieModel_Settings.Servers[ServersDataGrid_RowIndex].ServerTitle == "FANZA")
                Properties.Settings.Default.EnableDMM = enable;
            else if (vieModel_Settings.Servers[ServersDataGrid_RowIndex].ServerTitle == "JAV321")
                Properties.Settings.Default.Enable321 = enable;
            else if (vieModel_Settings.Servers[ServersDataGrid_RowIndex].ServerTitle == "AVMOO")
                Properties.Settings.Default.EnableMOO = enable;
            else if (vieModel_Settings.Servers[ServersDataGrid_RowIndex].ServerTitle == "FC2官网" || vieModel_Settings.Servers[ServersDataGrid_RowIndex].ServerTitle == "FC2")
                Properties.Settings.Default.EnableFC2 = enable;

            Properties.Settings.Default.Save();
            InitVariable();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //注册热键
            uint modifier = Properties.Settings.Default.HotKey_Modifiers;
            uint vk = Properties.Settings.Default.HotKey_VK;

            if (modifier != 0 && vk != 0)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk);
                if (!success)
                {
                    MessageBox.Show(Jvedio.Language.Resources.BossKeyError, Jvedio.Language.Resources.Hint);
                    Properties.Settings.Default.HotKey_Enable = false;
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
        }


        private void ReplaceWithValue(string property)
        {
            string inSplit = InComboBox.Text.Replace(Jvedio.Language.Resources.Nothing, "");
            PropertyInfo[] PropertyList = SampleMovie.GetType().GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                if (name == property)
                {
                    object o = item.GetValue(SampleMovie);
                    if (o != null)
                    {
                        string value = o.ToString();

                        if (property == "actor" || property == "genre" || property == "label")
                            value = value.Replace(" ", inSplit).Replace("/", inSplit);

                        if (property == "vediotype")
                        {
                            int v = 1;
                            int.TryParse(value, out v);
                            if (v == 1)
                                value = Jvedio.Language.Resources.Uncensored;
                            else if (v == 2)
                                value = Jvedio.Language.Resources.Censored;
                            else if (v == 3)
                                value = Jvedio.Language.Resources.Europe;
                        }
                        vieModel_Settings.ViewRenameFormat = vieModel_Settings.ViewRenameFormat.Replace("{" + property + "}", value);
                    }
                    break;
                }
            }
        }

        private void AddToRename(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            string text = toggleButton.Content.ToString();
            bool ischecked = (bool)toggleButton.IsChecked;
            string formatstring = "{" + text.ToSqlField() + "}";

            string split = OutComboBox.Text.Replace(Jvedio.Language.Resources.Nothing, "");


            if (ischecked)
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.RenameFormat))
                {
                    Properties.Settings.Default.RenameFormat += formatstring;
                }
                else
                {
                    Properties.Settings.Default.RenameFormat += split + formatstring;
                }
            }
            else
            {
                int idx = Properties.Settings.Default.RenameFormat.IndexOf(formatstring);
                if (idx == 0)
                {
                    Properties.Settings.Default.RenameFormat = Properties.Settings.Default.RenameFormat.Replace(formatstring, "");
                }
                else
                {
                    Properties.Settings.Default.RenameFormat = Properties.Settings.Default.RenameFormat.Replace(getSplit(formatstring) + formatstring, "");
                }
            }
        }

        private char getSplit(string formatstring)
        {
            int idx = Properties.Settings.Default.RenameFormat.IndexOf(formatstring);
            if (idx > 0)
                return Properties.Settings.Default.RenameFormat[idx - 1];
            else
                return '\0';

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vieModel_Settings == null) return;
            TextBox textBox = (TextBox)sender;
            string txt = textBox.Text;
            ShowViewRename(txt);
        }

        private void ShowViewRename(string txt)
        {

            MatchCollection matches = Regex.Matches(txt, "\\{[a-z]+\\}");
            if (matches != null && matches.Count > 0)
            {
                vieModel_Settings.ViewRenameFormat = txt;
                foreach (Match match in matches)
                {
                    string property = match.Value.Replace("{", "").Replace("}", "");
                    ReplaceWithValue(property);
                }
            }
            else
            {
                vieModel_Settings.ViewRenameFormat = "";
            }
        }

        private void OutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            Properties.Settings.Default.OutSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
        }

        private void InComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            Properties.Settings.Default.InSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
        }

        private void SetBackgroundImage(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.Choose;
            OpenFileDialog1.FileName = "background.jpg";
            OpenFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            OpenFileDialog1.Filter = "jpg|*.jpg";
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = OpenFileDialog1.FileName;
                if (File.Exists(path))
                {
                    //设置背景
                    GlobalVariable.BackgroundImage = null;
                    GC.Collect();
                    GlobalVariable.BackgroundImage = ImageProcess.BitmapImageFromFile(path);

                    Properties.Settings.Default.BackgroundImage = path;
                    Main main = ((Main)GetWindowByName("Main"));
                    if (main != null) main.SetSkin();

                    WindowDetails windowDetails = ((WindowDetails)GetWindowByName("WindowDetails"));
                    if (windowDetails != null) windowDetails.SetSkin();
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.SettingsIndex = TabControl.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            try
            {
                Clipboard.SetText(ffmpeg_url);
                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.SuccessCopyUrl, "SettingsGrowl");
            }
            catch (Exception ex) {
                new DialogInput(this, Jvedio.Language.Resources.CopyToDownload, ffmpeg_url).ShowDialog();
                Console.WriteLine(ex.Message);
            }

        }

        private void LoadTranslate(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("youdao.key")) return;
            string v = GetValueKey("youdao.key");
            if(v.Split(' ').Length == 2)
            {
                Properties.Settings.Default.TL_YOUDAO_APIKEY = v.Split(' ')[0];
                Properties.Settings.Default.TL_YOUDAO_SECRETKEY = v.Split(' ')[1];
            } 
        }


        public string GetValueKey(string filename)
        {
            string v = "";
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    v = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            if (v != "")
                return Encrypt.AesDecrypt(v, EncryptKeys[0]);
            else
                return "";
        }

        private void LoadAI(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("BaiduAI.key")) return;
            string v = GetValueKey("BaiduAI.key");
            if (v.Split(' ').Length == 2)
            {
                Properties.Settings.Default.Baidu_API_KEY = v.Split(' ')[0];
                Properties.Settings.Default.Baidu_SECRET_KEY = v.Split(' ')[1];
            }
        }
    }


}
