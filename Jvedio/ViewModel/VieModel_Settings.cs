﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;

namespace Jvedio.ViewModel
{
    public class VieModel_Settings : ViewModelBase
    {

        public VieModel_Settings()
        {
            DataBase = Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath);
            DataBases = ((Main)App.Current.Windows[0]).vieModel.DataBases;




        }


        public void Reset()
        {
            //读取配置文件
            ScanPath = new ObservableCollection<string>();
            foreach(var item in ReadScanPathFromConfig(DataBase))
            {
                ScanPath.Add(item);
            }
            if (ScanPath.Count == 0) ScanPath = null;

            Servers = new ObservableCollection<Server>();
            if (Properties.Settings.Default.Bus != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.Bus);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableBus, Url = Properties.Settings.Default.Bus, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.BusEurope != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.BusEu);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableBusEu, Url = Properties.Settings.Default.BusEurope, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.DB != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.DB);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableDB, Url = Properties.Settings.Default.DB, Cookie = Properties.Settings.Default.DBCookie, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.FC2 != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.FC2);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableFC2, Url = Properties.Settings.Default.FC2, Cookie = Properties.Settings.Default.FC2Cookie, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.Library != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.Library);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableLibrary, Url = Properties.Settings.Default.Library, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }

            if (Properties.Settings.Default.DMM != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.DMM);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableDMM, Url = Properties.Settings.Default.DMM, Cookie=Properties.Settings.Default.DMMCookie,Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.Jav321 != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.Jav321);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.Enable321, Url = Properties.Settings.Default.Jav321, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.MOO != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.MOO);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableMOO, Url = Properties.Settings.Default.MOO, Cookie=Properties.Settings.Default.MOOCookie,Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }

        }


        private string _ViewRenameFormat;

        public string ViewRenameFormat
        {
            get { return _ViewRenameFormat; }
            set
            {
                _ViewRenameFormat = value;
                RaisePropertyChanged();
            }
        }





        private ObservableCollection<Server> _Servers;

        public ObservableCollection<Server> Servers
        {
            get { return _Servers; }
            set
            {
                _Servers = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _ScanPath ;

        public ObservableCollection<string> ScanPath
        {
            get { return _ScanPath; }
            set
            {
                _ScanPath = value;
                RaisePropertyChanged();
            }
        }


        private Skin _Themes = (Skin)Enum.Parse(typeof(Skin), Properties.Settings.Default.Themes, true);

        public Skin Themes
        {
            get { return _Themes; }
            set
            {
                _Themes = value;
                RaisePropertyChanged();
            }
        }


        private MyLanguage _Language = (MyLanguage)Enum.Parse(typeof(MyLanguage), Properties.Settings.Default.Language, true);

        public MyLanguage Language
        {
            get { return _Language; }
            set
            {
                _Language = value;
                RaisePropertyChanged();
            }
        }

        private string _DataBase  ;

        public string DataBase
        {
            get { return _DataBase; }
            set
            {
                _DataBase = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _DataBases;

        public ObservableCollection<string> DataBases
        {
            get { return _DataBases; }
            set
            {
                _DataBases = value;
                RaisePropertyChanged();
            }
        }


        



    }
}
