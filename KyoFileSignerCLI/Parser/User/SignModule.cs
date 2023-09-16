using ConsoleRouting;
using KyoFileSignerCLI.Models;
using KyoFileSignerCLI.Http;
using Microsoft.Extensions.DependencyInjection;
using KyoFileSignerCLI.Authentication;
using KyoFileSignerCLI.Utilities;
using System.Diagnostics;

namespace KyoFileSignerCLI.Parser.Admin
{
    [Module]
    public class SignModule
    {
        [Command]
        public void Sign(string file, [Alt("email")] Flag<string> emailAddress, Flag<string> password, [Alt("certificate-id")] Flag<int> certificateId, Flag<string> description)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Console.Clear();

            var authenticationRequest = Program.GetService().GetRequiredService<AuthenticationRequest>();

            LoginUser loginUser = new()
            {
                Email = emailAddress.Value,
                Password = password.Value
            };

            var token = authenticationRequest.Login(cancellationToken: CancellationToken.None, loginUser).Result;
            if (token == null)
            {
                return;
            }

            // AppUser.TOKEN = token;

            string sourcePath = Path.GetFullPath(file);
            string directoryName = new DirectoryInfo(sourcePath).Name;
            string destinationPath = Path.Combine(Directory.GetCurrentDirectory(), "Output", directoryName);

            var signer = new Signer(sourcePath, destinationPath);
            signer.Start().Wait();

            string compressPath = Path.Combine(Directory.GetCurrentDirectory(), "Output", $"{directoryName}.zip");
            var compressProgress = 0;
            var track = true;

            new Task(new Action(() => { ProgressTracker(ref compressProgress, ref track, ref destinationPath, ref compressPath); })).Start();
            FileUtility.Compress(destinationPath, compressPath, new Progress<double>((progress) =>
            {
                compressProgress = (int)(progress * 100);
            }
            )).Wait();

            track = false;

            string[] filesToSign = new string[]
            {
                compressPath,
            };

            CreateTask createTask = new CreateTask
            {
                FilesToSign = filesToSign,
                CertificateId = certificateId.Value,
                Description = description.Value
            };

            var signingRequest = Program.GetService().GetRequiredService<SigningRequest>();

            signingRequest.AddTask(CancellationToken.None, createTask, token).Wait();

            stopWatch.Stop();
            // Console.WriteLine($@"Elapsed Time: {stopWatch.Elapsed}
            // Elapsed Milliseconds: {stopWatch.ElapsedMilliseconds}
            // Elapsed Ticks: {stopWatch.ElapsedTicks}
            // Is Running: {stopWatch.IsRunning}
            // Frequency: {Stopwatch.Frequency}
            // Using High Resolution: {Stopwatch.IsHighResolution}"
            // );

            Console.WriteLine($@"Elapsed Time: {stopWatch.Elapsed}
            Elapsed Milliseconds: {stopWatch.ElapsedMilliseconds}
            Elapsed Ticks: {stopWatch.ElapsedTicks}
            Frequency: {Stopwatch.Frequency}
            ");
        }

        public void Download(string file, [Alt("email")] Flag<string> emailAddress, Flag<string> password, [Alt("task-id")] Flag<int> taskId)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Console.Clear();

            var authenticationRequest = Program.GetService().GetRequiredService<AuthenticationRequest>();

            LoginUser loginUser = new()
            {
                Email = emailAddress.Value,
                Password = password.Value
            };

            var token = authenticationRequest.Login(cancellationToken: CancellationToken.None, loginUser).Result;
            if (token == null)
            {
                return;
            }

            // AppUser.TOKEN = token;

            var signingRequest = Program.GetService().GetRequiredService<SigningRequest>();

            if (!taskId.IsSet)
            {
                return;
            }

            signingRequest.Download(CancellationToken.None, taskId.Value, token).Wait();

            stopWatch.Stop();
            // Console.WriteLine($@"Elapsed Time: {stopWatch.Elapsed}
            // Elapsed Milliseconds: {stopWatch.ElapsedMilliseconds}
            // Elapsed Ticks: {stopWatch.ElapsedTicks}
            // Is Running: {stopWatch.IsRunning}
            // Frequency: {Stopwatch.Frequency}
            // Using High Resolution: {Stopwatch.IsHighResolution}"
            // );

            Console.WriteLine($@"Elapsed Time: {stopWatch.Elapsed}
            Elapsed Milliseconds: {stopWatch.ElapsedMilliseconds}
            Elapsed Ticks: {stopWatch.ElapsedTicks}
            Frequency: {Stopwatch.Frequency}
            ");
        }

        // public static T Deserialize<T>(byte[] data) where T : class
        // {
        //     using (var stream = new MemoryStream(data))
        //     using (var reader = new StreamReader(stream, Encoding.UTF8))
        //     {
        //         return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
        //     }
        // }

        private void ProgressTracker(ref int progress, ref bool keepTracking, ref string folderPath, ref string compressDirectory)
        {
            while (keepTracking)
            {
                Console.Clear();
                if (progress < 99)
                {
                    Console.WriteLine($"Compressing {folderPath} to {compressDirectory}: {progress}%");
                }
                else
                {
                    Console.WriteLine($"Compressing {folderPath} to {compressDirectory}: {100}%");
                }
                Thread.Sleep(500);
            }
        }
    }
}