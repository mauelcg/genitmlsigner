using System.IO.Compression;
using KyoFileSignerCLI.Helpers;

namespace KyoFileSignerCLI.Utilities
{
    public class FileUtility
    {
        public static async Task<bool> Compress(string sourceDirectoryName, string destinationArchiveFileName, IProgress<double> progress)
        {
            sourceDirectoryName = Path.GetFullPath(sourceDirectoryName);
            FileInfo[] sourceFiles = new DirectoryInfo(sourceDirectoryName).GetFiles("*", SearchOption.AllDirectories);
            double totalBytes = sourceFiles.Sum(f => f.Length);
            long currentBytes = 0;

            using (ZipArchive archive = ZipFile.Open(destinationArchiveFileName, ZipArchiveMode.Create))
            {
                foreach (FileInfo file in sourceFiles)
                {
                    // NOTE: 
                    // Naive method to get sub-path from file name, relative to
                    // input directory. Production code should be more robust than this.
                    // Either use Path class or similar to parse directory separators and
                    // reconstruct output file name, or change this entire method to be
                    // recursive so that it can follow the sub-directories and include them
                    // in the entry name as they are processed.

                    string entryName = file.FullName.Substring(sourceDirectoryName.Length + 1);
                    ZipArchiveEntry entry = archive.CreateEntry(entryName);

                    entry.LastWriteTime = file.LastWriteTime;

                    using (Stream inputStream = File.OpenRead(file.FullName))
                    using (Stream outputStream = entry.Open())
                    {
                        Stream progressStream = new StreamProgress(inputStream,
                            new Progress<int>(i =>
                            {
                                currentBytes += i;
                                progress.Report(currentBytes / totalBytes);
                            }
                            ), null);
                        // progressStream.CopyTo(outputStream);
                        await progressStream.CopyToAsync(outputStream);
                    }
                }
            }

            return true;
        }

        public static void Extract(string sourceArchiveFileName, string destinationDirectoryName, IProgress<double> progress)
        {
            using (ZipArchive archive = ZipFile.OpenRead(sourceArchiveFileName))
            {
                double totalBytes = archive.Entries.Sum(e => e.Length);
                long currentBytes = 0;

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fileName = Path.Combine(destinationDirectoryName, entry.FullName);

                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    using (Stream inputStream = entry.Open())
                    using (Stream outputStream = File.OpenWrite(fileName))
                    {
                        Stream progressStream = new StreamProgress(outputStream, null,
                            new Progress<int>(i =>
                            {
                                currentBytes += i;
                                progress.Report(currentBytes / totalBytes);
                            }));

                        inputStream.CopyTo(progressStream);
                    }
                    File.SetLastWriteTime(fileName, entry.LastWriteTime.LocalDateTime);
                }
            }
        }
    }
}
