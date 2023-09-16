using System.Diagnostics;

namespace KyoFileSignerCLI.Utilities
{
    public class Signer
    {
        private const string SIGNTOOL = @"Resources\signtool.exe";
        private const string CERTIFICATE = @"Resources\ta-cert.cer";

        private string SourcePath { get; set; }
        private string DestinationPath { get; set; }
        private int TotalCount { get; set; }
        private int CurrentCount { get; set; }

        public Signer(string sourcePath, string destinationPath)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            TotalCount = 0;
            CurrentCount = 0;
        }

        public async Task Start()
        {
            if (!Directory.Exists(DestinationPath))
            {
                Directory.CreateDirectory(DestinationPath);
            }
            CountFilesRecursively(SourcePath);
            await SignFilesRecursively(SourcePath);
        }

        private void CountFilesRecursively(string stringPath)
        {
            var directories = from directory in Directory.EnumerateDirectories(Path.GetFullPath(stringPath)) select directory;

            foreach (var directory in directories)
            {
                CountFilesRecursively(directory);
            }

            var files = from file in Directory.EnumerateFiles(Path.GetFullPath(stringPath)) select file;
            TotalCount += files.Count();
        }

        private string GetSigningArguments(string file, string destinationPath)
        {
            var parameters = new string[]{
                "sign",
                "/dg",
                destinationPath,
                "/f",
                CERTIFICATE,
                "/fd",
                "sha256",
                file
            };

            return string.Join(" ", parameters);
        }

        public async Task<bool> SignFilesRecursively(string stringPath)
        {
            var directories = from directory in Directory.EnumerateDirectories(Path.GetFullPath(stringPath)) select directory;
            foreach (var directory in directories)
            {
                string suffix = directory.Replace(SourcePath, "");
                string fullPath = Path.Join(DestinationPath, suffix);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                else
                {
                    Console.WriteLine($"Directory {DestinationPath} exists already.");
                }

                await SignFilesRecursively(directory);
            }

            var files = from file in Directory.EnumerateFiles(Path.GetFullPath(stringPath)) select file;
            foreach (var file in files)
            {
                string suffix = file.Replace($"\\{Path.GetFileName(file)}", "").Replace(SourcePath, "");

                // Create directory for each indiviual file
                Directory.CreateDirectory(Path.Join(DestinationPath, suffix, Path.GetFileNameWithoutExtension(file)));
                string fullPath = Path.Join(DestinationPath, suffix, Path.GetFileNameWithoutExtension(file));

                // Add quotation marks for possible paths with whitespace
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(SIGNTOOL, GetSigningArguments($"\"{file}\"", $"\"{fullPath}\""))
                    {
                        // FileName = "signtool.exe",
                        // Arguments = $"sign /v /dg \"{fullPath}\" /fd SHA256 /f ta-cert.cer \"{file}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                CurrentCount++;
                int progress = (int)Math.Round(100 * (CurrentCount / (double)TotalCount));
                Console.WriteLine($"Signing: {progress}% ({CurrentCount}|{TotalCount} {file}) {process.StandardOutput.ReadToEndAsync().Result}");

                await process.WaitForExitAsync();
            }

            return true;
        }
    }
}