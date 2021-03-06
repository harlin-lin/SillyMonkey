﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DataInterface;
using DevExpress.Mvvm;
using FileHelper;
using FileReader;
using Microsoft.Win32;
using SillyMonkeyD.Views;

namespace SillyMonkeyD.ViewModels {

    public class FileManagementViewModel : ViewModelBase {

        public FileManagementViewModel() {
            Files = new ObservableCollection<IDataAcquire>();
            Sites = null;
            SelectedFile = null;
            //SelectedSite = new KeyValuePair<byte, int>();

            TabList = new ObservableCollection<ITab>();

            InitUiCtr();
        }

        private FilterEditor _filterWindow;
        private Data _dataWindow;
        private Misc _miscWindow;

        public ObservableCollection<IDataAcquire> Files { get; private set; }
        public ObservableCollection<ITab> TabList { get; private set; }
        public Dictionary<byte, int> Sites { get { return GetProperty(() => Sites); } private set { SetProperty(() => Sites, value); } }
        public IDataAcquire SelectedFile { get { return GetProperty(() => SelectedFile); } set { SetProperty(() => SelectedFile, value); } }
        public KeyValuePair<byte, int> SelectedSite { get { return GetProperty(() => SelectedSite); } set { SetProperty(() => SelectedSite, value); } }
        public ITab SelectedTab { get { return GetProperty(() => SelectedTab); } set { SetProperty(() => SelectedTab, value); } }
        public string FileInfo { get { return GetProperty(() => FileInfo); } private set { SetProperty(() => FileInfo, value); } }



        public async void AddFile(string path) {
            IDataAcquire data = new StdReader(path, StdFileType.STD);
            data.ExtractDone += Data_ExtractDone;
            //data.FilterGenerated += Data_FilterGenerated;
            Files.Add(data);
            //extract the files
            await System.Threading.Tasks.Task.Run(new Action(() => data.ExtractStdf()));
        }

        //private void Data_FilterGenerated(IDataAcquire data) {
            

        //}

        private void Data_ExtractDone(IDataAcquire data) {
            if (data == SelectedFile) {
                Sites = SelectedFile.GetSitesChipCount();
                FileInfo = StdFileHelper.GetBriefSummary(SelectedFile);
            }
        }

        private void RemoveFile(IDataAcquire data) {
            data.ExtractDone -= Data_ExtractDone;
            //data.ExtractDone -= Data_FilterGenerated;

            for(int i = (TabList.Count - 1); i>= 0; i--) {
                if (TabList[i].DataAcquire == data) {
                    if (TabList[i].WindowFlag==1) {
                        ((DataViewModel)_dataWindow.DataContext).RemoveTab(TabList[i].CorrespondingTab);
                        TabList.RemoveAt(i);
                    }else if (TabList[i].WindowFlag == 2) {
                        ((MiscViewModel)_miscWindow.DataContext).RemoveTab(TabList[i].CorrespondingTab);
                        TabList.RemoveAt(i);
                    }
                }
            }
            
            Files.Remove(data);
            GC.Collect();
        }



        private void AddTab(ITab tabItem) {
            bool foundSameFile = false;
            for (int i = TabList.Count - 1; i >= 0; i--) {
                if (TabList[i].DataAcquire == tabItem.DataAcquire && TabList[i].FilterId == tabItem.FilterId) {
                    TabList.Insert(i+1, tabItem);
                    foundSameFile = true;
                    break;
                }
            }
            if(!foundSameFile) TabList.Add(tabItem);
            RaisePropertiesChanged("TabList");
        }

        private void AddDataTab(IDataAcquire dataAcquire, int filterId) {
            RawGridTab rawGridTab = new RawGridTab(dataAcquire, filterId);
            ((DataViewModel)_dataWindow.DataContext).AddTab(rawGridTab);
            AddTab((ITab)rawGridTab.DataContext); 
        }
        private void AddDataTab(TabItem tabItem) {
            ((DataViewModel)_dataWindow.DataContext).AddTab(tabItem);
            AddTab((ITab)tabItem.DataContext);
        }

        private void RemoveDataTab(ITab tabItem) {
            //need first remove its sub tab, then remove itself
            if (tabItem.IsMainTab) {
                for(int i=TabList.Count-1; i >= 0; i--) {
                    if (!TabList[i].IsMainTab && TabList[i].DataAcquire == tabItem.DataAcquire && TabList[i].FilterId == tabItem.FilterId) {
                        if(TabList[i].WindowFlag==1)
                            ((DataViewModel)_dataWindow.DataContext).RemoveTab(TabList[i].CorrespondingTab);
                        else
                            ((MiscViewModel)_miscWindow.DataContext).RemoveTab(TabList[i].CorrespondingTab);
                        TabList.RemoveAt(i);
                    }
                }
                
            }
            ((DataViewModel)_dataWindow.DataContext).RemoveTab(tabItem.CorrespondingTab);
            TabList.Remove(tabItem);
        }

