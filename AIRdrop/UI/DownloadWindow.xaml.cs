﻿using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AIRdrop
{
    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        public bool YesNo = false;
        public DownloadWindow(GameBananaAPIV4 record)
        {
            InitializeComponent();
            DownloadText.Text = $"{record.Title}\nSubmitted by {record.Owner.Name}";
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = record.Image;
            bitmap.EndInit();
            Preview.Source = bitmap;
        }
        public DownloadWindow(GameBananaRecord record)
        {
            InitializeComponent();
            DownloadText.Text = $"{record.Title}\nSubmitted by {record.Owner.Name}";
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = record.Image;
            bitmap.EndInit();
            Preview.Source = bitmap;
        }
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            YesNo = true;
            
            Close();
        }
        private void No_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
