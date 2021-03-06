﻿using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMethod;
using static Jvedio.ImageProcess;
using static Jvedio.FileProcess;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;

namespace Jvedio
{
    /// <summary>
    /// WindowDetails.xaml 的交互逻辑
    /// </summary>
    public partial class WindowDetails : Window
    {


        public VieModel_Details vieModel;
        public Point WindowPoint = new Point(100, 100);
        public Size WindowSize = new Size(1200, 700);
        public JvedioWindowState WinState = JvedioWindowState.Normal;

        public DetailDownLoad DetailDownLoad;
        public List<string> MovieIDs = new List<string>();
        public string MovieID = "";
        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance=null;

        public WindowDetails(string movieid = "")
        {
            //movieid = "IPX-163";
            InitializeComponent();
            MovieID = movieid;
            this.Height = SystemParameters.PrimaryScreenHeight *0.8;
            this.Width = SystemParameters.PrimaryScreenHeight * 0.8 * 1230/720;
            ProgressBar.Visibility = Visibility.Collapsed;
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported ) taskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;

            //this.Loaded += delegate { FadeIn(); };
        }

        public void FadeIn()
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                var anim = new DoubleAnimation(0, 1, (Duration)FadeInterval, FillBehavior.Stop);
                anim.Completed += (s, _) => this.Opacity = 1;
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            }
            else
            {
                this.Opacity = 1;
            }

        }



        public void SetSkin()
        {
            switch (Properties.Settings.Default.Themes)
            {
                case "蓝色":
                    //设置渐变
                    LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
                    myLinearGradientBrush.StartPoint = new Point(0.5, 0);
                    myLinearGradientBrush.EndPoint = new Point(0.5, 1);
                    myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(62, 191, 223), 1));
                    myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(11, 114, 189), 0));
                    BackBorder.Background = myLinearGradientBrush;
                    break;
            }

            //设置背景

            BackGroundImage.Source = GlobalVariable.BackgroundImage;
            MouseMoveGrid.Background = Brushes.Transparent;
            ImageGrid.Background = Brushes.Transparent;
            ExtraImageBorder.Background = Brushes.Transparent;
            TitleTextBox.Background = Brushes.Transparent;
            InfoBorder.Background = (SolidColorBrush)Application.Current.Resources["BackgroundMain"];
            InfoBorder.Opacity = 0.7;
            ExtraImageRadioButton.Opacity = 0.7;
            ScreenShotRadioButton.Opacity = 0.7;
            NextBorder.Opacity = 0.7;
            PreviousBorder. Opacity = 0.7;
        }


        public void ActorMouseMove(object sender, RoutedEventArgs e)
        {
            ActorPopup.IsOpen = true;
            ShowActor(sender, e);

        }

        public void ShowActor(object sender, RoutedEventArgs e)
        {

            Label label = sender as Label;
            string name = label.Content.ToString();
            string imagePath = BasePicPath + $"Actresses\\{name}.jpg";
            if (File.Exists(imagePath))
                ActorImage.Source = GetBitmapImage(name, "Actresses");
            else
                ActorImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Resources/Picture/NoPrinting_A.png", UriKind.Relative));

        }

        public void HideActor(object sender, RoutedEventArgs e)
        {
            ActorPopup.IsOpen = false;
        }

        public void DownLoad(object sender, RoutedEventArgs e)
        {
            if (!JvedioServers.IsProper())
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_UrlNotSet, "DetailsGrowl");

            }
            else
            {
                if (DetailDownLoad == null)
                {
                    Task.Run(() => { StartDownload(); });
                }
                else
                {
                    if (!DetailDownLoad.IsDownLoading)
                    {
                        Task.Run(() => { StartDownload(); });
                    }
                }
            }
        }

        public async void GetScreenShot(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetFFmpeg, "DetailsGrowl"); return; }

            if (((MenuItem)(sender)).Parent is MenuItem mnu)
            {
                DetailMovie movie = vieModel.DetailMovie;

                if (!File.Exists(movie.filepath)) { HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_FileNotExist, "DetailsGrowl"); return; }

                bool success = false;
                string message = "";

                ScreenShotRadioButton.IsChecked = true;
                ExtraImageRadioButton.IsChecked = false;
                vieModel.DetailMovie.extraimagelist = new ObservableCollection<BitmapSource>();
                vieModel.DetailMovie.extraimagePath = new ObservableCollection<string>();
                App.Current.Dispatcher.Invoke((Action)delegate { imageItemsControl.ItemsSource = vieModel.DetailMovie.extraimagelist; });

                ScreenShot screenShot = new ScreenShot();
                screenShot.SingleScreenShotCompleted += (s, ev) =>
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (Path.GetDirectoryName(((ScreenShotEventArgs)ev).FilePath).Split('\\').Last().ToUpper() != vieModel.DetailMovie.id) return;
                        if (!(bool)ScreenShotRadioButton.IsChecked) return;
                        vieModel.DetailMovie.extraimagePath.Add(((ScreenShotEventArgs)ev).FilePath);
                        vieModel.DetailMovie.extraimagelist.Add(GetExtraImage(((ScreenShotEventArgs)ev).FilePath));
                        imageItemsControl.ItemsSource = vieModel.DetailMovie.extraimagelist;
                        if (vieModel.DetailMovie.extraimagelist.Count == 1) SetImage(0);
                    });
                };
                (success, message) = await screenShot.AsyncScreenShot(movie);

                if (success) HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
                else HandyControl.Controls.Growl.Error(message, "DetailsGrowl");
            }
        }

        public Semaphore SemaphoreScreenShot;
        public int TotalSSNum = 0;
        public int CurrentSSNum = 0;
        public void ScreenShot(Movie movie)
        {
            // n 个线程截图
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) return;

            int num = Properties.Settings.Default.ScreenShot_ThreadNum;
            string ScreenShotPath = "";
            //if (Properties.Settings.Default.ScreenShotToExtraPicPath)
            //    ScreenShotPath = BasePicPath + "ExtraPic\\" + movie.id;
            //else
                ScreenShotPath = BasePicPath + "ScreenShot\\" + movie.id;

            if (!Directory.Exists(ScreenShotPath)) Directory.CreateDirectory(ScreenShotPath);



            string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
            if (cutoffArray.Length == 0) return;
            SemaphoreScreenShot = new Semaphore(cutoffArray.Count(), cutoffArray.Count());
            TotalSSNum = cutoffArray.Count();
            CurrentSSNum = 0;
            for (int i = 0; i < cutoffArray.Count(); i++)
            {
                List<object> list = new List<object>() { cutoffArray[i], i.ToString(), movie.filepath, ScreenShotPath };
                Thread threadObject = new Thread(BeginScreenShot);
                threadObject.Start(list);
            }
            //HandyControl.Controls.Growl.Info($"已启用 {cutoffArray.Count()} 个线程， 3-10S 后即可截图成功\n", "DetailsGrowl");
        }

        public void BeginScreenShot(object o)
        {
            App.Current.Dispatcher.Invoke((Action)delegate { this.Cursor = Cursors.Wait; });
            
            List<object> list = o as List<object>;
            string cutoffTime = list[0] as string;
            string i = list[1] as string;
            string filePath = list[2] as string;
            string ScreenShotPath = list[3] as string;

            if (string.IsNullOrEmpty(cutoffTime)) return;
            SemaphoreScreenShot.WaitOne();
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            string outputfile= $"{ ScreenShotPath }\\ScreenShot -{ i.PadLeft(2, '0')}.jpg";
            string str = $"\"{Properties.Settings.Default.FFMPEG_Path}\" -y -threads 1 -ss {cutoffTime} -i \"{filePath}\" -f image2 -frames:v 1 \"{outputfile}\"";

            p.StandardInput.WriteLine(str + "&exit");
            p.StandardInput.AutoFlush = true;
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            SemaphoreScreenShot.Release();
            CurrentSSNum++;
            App.Current.Dispatcher.Invoke((Action)delegate { this.Cursor = Cursors.Arrow;  });

            if (CurrentSSNum == TotalSSNum)
            {
                try
                {
                    App.Current.Dispatcher.Invoke((Action)delegate {
                        vieModel.Query(vieModel.DetailMovie.id);
                        SetImage(0);
                    });
                }
                catch { }

            }


        }


        public void StartDownload()
        {
            List<string> urlList = new List<string>();
            foreach (var item in vieModel.DetailMovie.extraimageurl?.Split(';')) { if (!string.IsNullOrEmpty(item)) urlList.Add(item); }
            if (vieModel.DetailMovie.extraimagelist.Count-1 >= urlList.Count && vieModel.DetailMovie.bigimage != null && vieModel.DetailMovie.title != "" && vieModel.DetailMovie.sourceurl!="") return;



            //添加到下载列表
            DetailDownLoad = new DetailDownLoad(vieModel.DetailMovie);
            DetailDownLoad.DownLoad();
            Dispatcher.Invoke((Action)delegate () { ProgressBar.Value = 0; ProgressBar.Visibility = Visibility.Visible; });

            //监听取消下载：
            DetailDownLoad.CancelEvent += (s, e) => { Dispatcher.Invoke((Action)delegate () { ProgressBar.Visibility = Visibility.Collapsed; }); };

            //显示详细信息
            DetailDownLoad.InfoDownloadCompleted += (s, e) => {
                MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
                if (vieModel.DetailMovie.id == eventArgs.Message)
                {
                    //判断是否是当前番号

                    //vieModel.DetailMovie = new DetailMovie();
                    DetailMovie detailMovie = DataBase.SelectDetailMovieById(eventArgs.Message);
                    if (detailMovie != null)
                    {
                        MySqlite db = new MySqlite("Translate");
                        //加载翻译结果
                        if (Properties.Settings.Default.TitleShowTranslate)
                        {
                            string translate_title = db.GetInfoBySql($"select translate_title from youdao where id='{detailMovie.id}'");
                            if (translate_title != "") detailMovie.title = translate_title;
                        }

                        if (Properties.Settings.Default.PlotShowTranslate)
                        {
                            string translate_plot = db.GetInfoBySql($"select translate_plot from youdao where id='{detailMovie.id}'");
                            if (translate_plot != "") detailMovie.plot = translate_plot;
                        }
                        db.CloseDB();

                        //显示新增按钮
                        List<string> labels = detailMovie.labellist;
                        detailMovie.labellist = new List<string>();
                        detailMovie.labellist.Add("+");
                        detailMovie.labellist.AddRange(labels);
                        detailMovie.extraimagelist = vieModel.DetailMovie.extraimagelist;
                        detailMovie.extraimagePath = vieModel.DetailMovie.extraimagePath;
                        detailMovie.bigimage = vieModel.DetailMovie.bigimage;

                        vieModel.DetailMovie = detailMovie;
                        vieModel.VedioInfo = MediaParse.GetMediaInfo(detailMovie.filepath);
                    }

                    //显示到主界面
                    this.Dispatcher.Invoke((Action)delegate { 
                        Main main = App.Current.Windows[0] as Main;
                        main.RefreshMovieByID(eventArgs.Message);
                    });
                }

            };

            //进度
            DetailDownLoad.InfoUpdate += (s, e) =>
            {
                Dispatcher.Invoke((Action)delegate ()
                {
                    DetailMovieEventArgs eventArgs = e as DetailMovieEventArgs;
                    ProgressBar.Value = (eventArgs.value / eventArgs.maximum) * 100; ProgressBar.Visibility = Visibility.Visible;
                    if (ProgressBar.Value == ProgressBar.Maximum) ProgressBar.Visibility = Visibility.Collapsed;
                });
            };


            //显示错误消息
            DetailDownLoad.MessageCallBack += (s, e) =>
            {
                Dispatcher.Invoke((Action)delegate ()
                {
                    MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
                    HandyControl.Controls.Growl.Error(eventArgs.Message, "DetailsGrowl");
                });
            };



            //显示大图
            DetailDownLoad.BigImageDownLoadCompleted += (s, e) =>
            {
                if (!File.Exists(BasePicPath + $"BigPic\\{vieModel.DetailMovie.id}.jpg")) return;
                MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
                if (vieModel.DetailMovie.id == eventArgs.Message)
                {
                    Dispatcher.Invoke((Action)delegate ()
                {
                    vieModel.DetailMovie.bigimage = null;
                    vieModel.DetailMovie.bigimage = GetBitmapImage(vieModel.DetailMovie.id, "BigPic");
                    BigImage.Source = vieModel.DetailMovie.bigimage;
                    if (vieModel.DetailMovie.extraimagelist.Count == 0)
                    {
                        vieModel.DetailMovie.extraimagelist = new ObservableCollection<BitmapSource>();
                        vieModel.DetailMovie.extraimagePath = new ObservableCollection<string>();
                    }
                    vieModel.DetailMovie.extraimagelist.Insert(0, vieModel.DetailMovie.bigimage);
                    vieModel.DetailMovie.extraimagePath.Insert(0, BasePicPath + $"BigPic\\{vieModel.DetailMovie.id}.jpg");
                    imageItemsControl.ItemsSource = vieModel.DetailMovie.extraimagelist;

                });
                }

                Dispatcher.Invoke((Action)delegate ()
                {
                    //显示到主界面
                    Main main = App.Current.Windows[0] as Main;
                    main.RefreshMovieByID(vieModel.DetailMovie.id);
                });
            };


            //显示小图
            DetailDownLoad.SmallImageDownLoadCompleted += (s, e) =>
            {
                if (!File.Exists(BasePicPath + $"SmallPic\\{vieModel.DetailMovie.id}.jpg")) return;
                MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
                Dispatcher.Invoke((Action)delegate ()
                {
                    //显示到主界面
                    Main main = App.Current.Windows[0] as Main;
                    main.RefreshMovieByID(vieModel.DetailMovie.id);
                });
            };


            //显示预览图
            DetailDownLoad.ExtraImageDownLoadCompleted += (s, e) =>
            {
                Dispatcher.Invoke((Action)delegate ()
                {
                    if ((bool)ScreenShotRadioButton.IsChecked) return;
                    MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
                    if (!File.Exists(eventArgs.Message)) return;
                    vieModel.DetailMovie.extraimagelist.Add(GetExtraImage(eventArgs.Message));
                    vieModel.DetailMovie.extraimagePath.Add(eventArgs.Message);
                    imageItemsControl.ItemsSource = vieModel.DetailMovie.extraimagelist;
                    
                });
            };


        }

        public void StopDownLoad()
        {
            if (DetailDownLoad != null && DetailDownLoad.IsDownLoading == true) HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_CancelCurrentTask, "DetailsGrowl");
            DetailDownLoad?.CancelDownload();
        }




        //显示预览图


        public void HeartEnter(object sender, RoutedEventArgs e)
        {

            System.Windows.Shapes.Path path = (System.Windows.Shapes.Path)sender;
            var siblings = ((sender as FrameworkElement).Parent as Canvas).Children;
            var paths = siblings.OfType<System.Windows.Shapes.Path>().ToList();

            if (Mouse.DirectlyOver != null)
            {
                if (paths.IndexOf(path) == 0) { path.Fill = Brushes.DarkGray; } else { paths[0].Fill = Brushes.Gray; }
                for (int i = 1; i <= paths.IndexOf(path); i++)
                {
                    paths[i].Fill = Brushes.Red;
                }
                for (int i = paths.IndexOf(path) + 1; i <= 5; i++)
                {
                    paths[i].Fill = Brushes.White;
                }
            }

        }

        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        Main windowMain = GetWindowByName("Main") as Main;


        //显示类别
        public void ShowSameGenre(object sender, MouseButtonEventArgs e)
        {
            string genretext = ((TextBlock)sender).Text;
            if (string.IsNullOrEmpty(genretext)) return;
            windowMain.Genre_MouseDown(sender, e);
            this.Close();
            

        }

        //显示演员
        public void ShowSameActor(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;
            string name = label.Content.ToString();
            if (string.IsNullOrEmpty(name)) return;
            Actress actress = vieModel.DetailMovie.actorlist.Where(arg => arg.name == name).First();
            if (actress != null)
            {
                actress.id = "";
                windowMain.ShowActorMovieFromDetailWindow(actress);
                this.Close();
            }


        }

        //显示标签
        public void ShowSameLabel(object sender, MouseButtonEventArgs e)
        {

            string tagtext = ((HandyControl.Controls.Tag)sender).Content.ToString();

            if (string.IsNullOrEmpty(tagtext)) return;

            if (tagtext == "+")
            {
                //新增
                var r = new DialogInput(this, Jvedio.Language.Resources.EnterLabel);
                if (r.ShowDialog() == true)
                {
                    string text = r.Text;
                    if (text != "" & text != "+" & text.IndexOf(" ") < 0)
                    {
                        if (!vieModel.DetailMovie.labellist.Contains(text))
                        {
                            vieModel.DetailMovie.labellist.Add(text);
                            vieModel.SaveLabel();
                            vieModel.Query(vieModel.DetailMovie.id);
                            //显示到主界面
                            Main main = App.Current.Windows[0] as Main;
                            main.RefreshMovieByID(vieModel.DetailMovie.id);
                        }

                    }

                }
            }
            else
            {
                windowMain.Label_MouseDown(sender, e);
                this.Close();
            }
        }

        //显示导演
        public void ShowSameDirector(object sender, MouseButtonEventArgs e)
        {
            string genretext = ((Label)sender).Content.ToString();
            if (string.IsNullOrEmpty(genretext)) return;
            windowMain.Director_MouseDown(sender, e);
                this.Close();
            

        }

        //显示发行商
        public void ShowSameStudio(object sender, MouseButtonEventArgs e)
        {
            string genretext = ((Label)sender).Content.ToString();
            if (string.IsNullOrEmpty(genretext)) return;
            windowMain.Studio_MouseDown(sender, e);
                this.Close();
            

        }

        //显示系列
        public void ShowSameTag(object sender, MouseButtonEventArgs e)
        {
            string genretext = ((Label)sender).Content.ToString();
            if (string.IsNullOrEmpty(genretext)) return;
            windowMain.Tag_MouseDown(sender, e);
            this.Close();
            
        }


        public WindowEdit WindowEdit;
        public void EditInfo(object sender, RoutedEventArgs e)
        {

            if (WindowEdit != null)  WindowEdit.Close(); 
            WindowEdit = new WindowEdit(vieModel.DetailMovie.id);
            WindowEdit.Show();
        }



        public void PlayVedio(object sender, MouseEventArgs e)
        {

            if (vieModel.DetailMovie.hassubsection)
            {
                ShowSubsection(this, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            }
            else
            {
                string filepath = vieModel.DetailMovie.filepath;
                if (File.Exists(filepath))
                {
                    Main main = App.Current.Windows[0] as Main;
                    //使用默认播放器
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.VedioPlayerPath) && File.Exists(Properties.Settings.Default.VedioPlayerPath))
                    {
                        try
                        {
                            Process.Start(Properties.Settings.Default.VedioPlayerPath, filepath);
                            main.vieModel.AddToRecentWatch(vieModel.DetailMovie.id);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogE(ex);
                            Process.Start(filepath);
                        }

                    }
                    else
                    {
                        Process.Start(filepath);
                        main.vieModel.AddToRecentWatch(vieModel.DetailMovie.id);
                    }
                }
                else
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.CannotOpen + filepath, "DetailsGrowl");
            }
        }

        private void MoveWindow(object sender, MouseEventArgs e)
        {
            //移动窗口
            if (e.LeftButton == MouseButtonState.Pressed && WinState == JvedioWindowState.Normal)
            {
                this.DragMove();
            }

            //FatherGrid.Focus();
        }

        private void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        public void PreviewMovie(object sender, MouseButtonEventArgs e)
        {

            StopDownLoad();
            string id = "";
            //加载所有影片
            if (Properties.Settings.Default.DetialWindowShowAllMovie &&  MovieIDs.Count <= 0)
            {
                MovieIDs = DataBase.SelectPartialInfo("SELECT * FROM movie").Select(arg=> arg.id).ToList();


            }

            if (!Properties.Settings.Default.DetialWindowShowAllMovie) {

                windowMain = App.Current.Windows[0] as Main;

                for (int i = 0; i < windowMain.vieModel.CurrentMovieList.Count; i++)
                {
                    if (vieModel.DetailMovie.id.ToLower() == windowMain.vieModel.CurrentMovieList[i].id.ToLower())
                    {
                        if (i == 0) { id = windowMain.vieModel.CurrentMovieList[windowMain.vieModel.CurrentMovieList.Count - 1].id; }
                        else { id = windowMain.vieModel.CurrentMovieList[i - 1].id; }
                        break;
                    }
                }

            }
            else
            {
                for (int i = 0; i < MovieIDs.Count; i++)
                {
                    if (vieModel.DetailMovie.id.ToLower() == MovieIDs[i].ToLower())
                    {
                        if (i == 0) { id = MovieIDs[MovieIDs.Count - 1]; }
                        else { id = MovieIDs[i - 1]; }
                        break;
                    }
                }
            }





            if (id != "")
            {
                vieModel.CleanUp();
                vieModel.Query(id);
                vieModel.SelectImageIndex = 0;
            }


        }

        public void NextMovie(object sender, MouseButtonEventArgs e)
        {
            StopDownLoad();

            
            string id = "";
            //加载所有影片
            if (Properties.Settings.Default.DetialWindowShowAllMovie && MovieIDs.Count <= 0)
            {
                MovieIDs = DataBase.SelectPartialInfo("SELECT * FROM movie").Select(arg => arg.id).ToList();
            }

            if (!Properties.Settings.Default.DetialWindowShowAllMovie)
            {
                windowMain = App.Current.Windows[0] as Main;

                for (int i = 0; i < windowMain.vieModel.CurrentMovieList.Count; i++)
                {
                    if (vieModel.DetailMovie.id == windowMain.vieModel.CurrentMovieList[i].id)
                    {
                        if (i == windowMain.vieModel.CurrentMovieList.Count - 1) { id = windowMain.vieModel.CurrentMovieList[0].id; }
                        else { id = windowMain.vieModel.CurrentMovieList[i + 1].id; }
                        break;
                    }
                }
            }
            else
            {

                for (int i = 0; i < MovieIDs.Count; i++)
                {
                    if (vieModel.DetailMovie.id.ToLower() == MovieIDs[i].ToLower())
                    {
                        if (i == MovieIDs.Count - 1) { id = MovieIDs[0]; }
                        else { id = MovieIDs[i + 1]; }
                        break;
                    }
                }
            }

            if (id != "")
            {
                vieModel.CleanUp();
                vieModel.Query(id);
                vieModel.SelectImageIndex = 0;
            }
            


        }


        private void ShowTagStamps()
        {
            var labels = TagStampsStackPanel.Children.OfType<Label>().ToList();
            if (vieModel.DetailMovie.tagstamps.IndexOfAnyString(TagStrings_HD) >= 0)
                labels[0].Visibility = Visibility.Visible;
            else
                labels[0].Visibility = Visibility.Collapsed;

            if (vieModel.DetailMovie.tagstamps.IndexOfAnyString(TagStrings_Translated) >= 0)
                labels[1].Visibility = Visibility.Visible;
            else
                labels[1].Visibility = Visibility.Collapsed;

            if (vieModel.DetailMovie.tagstamps.IndexOfAnyString(TagStrings_FlowOut) >= 0)
                labels[2].Visibility = Visibility.Visible;
            else
                labels[2].Visibility = Visibility.Collapsed;
        }



        public void OpenImagePath(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem _mnu = sender as MenuItem;
                if (_mnu.Parent is MenuItem mnu)
                {
                    DetailMovie detailMovie = vieModel.DetailMovie;
                    int index = mnu.Items.IndexOf(_mnu);
                    string filepath = detailMovie.filepath;
                    if (index == 0) { filepath = detailMovie.filepath; }
                    else if (index == 1) { filepath = BasePicPath + $"BigPic\\{detailMovie.id}.jpg"; }
                    else if (index == 2) { filepath = BasePicPath + $"SmallPic\\{detailMovie.id}.jpg"; }
                    else if (index == 3) { filepath = BasePicPath + $"Gif\\{detailMovie.id}.gif"; }
                    else if (index == 4) { filepath = BasePicPath + $"ExtraPic\\{detailMovie.id}\\"; }
                    else if (index == 5) { filepath = BasePicPath + $"ScreenShot\\{detailMovie.id}\\"; }

                    if (index == 4 | index == 5)
                    {
                        if (Directory.Exists(filepath)) { Process.Start("explorer.exe", "\"" + filepath + "\""); }
                        else
                        {
                            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_FileNotExist, "DetailsGrowl");
                        }
                    }
                    else
                    {
                        if (File.Exists(filepath)) { Process.Start("explorer.exe", "/select, \"" + filepath + "\""); }
                        else
                        {
                            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_FileNotExist, "DetailsGrowl");
                        }
                    }

                }

            }
            catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
        }

        public void OpenExtraImagePath(object sender, RoutedEventArgs e)
        {
            //string filepath = BasePicPath + $"ExtraPic\\{vieModel.DetailMovie.id}\\";
            //if ((bool)ScreenShotRadioButton.IsChecked) filepath = BasePicPath + $"ScreenShot\\{vieModel.DetailMovie.id}\\";
            //if (Directory.Exists(filepath)) { Process.Start("explorer.exe", filepath); }
            //else
            //{
            //    HandyControl.Controls.Growl.Info($"打开失败，不存在 ：{filepath}", "DetailsGrowl");
            //}

            MenuItem m1 = sender as MenuItem;

            ContextMenu contextMenu = m1.Parent as ContextMenu;

            Border border = contextMenu.PlacementTarget as Border;
            Grid grid = border.Parent as Grid;
            TextBlock textBox = grid.Children.OfType<TextBlock>().First();

            int idx = int.Parse(textBox.Text);

            string path = vieModel.DetailMovie.extraimagePath[idx];
            if (File.Exists(path))
            {
                try
                {
                    Process.Start("explorer.exe", "/select, \"" + path + "\"");
                }catch(Exception ex)
                {
                    HandyControl.Controls.Growl.Error(ex.Message);
                }

            } else
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_FileNotExist);

            

        }

        public void DeleteImage(object sender, RoutedEventArgs e)
        {
            MenuItem m1 = sender as MenuItem;

            ContextMenu contextMenu = m1.Parent as ContextMenu;

            Border border = contextMenu.PlacementTarget as Border;
            Grid grid = border.Parent as Grid;
            TextBlock textBox = grid.Children.OfType<TextBlock>().First();

            int idx = int.Parse(textBox.Text);

            string path = vieModel.DetailMovie.extraimagePath[idx];

            try
            {
                File.Delete(path);
                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
                vieModel.Query(vieModel.DetailMovie.id);
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl");
            }
        }

        public void OpenFilePath(object sender, RoutedEventArgs e)
        {
            try
            {
                DetailMovie detailMovie = vieModel.DetailMovie;

                    if (File.Exists(detailMovie.filepath)) { Process.Start("explorer.exe", "/select, \"" + detailMovie.filepath + "\""); }
                    else
                    {
                        HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_FileNotExist, "DetailsGrowl");
                    }
            }
            catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
        }


        private void SetToSmallPic(object sender, RoutedEventArgs e)
        {
            MenuItem m1 = sender as MenuItem;
            MenuItem m2 = m1.Parent as MenuItem;

            ContextMenu contextMenu = m2.Parent as ContextMenu;

            Border border = contextMenu.PlacementTarget as Border;
            Grid grid = border.Parent as Grid;
            TextBlock textBox = grid.Children.OfType<TextBlock>().First();

            int idx = int.Parse(textBox.Text);

            string path = vieModel.DetailMovie.extraimagePath[idx];

            try
            {
                File.Copy(path, BasePicPath + $"SmallPic\\{vieModel.DetailMovie.id}.jpg", true);
                //更新到 UI
                RefreshUI(path);
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl");
            }
        }


        private void SetToBigAndSmallPic(object sender, RoutedEventArgs e)
        {
            MenuItem m1 = sender as MenuItem;
            MenuItem m2 = m1.Parent as MenuItem;

            ContextMenu contextMenu = m2.Parent as ContextMenu;

            Border border = contextMenu.PlacementTarget as Border;
            Grid grid = border.Parent as Grid;
            TextBlock textBox = grid.Children.OfType<TextBlock>().First();

            int idx = int.Parse(textBox.Text);

            string path = vieModel.DetailMovie.extraimagePath[idx];

            try
            {
                File.Copy(path, BasePicPath + $"SmallPic\\{vieModel.DetailMovie.id}.jpg", true);
                File.Copy(path, BasePicPath + $"BigPic\\{vieModel.DetailMovie.id}.jpg", true);
                RefreshUI(path,path);
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl");
            }
        }



        private void SetToBigPic(object sender, RoutedEventArgs e)
        {
            MenuItem m1 = sender as MenuItem;
            MenuItem m2 = m1.Parent as MenuItem;

            ContextMenu contextMenu = m2.Parent as ContextMenu;

            Border border = contextMenu.PlacementTarget as Border;
            Grid grid = border.Parent as Grid;
            TextBlock textBox = grid.Children.OfType<TextBlock>().First();

            int idx = int.Parse(textBox.Text);

            string path = vieModel.DetailMovie.extraimagePath[idx];
            if (!File.Exists(path)) { return; }

            try
            {
                File.Copy(path, BasePicPath + $"BigPic\\{vieModel.DetailMovie.id}.jpg", true);
                ////BigImage.Source = new BitmapImage(new Uri(path));
                //DetailMovie detailMovie = vieModel.DetailMovie;
                //detailMovie.bigimage = null;

                //vieModel.DetailMovie = null;

                //detailMovie.bigimage = StaticClass.BitmapImageFromFile(path);
                //vieModel.DetailMovie = detailMovie;

                RefreshUI("", path);
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl");
            }
        }


        private void RefreshUI(string smallPicPath, string BigPicPath = "")
        {
            windowMain = App.Current.Windows[0] as Main;
            for (int i = 0; i < windowMain.vieModel.CurrentMovieList.Count; i++)
            {
                try
                {
                    if (windowMain.vieModel.CurrentMovieList[i]?.id == vieModel.DetailMovie.id)
                    {
                        Movie movie = windowMain.vieModel.CurrentMovieList[i];
                        BitmapSource smallimage = movie.smallimage;
                        BitmapSource bigimage = movie.bigimage;

                        if (smallPicPath != "") movie.bigimage = null;
                        if (BigPicPath != "") movie.smallimage = null;
                        windowMain.vieModel.CurrentMovieList[i] = null;
                        if (smallPicPath != "") movie.smallimage =BitmapImageFromFile(smallPicPath);
                        if (BigPicPath != "") movie.bigimage = BitmapImageFromFile(BigPicPath);

                        if (movie.bigimage == null && bigimage != null) movie.bigimage = bigimage;
                        if (movie.smallimage == null && smallimage != null) movie.smallimage = smallimage;

                        windowMain.vieModel.CurrentMovieList[i] = movie;
                    }
                }
                catch (Exception ex1)
                {
                    Console.WriteLine(ex1.StackTrace);
                    Console.WriteLine(ex1.Message);
                }
            }
        }


        public void RefreshFavorites()
        {
            windowMain = App.Current.Windows[0] as Main;
            for (int i = 0; i < windowMain.vieModel.CurrentMovieList.Count; i++)
            {
                try
                {
                    if (windowMain.vieModel.CurrentMovieList[i]?.id == vieModel.DetailMovie.id)
                    {
                        Movie movie = windowMain.vieModel.CurrentMovieList[i];
                        windowMain.vieModel.CurrentMovieList[i] = null;
                        movie.favorites = vieModel.DetailMovie.favorites;
                        windowMain.vieModel.CurrentMovieList[i] = movie;
                        windowMain.vieModel.Statistic();
                    }
                }
                catch (Exception ex1)
                {
                    Console.WriteLine(ex1.StackTrace);
                    Console.WriteLine(ex1.Message);
                }
            }
        }


        public void CopyFile(object sender, RoutedEventArgs e)
        {
            string filepath = vieModel.DetailMovie.filepath;
            if (File.Exists(filepath))
            {
                StringCollection paths = new StringCollection { filepath };
                try
                {
                    Clipboard.SetFileDropList(paths);
                    HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.HasCopy} {filepath}", "DetailsGrowl");
                }
                catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl"); }

            }
        }

        public void DeleteFile(object sender, RoutedEventArgs e)
        {
            DetailMovie detailMovie = vieModel.DetailMovie;
            if (detailMovie.subsectionlist.Count > 0)
            {
                if (new Msgbox(this, Jvedio.Language.Resources.IsToDeleteAllSubSection).ShowDialog() == true)
                {
                    detailMovie.subsectionlist.ForEach(path =>
                    {

                        if (File.Exists(path))
                        {
                            try
                            {
                                FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }
                            catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl"); }
                        }
                    });
                    if (Properties.Settings.Default.DelInfoAfterDelFile) DeleteID(sender, e);
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
                }
            }
            else
            {
                string filepath = detailMovie.filepath;
                if (File.Exists(filepath))
                {
                    if (new Msgbox(this, $"{Jvedio.Language.Resources.IsToDelete}  {filepath} ").ShowDialog() == true)
                    {
                        try
                        {
                            FileSystem.DeleteFile(filepath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
                            if (Properties.Settings.Default.DelInfoAfterDelFile) DeleteID(sender, e);
                        }
                        catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl"); }

                    }
                }
                else
                {
                    HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_FileNotExist, "DetailsGrowl");
                    DeleteID(sender, e);
                }
            }



        }


        public void DeleteInfo(object sender, RoutedEventArgs e)
        {
            DataBase.ClearInfoByID(vieModel.DetailMovie.id);
            windowMain = App.Current.Windows[0] as Main;
            windowMain.RefreshMovieByID(vieModel.DetailMovie.id);
            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
        }

        public void DeleteID(object sender, RoutedEventArgs e)
        {
            DataBase.DeleteByField("movie", "id", vieModel.DetailMovie.id);
            windowMain = App.Current.Windows[0] as Main;
            var movie = windowMain.vieModel.CurrentMovieList.Where(arg => arg.id == vieModel.DetailMovie.id).First();

            if (windowMain.vieModel.CurrentMovieList.Count > 1)
            {
                NextMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
            }

            //从主界面删除
            windowMain.vieModel.CurrentMovieList.Remove(movie);
            windowMain.vieModel.MovieList.Remove(movie);
            windowMain.vieModel.FilterMovieList.Remove(movie);
            windowMain.vieModel.Statistic();

            if (windowMain.vieModel.CurrentMovieList.Count == 0)
            {
                this.Close();
            }
        }

        private void OpenWeb(object sender, RoutedEventArgs e)
        {

            DetailMovie detailMovie = vieModel.DetailMovie;

            if (!string.IsNullOrEmpty(detailMovie.sourceurl) && detailMovie.sourceurl.IndexOf("http") >= 0)
            {
                try
                {
                    Process.Start(detailMovie.sourceurl);
                }
                catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl"); }

            }
            else
            {
                //为空则使用 bus 打开
                if (!string.IsNullOrEmpty(JvedioServers.Bus.Url) && JvedioServers.Bus.Url.IndexOf("http") >= 0)
                {
                    try
                    {
                        Process.Start(JvedioServers.Bus.Url + detailMovie.id);
                    }
                    catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl"); }
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_UrlNotSet, "DetailsGrowl");
                }

            }
        }


        private void UpdateInfo(DetailMovie movie)
        {
            //显示到主界面
            Main main = App.Current.Windows[0] as Main;

            int index1 = main.vieModel.CurrentMovieList.IndexOf(main.vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
            int index2 = main.vieModel.MovieList.IndexOf(main.vieModel.MovieList.Where(arg => arg.id == movie.id).First());
            int index3 = main.vieModel.FilterMovieList.IndexOf(main.vieModel.FilterMovieList.Where(arg => arg.id == movie.id).First());
            try
            {
                main.vieModel.CurrentMovieList[index1] = null;
                main.vieModel.MovieList[index2] = null;
                main.vieModel.CurrentMovieList[index1] = movie;
                main.vieModel.MovieList[index2] = movie;
                main.vieModel.FilterMovieList[index3] = null;
                main.vieModel.FilterMovieList[index3] = movie;
            }
            catch (ArgumentNullException) { }

            //显示到当前页面
            vieModel.DetailMovie = null;
            vieModel.DetailMovie = movie;
            
        }

        public async void TranslateMovie(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_TL_BAIDU & !Properties.Settings.Default.Enable_TL_YOUDAO) { HandyControl.Controls.Growl.Warning("请设置【有道翻译】并测试", "DetailsGrowl"); return; }
                string result = "";
                MySqlite dataBase = new MySqlite("Translate");

                DetailMovie movie = vieModel.DetailMovie;

                //检查是否已经翻译过，如有则跳过
                if (!string.IsNullOrEmpty(dataBase.SelectByField("translate_title", "youdao", movie.id))) { HandyControl.Controls.Growl.Warning("影片已经翻译过！", "DetailsGrowl"); return; }
                if (movie.title != "")
                {

                    if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.title);
                    //保存
                    if (result != "")
                    {

                        dataBase.SaveYoudaoTranslateByID(movie.id, movie.title, result, "title");
                        movie.title = result;
                        UpdateInfo(movie);
                        HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.TranslateSuccess, "DetailsGrowl");
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.TranslateFail, "DetailsGrowl");
                    }

                }

                if (movie.plot != "")
                {
                    if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.plot);
                    //保存
                    if (result != "")
                    {
                        dataBase.SaveYoudaoTranslateByID(movie.id, movie.plot, result, "plot");
                        movie.plot = result;
                        UpdateInfo(movie);
                        HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.TranslateSuccess, "DetailsGrowl");
                    }

                }
                dataBase.CloseDB();
            
        }



        public async void GenerateSmallImage(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_BaiduAI) { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetBaiduAI, "DetailsGrowl"); return; }
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            if (mnu != null)
            {
                this.Cursor = Cursors.Wait;

                DetailMovie movie = vieModel.DetailMovie;
                    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";
                    string SmallPicPath = Properties.Settings.Default.BasePicPath + $"SmallPic\\{movie.id}.jpg";
                    if (File.Exists(BigPicPath))
                    {
                        Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);

                        if (int32Rect != Int32Rect.Empty)
                        {
                            await Task.Delay(500);
                            //切割缩略图
                            System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
                            BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
                            ImageSource smallImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetRect(bitmapImage, int32Rect));
                            System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(smallImage);
                            try
                            {
                                bitmap.Save(SmallPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); 
                            }
                            catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl"); }

                            movie.smallimage = GetBitmapImage(movie.id, "SmallPic");
                            UpdateInfo(movie);
                        HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_Fail, "DetailsGrowl");
                    }

                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_PosterMustExist, "DetailsGrowl");
                    }

                


            }
            this.Cursor = Cursors.Arrow;
        }


        public async void GenerateActor(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_BaiduAI) { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetBaiduAI, "DetailsGrowl"); return; }
            MenuItem _mnu = sender as MenuItem;
            if (_mnu.Parent is MenuItem mnu)
            {

                DetailMovie movie = vieModel.DetailMovie;



                if (movie.actor == "") { HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.MovieHasNoActor, "DetailsGrowl"); return; }
                string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";
                string name = movie.actor.Split(actorSplitDict[movie.vediotype])[0];
                this.Cursor = Cursors.Wait;
                string ActressesPicPath = Properties.Settings.Default.BasePicPath + $"Actresses\\{name}.jpg";
                if (File.Exists(BigPicPath))
                {
                    Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);
                    if (int32Rect != Int32Rect.Empty)
                    {
                        await Task.Delay(500);
                        //切割演员头像
                        System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
                        BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
                        ImageSource actressImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetActressRect(bitmapImage, int32Rect));
                        System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(actressImage);
                        try { bitmap.Save(ActressesPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); }
                        catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl"); }
                        HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_Fail, "DetailsGrowl");
                    }
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_PosterMustExist, "DetailsGrowl");
                }

            }
            this.Cursor = Cursors.Arrow;
        }
        public void OpenSubSuctionVedio(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            TextBlock textBlock = stackPanel.Children.OfType<TextBlock>().Last();
            string filepath = textBlock.Text;
            if (File.Exists(filepath)) { Process.Start(filepath); } else { HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.CannotOpen} {filepath}", "DetailsGrowl"); }


        }

        public void ShowSubsection(object sender, MouseButtonEventArgs e)
        {
            SubSectionPopup.IsOpen = true;
            Console.WriteLine(SubSectionPopup.IsOpen);
        }



        private void Grid_PreviewKeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Escape)
                this.Close();
            else if (e.Key == Key.Left)
                PreviewMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (e.Key == Key.Right)
                NextMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.P)
                PlayVedio(sender, new MouseEventArgs(InputManager.Current.PrimaryMouseDevice, 0));
            else if (e.Key == Key.E)
                EditInfo(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (e.Key == Key.D )
                DownLoad(sender, new RoutedEventArgs());

        }





        private void SetImage(int idx)
        {
            if (vieModel.DetailMovie.extraimagelist.Count == 0)
            {
                //设置为默认图片
                BigImage.Source = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
            }
            else
            {
                BigImage.Source = vieModel.DetailMovie.extraimagelist[idx];

                //设置遮罩
                for (int i = 0; i < imageItemsControl.Items.Count; i++)
                {
                    ContentPresenter c = (ContentPresenter)imageItemsControl.ItemContainerGenerator.ContainerFromItem(imageItemsControl.Items[i]);
                    StackPanel stackPanel = FindElementByName<StackPanel>(c, "ImageStackPanel");
                    if (stackPanel != null)
                    {
                        Grid grid = stackPanel.Children[0] as Grid;
                        Border border = grid.Children[0] as Border;
                        TextBlock tb = grid.Children.OfType<TextBlock>().First();

                        if (border != null & tb != null)
                        {
                            if (int.Parse(tb.Text) == idx)
                                border.Opacity = 0;
                            else
                                border.Opacity = 0.5;
                        }
                    }
                }
            }
        }



        private void ShowExtraImage(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            TextBlock tb = ((Grid)border.Parent).Children.OfType<TextBlock>().First();
            vieModel.SelectImageIndex = int.Parse(tb.Text);
            SetImage(vieModel.SelectImageIndex);
        }

        public T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
        {
            T childElement = null;
            if (element == null) return childElement;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < nChildCount; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

                if (child == null)
                    continue;

                if (child is T && child.Name.Equals(sChildName))
                {
                    childElement = (T)child;
                    break;
                }

                childElement = FindElementByName<T>(child, sChildName);

                if (childElement != null)
                    break;
            }

            return childElement;
        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopDownLoad();
            DisposeImage();
        }


        private void BigImage_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void BigImage_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = dragdropFiles[0];

            if (IsFile(file))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension.ToLower() == ".jpg")
                {
                    //try
                    //{
                        File.Copy(fileInfo.FullName, BasePicPath + $"BigPic\\{vieModel.DetailMovie.id}.jpg", true);
                        DetailMovie detailMovie = vieModel.DetailMovie;
                        detailMovie.bigimage = null;
                        detailMovie.bigimage =BitmapImageFromFile(fileInfo.FullName);

                        if (vieModel.DetailMovie.extraimagelist.Count > 0)
                        {
                        detailMovie.extraimagelist[0] = detailMovie.bigimage;
                        detailMovie.extraimagePath[0] = fileInfo.FullName;

                        }
                        else
                        {
                        detailMovie.extraimagelist.Add(detailMovie.bigimage);
                        detailMovie.extraimagePath.Add(fileInfo.FullName);
                    }


                        vieModel.DetailMovie = null;
                        vieModel.DetailMovie = detailMovie;
                        RefreshUI("", fileInfo.FullName);
                        SetImage(0);


                    //}
                    //catch (Exception ex)
                    //{
                    //    HandyControl.Controls.Growl.Error(ex.Message, "DetailsGrowl");
                    //}
                }
                else
                {
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_OnlySupportJPG, "DetailsGrowl");
                }
            }

        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            //分为文件夹和文件
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> files = new List<string>();
            StringCollection stringCollection = new StringCollection();
            foreach (var item in dragdropFiles)
            {
                if (IsFile(item))
                    files.Add(item);
                else
                    stringCollection.Add(item);
            }
            List<string> filepaths = new List<string>();
            //扫描导入
            foreach (var item in stringCollection)
            {
                try { filepaths.AddRange(Directory.GetFiles(item, "*.jpg").ToList<string>()); }
                catch (Exception ex) { Console.WriteLine(ex.Message); continue; }
            }
            if (files.Count > 0) filepaths.AddRange(files);

            //复制文件
            string path;
            if ((bool)ExtraImageRadioButton.IsChecked)
                path = BasePicPath + $"ExtraPic\\{vieModel.DetailMovie.id}\\";
            else
                path = BasePicPath + $"ScreenShot\\{vieModel.DetailMovie.id}\\";

            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            bool success = false;
            foreach (var item in filepaths)
            {
                try
                {
                    File.Copy(item, path + item.Split('\\').Last());
                    success = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }

            }
            if (success)
            {
                //更新UI
                if ((bool)ExtraImageRadioButton.IsChecked)
                    ExtraImageRadioButton_Click(sender, new RoutedEventArgs());
                else
                    ScreenShotRadioButton_Click(sender, new RoutedEventArgs());

                HandyControl.Controls.Growl.Success($"{Jvedio.Language.Resources.ImportNumber} {filepaths.Count}", "DetailsGrowl");

            }

        }

        private void Rate_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
                if (vieModel.DetailMovie != null)
                {
                    vieModel.SaveLove();
                    //更新主界面
                    RefreshFavorites();
                }
            


        }

        private void Tag_Closing(object sender, EventArgs e)
        {
            HandyControl.Controls.Tag Tag = sender as HandyControl.Controls.Tag;
            string text = Tag.Content.ToString();
            //删除
            if (vieModel.DetailMovie.labellist.Contains(text))
            {
                vieModel.DetailMovie.labellist.Remove(text);
                vieModel.SaveLabel();
            }

            if (text == "+")
            {
                //显示新增按钮
                List<string> labels = vieModel.DetailMovie.labellist;
                vieModel.DetailMovie.labellist = new List<string>();
                vieModel.DetailMovie.labellist.Add("+");
                vieModel.DetailMovie.labellist.AddRange(labels);
                vieModel.Query(vieModel.DetailMovie.id);
            }


        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            border.Opacity = 0;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            Grid grid = border.Parent as Grid;
            TextBlock textBlock = grid.Children.OfType<TextBlock>().First();

            int idx = int.Parse(textBlock.Text);
            if (idx != vieModel.SelectImageIndex)
                border.Opacity = 0.5;
            else
                border.Opacity = 0;




        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (vieModel.DetailMovie.extraimagelist.Count == 0) return;
            if (e.Delta > 0)
            {
                vieModel.SelectImageIndex -= 1;
            }
            else
            {
                vieModel.SelectImageIndex += 1;
            }

            if (vieModel.SelectImageIndex < 0) { vieModel.SelectImageIndex = 0; } else if (vieModel.SelectImageIndex >= imageItemsControl.Items.Count) { vieModel.SelectImageIndex = imageItemsControl.Items.Count - 1; }
            SetImage(vieModel.SelectImageIndex);
            //滚动到指定的
            ContentPresenter c = (ContentPresenter)imageItemsControl.ItemContainerGenerator.ContainerFromItem(imageItemsControl.Items[vieModel.SelectImageIndex]);
            c.BringIntoView();


        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //Main main = App.Current.Windows[0] as Main;
                //main.Resizing = true;
                //main.ResizingTimer.Start();
                //this.Close();
                Window_ImageViewer window_ImageViewer = new Window_ImageViewer(BigImage.Source);
                window_ImageViewer.ShowDialog();
            }
        }

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                NextMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            }
            else
            {
                PreviewMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            }
        }

        private MenuItem GetMenuItem(ContextMenu contextMenu, string header)
        {
            foreach (MenuItem item in contextMenu.Items)
            {
                if (item.Header.ToString() == header)
                {
                    return item;
                }
            }
            return null;
        }

        private void ContextMenu_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ContextMenu contextMenu = sender as ContextMenu;
            if (e.Key == Key.D)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_DeleteInfo);
                if (menuItem != null) DeleteID(menuItem, new RoutedEventArgs());
            }
            else if (e.Key == Key.T)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_DeleteFile);
                if (menuItem != null) DeleteFile(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.S)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_SyncInfo);
                if (menuItem != null) DownLoad(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.E)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_EditInfo);
                if (menuItem != null) EditInfo(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.W)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_OpenWebSite);
                if (menuItem != null) OpenWeb(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.C)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_CopyFile);
                if (menuItem != null) CopyFile(menuItem, new RoutedEventArgs());

            }
            contextMenu.IsOpen = false;
        }

        public void SetStatus(bool status)
        {
            NextGrid.IsEnabled = status;
            PreviousGrid.IsEnabled = status;
            ImageChangeStackPanel.IsEnabled = status;
        }

        private  void Window_ContentRendered(object sender, EventArgs e)
        {
            SetSkin();
            if (MovieID != "")
            {
                vieModel = new VieModel_Details();

                vieModel.QueryCompleted += async delegate
                {
                    ShowTagStamps();
                    if ((bool)ExtraImageRadioButton.IsChecked) await LoadImage();
                    else await LoadScreenShotImage();
                    SetStatus(true);
                    await Task.Run(() => {
                        vieModel.VedioInfo = MediaParse.GetMediaInfo(vieModel.DetailMovie.filepath);
                    });
                };
                vieModel.Query(MovieID);
                this.DataContext = vieModel;

            }
            else { this.DataContext = null; }
            FatherGrid.Focus();

            InitList();
        }

        private void InitList()
        {
            MySqlite dB = new MySqlite("mylist");
            List<string> tables = dB.GetAllTable();
            vieModel.MyList = new ObservableCollection<MyListItem>();
            foreach (string table in tables)
            {
                vieModel.MyList.Add(new MyListItem(table, (long)dB.SelectCountByTable(table)));
            }
            dB.Close();


        }


        private void DisposeImage()
        {
            if (vieModel.DetailMovie.extraimagelist != null)
            {
                for (int i = 0; i < vieModel.DetailMovie.extraimagelist.Count; i++)
                {
                    vieModel.DetailMovie.extraimagelist[i] = null;
                }
            }
        }

        private async Task<bool> LoadImage()
        {
            //加载大图到预览图
            DisposeImage();
            vieModel.DetailMovie.extraimagelist = new ObservableCollection<BitmapSource>();
            vieModel.DetailMovie.extraimagePath = new ObservableCollection<string>();
            if (File.Exists(BasePicPath + $"BigPic\\{vieModel.DetailMovie.id}.jpg"))
            {
                vieModel.DetailMovie.extraimagelist.Add(vieModel.DetailMovie.bigimage);
                vieModel.DetailMovie.extraimagePath.Add(BasePicPath + $"BigPic\\{vieModel.DetailMovie.id}.jpg");
            }
            await Task.Run( async() => {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background ,(Action)delegate {
                    imageItemsControl.ItemsSource = vieModel.DetailMovie.extraimagelist;
                    SetImage(0);
                });
            });



            //扫描预览图目录
            List<string> imagePathList = new List<string>();
            await Task.Run(() => { 
                if (Directory.Exists(GlobalVariable.BasePicPath + $"ExtraPic\\{vieModel.DetailMovie.id}\\"))
                {
                    try
                    {
                        foreach (var path in Directory.GetFiles(GlobalVariable.BasePicPath + $"ExtraPic\\{vieModel.DetailMovie.id}\\")) imagePathList.Add(path);
                    }
                    catch { }
                    if (imagePathList.Count > 0) imagePathList = imagePathList.CustomSort().ToList();
                }
            });

            //加载预览图
            await Task.Run(async() => {
                foreach (var path in imagePathList)
                {
                    if (Path.GetDirectoryName(path).Split('\\').Last().ToUpper() != vieModel.DetailMovie.id.ToUpper()) break;
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadExtraImageDelegate(LoadExtraImage), GetExtraImage(path));
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadExtraPathDelegate(LoadExtraPath), path);

                     await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate { 
                         imageItemsControl.ItemsSource = vieModel.DetailMovie.extraimagelist; 
                     });
                }
            });
            SetImage(0);

            return true;
        }


        private async Task<bool> LoadScreenShotImage()
        {
            vieModel.DetailMovie.extraimagelist = new ObservableCollection<BitmapSource>();
            vieModel.DetailMovie.extraimagePath = new ObservableCollection<string>();
            await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate { 
                imageItemsControl.ItemsSource = vieModel.DetailMovie.extraimagelist; 
            });
            //扫描截图目录
            List<string> imagePathList = new List<string>();
            await Task.Run(() => {
                if (Directory.Exists(GlobalVariable.BasePicPath + $"ScreenShot\\{vieModel.DetailMovie.id}\\"))
                {
                    try
                    {
                        foreach (var path in Directory.GetFiles(GlobalVariable.BasePicPath + $"ScreenShot\\{vieModel.DetailMovie.id}\\")) imagePathList.Add(path);
                    }
                    catch { }
                    if (imagePathList.Count > 0) imagePathList = imagePathList.CustomSort().ToList();
                }
            });

            //加载影片截图
            await Task.Run(async () => { 
                foreach (var path in imagePathList)
                {
                    if (Path.GetDirectoryName(path).Split('\\').Last().ToUpper() != vieModel.DetailMovie.id) break;
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadExtraImageDelegate(LoadExtraImage), GetExtraImage(path));
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadExtraPathDelegate(LoadExtraPath), path);
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate { 
                        imageItemsControl.ItemsSource = vieModel.DetailMovie.extraimagelist; 
                    });
                }
            });
            SetImage(0);

            return true;
        }


        private delegate void LoadExtraImageDelegate(BitmapSource bitmapSource);
        private void LoadExtraImage(BitmapSource bitmapSource)
        {
            vieModel.DetailMovie.extraimagelist.Add(bitmapSource);
        }

        private delegate void LoadExtraPathDelegate(string path);
        private void LoadExtraPath(string path)
        {
            vieModel.DetailMovie.extraimagePath.Add(path);
        }

        private async void ExtraImageRadioButton_Click(object sender, RoutedEventArgs e)
        {
            //切换为预览图
            await LoadImage();
            scrollViewer.ScrollToHorizontalOffset(0);
        }

        private async void ScreenShotRadioButton_Click(object sender, RoutedEventArgs e)
        {
            //切换为截图
            await LoadScreenShotImage();
            scrollViewer.ScrollToHorizontalOffset(0);
        }

        private void BigImage_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Grid grid = (Grid)sender;
            ContextMenu contextMenu = grid.ContextMenu;
            string table = ((Main)GetWindowByName("Main")).GetCurrentList();
            if (!string.IsNullOrEmpty(table))
            {
                foreach (MenuItem item in contextMenu.Items)
                {
                    string header = item.Header.ToString();
                    if (header==Jvedio.Language.Resources.OpenPath || header== Jvedio.Language.Resources.Menu_OpenWebSite || header== Jvedio.Language.Resources.Menu_CopyFile)
                        item.Visibility = Visibility.Visible;
                    else
                        item.Visibility = Visibility.Collapsed;
                }
        }
            else
            {
                foreach (MenuItem item in contextMenu.Items)
                {
                        item.Visibility = Visibility.Visible;
                }
}


