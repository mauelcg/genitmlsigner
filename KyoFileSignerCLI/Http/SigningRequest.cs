using System.Net.Http.Headers;
using System.Text.Json;
using KyoFileSignerCLI.Common;
using KyoFileSignerCLI.Models;

namespace KyoFileSignerCLI.Http
{
    public class SigningRequest : Request
    {
        private readonly AppClientFactory _appClientFactory;
        public SigningRequest(AppClientFactory appClientFactory)
        {
            _appClientFactory = appClientFactory ?? throw new ArgumentNullException(nameof(appClientFactory));
        }
        public async Task<IReadOnlyCollection<SigningTask>> GetTasks(CancellationToken cancellationToken)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.GetRequest(ApiConstants.TasksPath);
            using (var result = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                using (var contentStream = await result.Content.ReadAsStreamAsync())
                {
                    return await JsonSerializer.DeserializeAsync<List<SigningTask>>(contentStream, DefaultJsonSerializerOptions.Options, cancellationToken);
                }
            }
        }

        public async Task Download(CancellationToken cancellationToken, int taskId, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.GetRequest(ApiConstants.DownloadPath(taskId), token);
            int? result = null;

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                var downloadsPath = Path.GetFullPath(@"C:\Users\genitaml\Downloads");
                Directory.CreateDirectory(downloadsPath);
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.Create($"{Path.Combine(downloadsPath, $"{Guid.NewGuid().ToString()}")}"))
                {
                    contentStream.CopyTo(fileStream);
                }
            }
        }

        public async Task<int?> AddTask(CancellationToken cancellationToken, CreateTask createTask, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.PostRequest(ApiConstants.TasksPath, token);
            int? result = null;

            using (var streams = new StreamCollection())
            {
                long length = 0;
                foreach (string filePath in createTask.FilesToSign)
                {
                    FileStream fileStream = File.OpenRead(filePath);
                    var fileLength = fileStream.Length;
                    length += fileStream.Length;
                    streams.Add(fileStream);
                }

                Console.WriteLine("Preparing files. Please wait.");
                request.Content = PrepareFiles(createTask, streams, length);

                Console.WriteLine("Uploading files. Do not turn off your pc while uploading.");

                bool track = true;
                new Task(new Action(() => { ProgressTracker(streams, length, ref track); })).Start();

                using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    await ReadOnly<Dictionary<string, dynamic>>(response, (responseModel) =>
                    {
                        track = false;

                        Console.Clear();
                        Console.WriteLine($"Uploading: {100}%");

                        try
                        {
                            // Sometimes during Status200, an error might occur such as 'Invalid credentials', causing Result to be null
                            result = int.Parse(((IResponse.Only<Dictionary<string, dynamic>>)responseModel).Result.GetValueOrDefault("id").ToString());
                            Console.WriteLine("Your files have been received. Please check your files later while we are signing your files in the background.");
                        }
                        catch (Exception err)
                        {
                            responseModel.PrintErrors();
                            return;
                        }
                        responseModel.PrintErrors();
                    }, cancellationToken);

                    // In case of failure, set track to false. A much better approach is to provide failureHandler.
                    track = false;
                }
            }

            return result;
        }

        private void ProgressTracker(StreamCollection streams, long totalLength, ref bool track)
        {

            var currentStream = streams.First();
            int count = 0;
            long length = 0;

            while (track)
            {
                int progress = (int)Math.Round(100 * ((length + currentStream.Position) / (double)totalLength));

                if (currentStream.Position == currentStream.Length)
                {
                    length += currentStream.Length;
                    if (count < streams.Count)
                    {
                        currentStream = streams[count + 1];
                        count++;
                    }
                }

                Console.Clear();
                Console.WriteLine($"Uploading: {progress}%");
                Thread.Sleep(100);
            }
        }

        private MultipartFormDataContent PrepareFiles(CreateTask createTask, StreamCollection streams, long length)
        {
            MultipartFormDataContent formData = new MultipartFormDataContent();
            var description = new StringContent(createTask.Description);
            description.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "description"
            };

            var certificate = new StringContent(createTask.CertificateId.ToString());
            certificate.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "certificateId"
            };

            formData.Add(description);
            formData.Add(certificate);

            var lengthData = new StringContent(string.Format("{0}", length));
            lengthData.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "length"
            };
            formData.Add(lengthData);

            // For single file upload
            for (int i = 0; i < streams.Count; ++i)
            {
                var file = new StreamContent(streams[i]);

                file.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = Path.GetFileName(createTask.FilesToSign[i])
                };

                formData.Add(file);
            }

            // for (int i = 0; i < streams.Count; ++i)
            // {
            //     var file = new StreamContent(streams[i]);

            //     file.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            //     {
            //         Name = $"filesToSign{i + 1}",
            //         FileName = Path.GetFileName(createTask.FilesToSign[i])
            //     };

            //     formData.Add(file);
            // }

            return formData;
        }
    }
}