        private void AddMiscTab(TabItem tabItem) {
            ((MiscViewModel)_miscWindow.DataContext).AddTab(tabItem);
            AddTab((ITab)tabItem.DataContext);
        }

        private void RemoveMiscTab(ITab tabItem) {
            //need first remove its sub tab, then remove itself
            if (tabItem.IsMainTab) {
                for (int i = TabList.Count - 1; i >= 0; i--) {
                    if (!TabList[i].IsMainTab && TabList[i].DataAcquire == tabItem.DataAcquire && TabList[i].FilterId == tabItem.FilterId) {
                        if (TabList[i].WindowFlag == 1)
                            ((DataViewModel)_dataWindow.DataContext).RemoveTab(TabList[i].CorrespondingTab);
                        else
                            ((MiscViewModel)_miscWindow.DataContext).RemoveTab(TabList[i].CorrespondingTab);
                        TabList.RemoveAt(i);
                    }
                }

            }
            ((MiscViewModel)_miscWindow.DataContext).RemoveTab(tabItem.CorrespondingTab);
            TabList.Remove(tabItem);
        }

        private void LoadWindow() {
            if (_dataWindow is null) {
                //await System.Threading.Tasks.Task.Run(new Action(() => _dataWindow = new Data()));
                _dataWindow = new Data();
                ((DataViewModel)_dataWindow.DataContext).SelectedTabEvent += ((e) => SelectedTab = e);
                //_dataWindow.Show();
            }
            if (_filterWindow is null) {
                //await System.Threading.Tasks.Task.Run(new Action(() => new FilterEditor()));
                _filterWindow = new FilterEditor();
                ((FilterEditorViewModel)_filterWindow.DataContext).FilterUpdatedEvent += ((x, y)=> {
                    UpdateFilter(x, y);
                });
                //_filterWindow.Visibility = Visibility.Hidden;
                //_filterWindow.Show();
            }
            if(_miscWindow is null) {
                _miscWindow = new Misc();
                ((MiscViewModel)_miscWindow.DataContext).SelectedTabEvent += ((e) => SelectedTab = e);
            }
        }

        private void ShowFilterWindow(IDataAcquire dataAcquire, int filterId) {
            ((FilterEditorViewModel)_filterWindow.DataContext).UpdateFilter(dataAcquire, filterId);
            _filterWindow.Show();
        }

        private void ShowDataWindow(TabItem tabItem) {
            _dataWindow.Show();
            ((DataViewModel)_dataWindow.DataContext).FocusTab(tabItem);
        }
        private void ShowDataWindow() {
            _dataWindow.Show();
        }

        private void ShowMiscWindow(TabItem tabItem) {
            _miscWindow.Show();
            ((MiscViewModel)_miscWindow.DataContext).FocusTab(tabItem);
        }
        private void ShowMiscWindow() {
            _miscWindow.Show();
        }


        private void UpdateFilter(IDataAcquire dataAcquire, int filterId) {
            foreach(var t in TabList) {
                if(t.DataAcquire == dataAcquire && t.FilterId == filterId) {
                    t.UpdateFilter();
                }
            }
        }

        private TabItem FindOpendTab(IDataAcquire dataAcquire, int filterId, TabType tabType) {
            if (dataAcquire is null) return null;
            foreach (var t in TabList) {
                if (t.DataAcquire == dataAcquire && t.FilterId == filterId) {
                    if (t.TabType == tabType)
                        return t.CorrespondingTab;
                }
            }
            return null;
        }

        #region UI
        public ICommand<DragEventArgs> DropCommand { get; private set; }
        public ICommand<IDataAcquire> FileCloseCommand { get; private set; }
        public ICommand<ITab> TabCloseCommand { get; private set; }
        public ICommand FocusTab { get; private set; }
        public ICommand OpenFileRawData { get; private set; }
        public ICommand OpenSiteRawData { get; private set; }
        public ICommand UpdateInfo { get; private set; }
        public ICommand OpenFileDiag { get; private set; }
        public ICommand LoadedCommand { get; private set; }
        public ICommand<ITab> SetTabFilter { get; private set; }
        public ICommand<ITab> ShowTabSummary { get; private set; }
        public ICommand<ITab> ShowWaferMap { get; private set; }
        public ICommand<ITab> ShowChart { get; private set; }
        public ICommand<IList<object>> ShowCorrelation { get; private set; }

