﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;

namespace SillyMonkeyD.ViewModels {
    public class MiscViewModel : ViewModelBase {
        public ObservableCollection<TabItem> DataTabItems { get; private set; }
        public ITab SelectedTab { get { return GetProperty(() => SelectedTab); } set { SetProperty(() => SelectedTab, value); } }

        public SelectedTabHandler SelectedTabEvent;

        public MiscViewModel() {
            DataTabItems = new ObservableCollection<TabItem>();
            SelectedTab = null;

            InitUiCtr();
        }

        public void AddTab(TabItem tabItem) {
            DataTabItems.Add(tabItem);
            FocusTab(tabItem);
        }

        public void RemoveTab(TabItem tabItem) {
            DataTabItems.Remove(tabItem);
        }

        public void FocusTab(TabItem tabItem) {
            if (tabItem is null) return;
            tabItem.IsSelected = true;
        }

        #region UI
        public ICommand TabSelectionChanged { get; private set; }

        private void InitUiCtr() {
            TabSelectionChanged = new DelegateCommand(() => {
                SelectedTabEvent?.Invoke(SelectedTab);
            });



        }

        #endregion
    }
}