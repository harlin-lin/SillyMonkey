﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;
using FileHelper;

namespace SillyMonkeyD.ViewModels {
    public class SummaryTabViewModel : ViewModelBase, ITab {
        public delegate void SummaryUpdateHandler();

        public SummaryTabViewModel(IDataAcquire dataAcquire, int filterId, TabItem tab) {
            Init(dataAcquire, filterId);

            CorrespondingTab = tab;

            InitUI();

        }

        private SummaryHelper _summaryHelper;

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }

        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public int WindowFlag { get; private set; }
        public TabType TabType { get { return TabType.SummaryTab; } }
        public bool IsMainTab { get { return false; } }
        public Thickness LocationInTablist { get { return IsMainTab ? new Thickness(0, 0, 3, 0) : new Thickness(25, 0, 3, 0); } }
        public TabItem CorrespondingTab { get; }

        public FlowDocument Summary { get; private set; }

        public SummaryUpdateHandler SummaryUpdateEvent;

        private void Init(IDataAcquire dataAcquire, int filterId) {
            DataAcquire = dataAcquire;
            FilterId = filterId;

            WindowFlag = 2;

            var i = dataAcquire.GetFilterIndex(filterId);

            if (DataAcquire.FileName.Length > 15)
                TabTitle = DataAcquire.FileName.Substring(0, 15) + "..." + $"-F{i}-SUM";
            else
                TabTitle = DataAcquire.FileName + $"-F{i}-SUM";
            FilePath = DataAcquire.FilePath;

            _summaryHelper = new SummaryHelper(dataAcquire, filterId);

            Summary = _summaryHelper.GetSummary();
        }

        public void UpdateFilter() {
            Summary = _summaryHelper.GetSummary();

            SummaryUpdateEvent?.Invoke();
        }

        #region UI
        public ICommand ExportToExcel { get; private set; }

        private void InitUI() {

            ExportToExcel = new DelegateCommand(() => {
                ;
            });

        }

        #endregion

    }
}