if (contextMenu.Visibility != Visibility.Visible) return;
            Task.Run(() => {
                Task.Delay(100).Wait();
                this.Dispatcher.Invoke(() => {
                    MenuItem menuItem = FindElementByName<MenuItem>(contextMenu, "ListMenuItem");
                    if (menuItem != null)
                    {
                        menuItem.Items.Clear();
                        foreach (var item in vieModel.MyList)
                        {
                            MenuItem menuItem1 = new MenuItem();
                            menuItem1.Header = item.Name;
                            menuItem1.Click += MyListItemClick;
                            menuItem.Items.Add(menuItem1);
                        }
                    }
                });
            });

        }

        private void MyListItemClick(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            string table = menuItem.Header.ToString();
            Movie newMovie = DataBase.SelectMovieByID(vieModel.DetailMovie.id);
            MySqlite dB = new MySqlite("mylist");
            dB.InsertFullMovie(newMovie, table);
            dB.CloseDB();
            Main main = App.Current.Windows[0] as Main;
            main.InitList();
            InitList();
        }

        private void AddNewLabel(object sender, RoutedEventArgs e)
        {
            vieModel.GetLabelList();
            LabelGrid.Visibility = Visibility.Visible;
            for (int i = 0; i < vieModel.LabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    toggleButton.IsChecked = false;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //获得选中的标签
            List<string> originLabels = new List<string>();
            for (int i = 0; i < vieModel.LabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    if ((bool)toggleButton.IsChecked)
                    {
                        Match match = Regex.Match(toggleButton.Content.ToString(), @"\( \d+ \)");
                        if (match != null && match.Value != "")
                        {
                            string label = toggleButton.Content.ToString().Replace(match.Value, "");
                            if (!originLabels.Contains(label)) originLabels.Add(label);
                        }

                    }
                }
            }

            if (originLabels.Count <= 0)
            {
                return;
            }


            List<string> labels = LabelToList(vieModel.DetailMovie.label);
            labels = labels.Union(originLabels).ToList();
            vieModel.DetailMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(vieModel.DetailMovie.id, "label", vieModel.DetailMovie.label, "String");
            
            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "DetailsGrowl");
            LabelGrid.Visibility = Visibility.Hidden;

            vieModel.Query(vieModel.DetailMovie.id);
            //显示到主界面
            Main main = App.Current.Windows[0] as Main;
            main.RefreshMovieByID(vieModel.DetailMovie.id);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel stackPanel = (StackPanel)button.Parent;
            Grid grid = (Grid)stackPanel.Parent;
            ((Grid)grid.Parent).Visibility = Visibility.Hidden;
        }

        private void HideGrid(object sender, MouseButtonEventArgs e)
        {
            Grid grid = ((Border)sender).Parent as Grid;
            grid.Visibility = Visibility.Hidden;

        }


        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance !=null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal,this);
                taskbarInstance.SetProgressValue((int)e.NewValue, 100,this);
                if (e.NewValue == 100) taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress,this);
            }
        }

        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ProgressBar.Visibility == Visibility.Collapsed && Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress,this);
            }
        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            textBlock.TextDecorations= System.Windows.TextDecorations.Underline;
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            textBlock.TextDecorations = null;
        }

        private void TextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFilePath(sender, new RoutedEventArgs());
        }

    }







}
