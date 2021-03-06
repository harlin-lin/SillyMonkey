﻿using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;

namespace SillyMonkeyD.ViewModels {
    public class RawGridTabViewModel : ViewModelBase, ITab {

        public RawGridTabViewModel(IDataAcquire dataAcquire, int filterId, TabItem tab) {
            Init(dataAcquire, filterId);
            CorrespondingTab = tab;
            InitUI();
        }

        const int MinPerPageCount = 30;
        const int DefaultPerPageCount = 100;

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }

        private int _countPerPage;

        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public int WindowFlag { get; private set; }
        public TabType TabType { get { return TabType.RawDataTab; } }
        public bool IsMainTab { get { return true; } }
        public Thickness LocationInTablist { get { return IsMainTab ? new Thickness(0, 0, 3, 0) : new Thickness(25, 0, 3, 0); } }
        public TabItem CorrespondingTab { get; }

        //public DataTable Data { get { return GetProperty(() => Data); } private set { SetProperty(() => Data, value); } }
        public StdLogGridModel Data { get { return GetProperty(() => Data); } private set { SetProperty(() => Data, value); } }

        public int CountPerPage {
            get { return _countPerPage; }
            set {
                if (value > MinPerPageCount)
                    _countPerPage = value;
                else
                    _countPerPage = MinPerPageCount;
            }
        }
        public int TotalCount { get { return GetProperty(() => TotalCount); } private set { SetProperty(() => TotalCount, value); } }
        public int TotalPages { get { return GetProperty(() => TotalPages); } private set { SetProperty(() => TotalPages, value); } }
        public int CurrentPageIndex { get { return GetProperty(() => CurrentPageIndex); } private set { SetProperty(() => CurrentPageIndex, value); } }




        private void Init(IDataAcquire dataAcquire, int filterId) {
            DataAcquire = dataAcquire;
            FilterId = filterId;
            WindowFlag = 1;

            var i = dataAcquire.GetFilterIndex(filterId);

            if (DataAcquire.FileName.Length > 15)
                TabTitle = DataAcquire.FileName.Substring(0, 15) + "..." + $"-F{i}-RAW";
            else
                TabTitle = DataAcquire.FileName + $"-F{i}-RAW";
            FilePath = DataAcquire.FilePath;

            CountPerPage = DefaultPerPageCount;
            TotalCount = DataAcquire.GetFilteredChipSummary(FilterId).TotalCount;
            TotalPages = TotalCount / CountPerPage + 1;
            CurrentPageIndex = 1;
            
            if (TotalPages > 1)
                Data = new StdLogGridModel(new StdLogTable(DataAcquire, 0, CountPerPage, FilterId));
            else
                Data = new StdLogGridModel(new StdLogTable(DataAcquire, 0, TotalCount, FilterId));

            //UpdateDataToStartPage();


        }

        private void UpdateDataToStartPage() {
            CurrentPageIndex = 1;
            if (TotalPages > 1)
                //Data = DataAcquire.GetFilteredItemData(0, CountPerPage, FilterId, true);
                Data.ChangePage(0, CountPerPage);
            else
                //Data = DataAcquire.GetFilteredItemData(0, TotalCount, FilterId, true);
                Data.ChangePage(0, TotalCount);

            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateDataToLastPage() {
            if (CurrentPageIndex > 1) {
                CurrentPageIndex--;
                //Data = DataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, CountPerPage, FilterId, true);
                Data.ChangePage((CurrentPageIndex - 1) * CountPerPage, CountPerPage);
            }
            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateDataToNextPage() {
            if (CurrentPageIndex < TotalPages) {
                CurrentPageIndex++;
                int leftCnt = TotalCount - (CurrentPageIndex - 1) * CountPerPage;
                if (leftCnt > CountPerPage)
                    //Data = DataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, CountPerPage, FilterId, true);
                    Data.ChangePage((CurrentPageIndex - 1) * CountPerPage, CountPerPage);
                else
                    //Data = DataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, leftCnt, FilterId, true);
                    Data.ChangePage((CurrentPageIndex - 1) * CountPerPage, leftCnt);

            }
            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateDataToEndPage() {
            CurrentPageIndex = TotalPages;
            int leftCnt = TotalCount - (CurrentPageIndex - 1) * CountPerPage;
            //Data = DataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, leftCnt, FilterId, true);
            Data.ChangePage((CurrentPageIndex - 1) * CountPerPage, leftCnt);

            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }

        public void UpdateFilter() {
            CountPerPage = DefaultPerPageCount;
            TotalCount = DataAcquire.GetFilteredChipSummary(FilterId).TotalCount;
            TotalPages = TotalCount / CountPerPage + 1;
            CurrentPageIndex = 1;
            RaisePropertyChanged("TotalPages");
            RaisePropertyChanged("TotalCount");

            UpdateDataToStartPage();
        }

        #region UI
        public ICommand JumpStartPage { get; private set; }
        public ICommand JumpLastPage { get; private set; }
        public ICommand JumpNextPage { get; private set; }
        public ICommand JumpEndPage { get; private set; }
        public ICommand ExportToExcel { get; private set; }
        public ICommand CreateHistogram { get; private set; }

        private void InitUI() {
            JumpStartPage = new DelegateCommand(() => { UpdateDataToStartPage(); });
            JumpLastPage = new DelegateCommand(() => { UpdateDataToLastPage(); });
            JumpNextPage = new DelegateCommand(() => { UpdateDataToNextPage(); });
            JumpEndPage = new DelegateCommand(() => { UpdateDataToEndPage(); });

            CreateHistogram = new DelegateCommand(() => {
                ;
            });

        }

        #endregion

    }
}