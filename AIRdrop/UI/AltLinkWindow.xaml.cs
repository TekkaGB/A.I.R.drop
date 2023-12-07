using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Media;
using System.IO;

namespace AIRdrop.UI
{
    /// <summary>
    /// Interaction logic for UpdateFileBox.xaml
    /// </summary>
    public partial class AltLinkWindow : Window
    {
        public AltLinkWindow(List<GameBananaAlternateFileSource> files, string packageName, string url)
        {
            InitializeComponent();
            FileList.ItemsSource = files;
            TitleBox.Text = packageName;
            Description.Text =$"Links from the Alternate File Sources section were found. You can " +
                $"select one to manually download.\nTo install, extract the downloaded archive into:";
            PathText.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sonic3AIR", "mods");
            UrlText.Text = url;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var item = button.DataContext as GameBananaAlternateFileSource;
            var ps = new ProcessStartInfo(item.Url.AbsoluteUri)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
