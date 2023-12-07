using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using SharpCompress.Common;
using System.Text.RegularExpressions;
using SharpCompress.Readers;
using AIRdrop.UI;
using SharpCompress.Archives.SevenZip;
using System.Linq;
using SharpCompress.Archives;

namespace AIRdrop
{
    public class ModDownloader
    {
        private string URL_TO_ARCHIVE;
        private string URL;
        private string DL_ID;
        private string MOD_TYPE;
        private string MOD_ID;
        private string fileName;
        private string fileDescription;
        private bool cancelled;
        private bool downloadAll;
        private HttpClient client = new();
        private CancellationTokenSource cancellationToken = new();
        private GameBananaAPIV4 response = new();
        private ProgressBox progressBox;
        public async void BrowserDownload(GameBananaRecord record)
        {
            DownloadWindow downloadWindow = new DownloadWindow(record);
            downloadWindow.ShowDialog();
            if (downloadWindow.YesNo)
            {
                string downloadUrl = null;
                string fileName = null;
                if (record.AllFiles.Count == 1)
                {
                    downloadUrl = record.AllFiles[0].DownloadUrl;
                    fileName = record.AllFiles[0].FileName;
                    fileDescription = record.AllFiles[0].Description;
                }
                else if (record.AllFiles.Count > 1)
                {
                    UpdateFileBox fileBox = new UpdateFileBox(record.AllFiles, record.Title);
                    fileBox.Activate();
                    fileBox.ShowDialog();
                    downloadAll = fileBox.selectedDownloadAll;
                    downloadUrl = fileBox.chosenFileUrl;
                    fileName = fileBox.chosenFileName;
                    fileDescription = fileBox.chosenFileDescription;
                }
                if (downloadAll)
                {
                    foreach (GameBananaItemFile file in record.AllFiles)
                    {
                        downloadUrl = file.DownloadUrl;
                        fileName = file.FileName;
                        fileDescription = file.Description;
                        if (downloadUrl != null && fileName != null)
                        {
                            await DownloadFile(downloadUrl, fileName, new Progress<DownloadProgress>(ReportUpdateProgress),
                                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Token));
                        }
                    }
                }
                else
                {
                    if (downloadUrl != null && fileName != null)
                    {
                        await DownloadFile(downloadUrl, fileName, new Progress<DownloadProgress>(ReportUpdateProgress),
                            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Token));
                    }
                }
            }
        }
        public async void Download(string line, bool running)
        {
            if (ParseProtocol(line))
            {
                if (await GetData())
                {
                    DownloadWindow downloadWindow = new DownloadWindow(response);
                    downloadWindow.ShowDialog();
                    if (downloadWindow.YesNo)
                    {
                        await DownloadFile(URL_TO_ARCHIVE, fileName, new Progress<DownloadProgress>(ReportUpdateProgress),
                            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Token));
                    }
                }
            }
            if (running)
                Environment.Exit(0);
        }

        private async Task<bool> GetData()
        {
            try
            {
                string responseString = await client.GetStringAsync(URL);
                response = JsonSerializer.Deserialize<GameBananaAPIV4>(responseString);
                fileName = response.Files.Where(x => x.Id == DL_ID).ToArray()[0].FileName;
                fileDescription = response.Files.Where(x => x.Id == DL_ID).ToArray()[0].Description;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error while fetching data {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }
        private void ReportUpdateProgress(DownloadProgress progress)
        {
            if (progress.Percentage == 1)
            {
                progressBox.finished = true;
            }
            progressBox.progressBar.Value = progress.Percentage * 100;
            progressBox.taskBarItem.ProgressValue = progress.Percentage;
            progressBox.progressTitle.Text = $"Downloading {progress.FileName}...";
            progressBox.progressText.Text = $"{Math.Round(progress.Percentage * 100, 2)}% " +
                $"({StringConverters.FormatSize(progress.DownloadedBytes)} of {StringConverters.FormatSize(progress.TotalBytes)})";
        }

        private bool ParseProtocol(string line)
        {
            try
            {
                line = line.Replace("sonic3airdrop:", "");
                string[] data = line.Split(',');
                URL_TO_ARCHIVE = data[0];
                // Used to grab file info from dictionary
                var match = Regex.Match(URL_TO_ARCHIVE, @"\d*$");
                DL_ID = match.Value;
                MOD_TYPE = data[1];
                MOD_ID = data[2];
                URL = $"https://gamebanana.com/apiv6/{MOD_TYPE}/{MOD_ID}?_csvProperties=_sName,_aGame,_sProfileUrl,_aPreviewMedia,_sDescription,_aSubmitter,_aCategory,_aSuperCategory,_aFiles,_tsDateUpdated,_aAlternateFileSources,_bHasUpdates,_aLatestUpdates";
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error while parsing {line}: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }
        private async Task DownloadFile(string uri, string fileName, Progress<DownloadProgress> progress, CancellationTokenSource cancellationToken)
        {
            var modsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sonic3AIR", "mods");
            var downloadPath = Path.Combine(modsFolder, fileName);
            try
            {
                // Create the downloads folder if necessary
                Directory.CreateDirectory(modsFolder);
                // Download the file if it doesn't already exist
                if (File.Exists(downloadPath))
                {
                    try
                    {
                        File.Delete(downloadPath);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Couldn't delete the already existing {downloadPath} ({e.Message})", 
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                progressBox = new ProgressBox(cancellationToken);
                progressBox.progressBar.Value = 0;
                progressBox.finished = false;
                progressBox.Title = $"Download Progress";
                progressBox.Show();
                progressBox.Activate();
                // Write and download the file
                using (var fs = new FileStream(
                   downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await client.DownloadAsync(uri, fs, fileName, progress, cancellationToken.Token);
                }
                progressBox.Close();
                MessageBox.Show($"Finished installing {fileName}!");
            }
            catch (OperationCanceledException)
            {
                // Remove the file is it will be a partially downloaded one and close up
                File.Delete(downloadPath);
                if (progressBox != null)
                {
                    progressBox.finished = true;
                    progressBox.Close();
                    cancelled = true;
                }
                return;
            }
            catch (Exception e)
            {
                if (progressBox != null)
                {
                    progressBox.finished = true;
                    progressBox.Close();
                }
                MessageBox.Show($"Error whilst downloading {fileName}. {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cancelled = true;
            }
        }

    }
}
