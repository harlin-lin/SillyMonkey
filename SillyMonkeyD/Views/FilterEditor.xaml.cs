﻿using DataInterface;
using SillyMonkeyD.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SillyMonkeyD.Views {
    /// <summary>
    /// Interaction logic for FilterEditor.xaml
    /// </summary>
    public partial class FilterEditor : Window {
        public FilterEditorViewModel filter { get; }
        public FilterEditor(IDataAcquire dataAcquire, int filterId) {
            InitializeComponent();

            filter = new FilterEditorViewModel(dataAcquire, filterId);
            DataContext = filter;

        }
    }
}