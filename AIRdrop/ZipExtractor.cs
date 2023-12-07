using Onova.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Windows;
using AIRdrop;

namespace AIRdrop
{
    public class ZipExtractor : IPackageExtractor
    {
        public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using (Stream stream = File.OpenRead(sourceFilePath))
                using (var reader = ReaderFactory.Open(stream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            reader.WriteEntryToDirectory(destDirPath, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Failed to extract update");
            }
            File.Delete(sourceFilePath);
            // Move the folders to the right place
            string parentPath = Directory.GetParent(destDirPath).FullName;
            Directory.Move(Directory.GetDirectories(destDirPath)[0],Path.Combine(parentPath, "AIRdrop"));
            Directory.Delete(destDirPath);
            Directory.Move(Path.Combine(parentPath, "AIRdrop"), destDirPath);
        }

    }
}