        private void InitUiCtr() {
            DropCommand = new DelegateCommand<DragEventArgs>((e) => {
                var paths = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
                foreach (string path in paths) {
                    var ext = System.IO.Path.GetExtension(path).ToLower();
                    if (ext == ".stdf" || ext == ".std") {
                        AddFile(path);
                    } else {
                        //log message not supported
                    }
                }
            });

            OpenFileDiag = new DelegateCommand(()=> {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "选择数据源文件";
                openFileDialog.Filter = "All|*.*|STDF|*.stdf|STD|*.std";
                openFileDialog.FileName = string.Empty;
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = true;
                openFileDialog.RestoreDirectory = false;
                openFileDialog.DefaultExt = "stdf";
                if (openFileDialog.ShowDialog() == false) {
                    return;
                }
                var paths = openFileDialog.FileNames;
                foreach (string path in paths) {
                    var ext = System.IO.Path.GetExtension(path).ToLower();
                    if (ext == ".stdf" || ext == ".std") {
                        AddFile(path);
                    } else {
                        //log message not supported
                    }
                }

            });

            FileCloseCommand = new DelegateCommand<IDataAcquire>((e) => {
                RemoveFile(e);
            });
            TabCloseCommand = new DelegateCommand<ITab>((e) => {
                if(e.WindowFlag == 1)
                    RemoveDataTab(e);
                else if(e.WindowFlag == 2)
                    RemoveMiscTab(e);
            });

            FocusTab = new DelegateCommand(() => {
                if (SelectedTab is null) return;
                if (SelectedTab.WindowFlag == 1)
                    ShowDataWindow(SelectedTab.CorrespondingTab);
                else
                    ShowMiscWindow(SelectedTab.CorrespondingTab);
            });

            OpenFileRawData = new DelegateCommand(() => {
                if (SelectedFile.ParseDone) {
                    AddDataTab(SelectedFile, SelectedFile.CreateFilter());
                    ShowDataWindow();
                }
            });
            OpenSiteRawData = new DelegateCommand(() => {
                if (SelectedFile.ParseDone) {
                    FilterSetup f = new FilterSetup("Site:" + SelectedSite.Key);
                    f.EnableSingleSite(SelectedFile.GetSites(), SelectedSite.Key);

                    AddDataTab(SelectedFile, SelectedFile.CreateFilter(f));
                    ShowDataWindow();
                }
            });

            UpdateInfo = new DelegateCommand(() => {
                if (SelectedFile is null) {
                    Sites = null;
                    FileInfo = "";
                }
                else if (SelectedFile.ParseDone) {
                    Sites = SelectedFile.GetSitesChipCount();
                    FileInfo = StdFileHelper.GetBriefSummary(SelectedFile);
                } else {
                    Sites = null;
                    FileInfo = "";
                }
            });
            LoadedCommand = new DelegateCommand(() => {
                LoadWindow();
            });

            SetTabFilter = new DelegateCommand<ITab>((e) => {
                ShowFilterWindow(e.DataAcquire, e.FilterId);
            });
            ShowTabSummary = new DelegateCommand<ITab>((e) => {
                var v = FindOpendTab(e.DataAcquire, e.FilterId, TabType.SummaryTab);
                if (v is null) {
                    AddMiscTab(new SummaryTab(e.DataAcquire, e.FilterId));
                    ShowMiscWindow();
                } else {
                    ShowMiscWindow(v);
                }
            });
            ShowWaferMap = new DelegateCommand<ITab>((e) => {
                var v = FindOpendTab(e.DataAcquire, e.FilterId, TabType.WaferMapTab);
                if (v is null) {
                    AddMiscTab(new WaferMapTab(e.DataAcquire, e.FilterId));
                    ShowMiscWindow();
                } else {
                    ShowMiscWindow(v);
                }

            });
            ShowChart = new DelegateCommand<ITab>((e) => {

                AddMiscTab(new ItemChartTab(e.DataAcquire, e.FilterId, null));
                ShowMiscWindow();
            });

            ShowCorrelation = new DelegateCommand<IList<object>>((e) => {
                if (e.Count < 2) {
                    MessageBox.Show("Select at least 2 items!");
                    return;
                }
                foreach(var t in e)
                    if (t.GetType().Name != "RawGridTab") {
                        MessageBox.Show("Select Raw Data Items Please!");
                        return;
                    }

                var ll = (from r in e
                          let p = new Tuple<IDataAcquire, int>(((ITab)((RawGridTab)r).DataContext).DataAcquire, ((ITab)((RawGridTab)r).DataContext).FilterId)
                          select p).ToList();

                AddDataTab(new CorrelationTab(ll));
                ShowDataWindow();
            });
        }


        #endregion

    }
}