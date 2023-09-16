using System.Text;
using KyoFileSignerCLI.Common;
using KyoFileSignerCLI.Models;
using Newtonsoft.Json;

namespace KyoFileSignerCLI.Http
{
    public class AdminRequest : Request
    {
        private readonly AppClientFactory _appClientFactory;
        public AdminRequest(AppClientFactory appClientFactory)
        {
            _appClientFactory = appClientFactory ?? throw new ArgumentNullException(nameof(appClientFactory));
        }
        public async Task<List<User>> GetAllUsers(CancellationToken cancellationToken, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.GetRequest(ApiConstants.UsersPath, token);

            Console.WriteLine("Fetching users. Please wait.\n");
            var results = new List<User>();

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadMany<User>(response, (responseModel) =>
                {
                    if (responseModel.ErrorExists())
                    {
                        responseModel.PrintErrors();
                    }
                    else
                    {
                        results = ((IResponse.Many<User>)responseModel).Items();
                    }
                }, cancellationToken);
                return results;
            }
        }
        public async Task<List<Certificate>> GetAllCertificates(CancellationToken cancellationToken, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.GetRequest(ApiConstants.CertificatesPath, token);

            Console.WriteLine("Fetching certificates. Please wait.\n");
            var results = new List<Certificate>();

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadMany<Certificate>(response, (responseModel) =>
                {
                    if (responseModel.ErrorExists())
                    {
                        responseModel.PrintErrors();
                    }
                    else
                    {
                        results = ((IResponse.Many<Certificate>)responseModel).Items();
                    }
                }, cancellationToken);
                return results;
            }
        }
        public async Task<User?> GetUser(CancellationToken cancellationToken, int id, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.GetRequest(ApiConstants.CertificatePathById(id), token);

            Console.WriteLine("Fetching user records. Please wait.\n");
            User? result = null;

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadOnly<User>(response, (responseModel) =>
                {
                    if (responseModel.ErrorExists())
                    {
                        responseModel.PrintErrors();
                    }
                    else
                    {
                        result = ((IResponse.Only<User>)responseModel).Result;
                    }
                }, cancellationToken);
            }

            return result;
        }
        public async Task<Certificate?> GetCertificate(CancellationToken cancellationToken, int id, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.GetRequest(ApiConstants.CertificatePathById(id), token);

            Console.WriteLine("Fetching certificate information. Please wait.\n");
            Certificate? result = null;

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadOnly<Certificate>(response, (responseModel) =>
                {
                    if (responseModel.ErrorExists())
                    {
                        responseModel.PrintErrors();
                    }
                    else
                    {
                        result = ((IResponse.Only<Certificate>)responseModel).Result;
                    }
                }, cancellationToken);
                return result;
            }
        }

        public async Task<User?> PatchUser(CancellationToken cancellationToken, User user, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.PatchRequest(ApiConstants.UserPathById(user.Id), token);
            request.Content = new StringContent(JsonConvert.SerializeObject(user.toPatchList()), Encoding.UTF8, "application/json");

            Console.WriteLine("Updating user records. Please wait.\n");
            User? result = null;

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadOnly<User>(response, (responseModel) =>
                {
                    if (responseModel.ErrorExists())
                    {
                        responseModel.PrintErrors();
                    }
                    else
                    {
                        result = ((IResponse.Only<User>)responseModel).Result;
                    }
                }, cancellationToken);
            }

            return result;
        }

        public async Task<Certificate?> PatchCertificate(CancellationToken cancellationToken, Certificate certificate, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.PatchRequest(ApiConstants.UserPathById(certificate.Id), token);
            request.Content = new StringContent(JsonConvert.SerializeObject(certificate.toPatchList()), Encoding.UTF8, "application/json");

            Console.WriteLine("Updating certificate records. Please wait.");
            Certificate? result = null;

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadOnly<Certificate>(response, (responseModel) =>
                {
                    if (responseModel.ErrorExists())
                    {
                        responseModel.PrintErrors();
                    }
                    else
                    {
                        result = ((IResponse.Only<Certificate>)responseModel).Result;
                    }
                }, cancellationToken);
            }

            return result;
        }

        public async Task<int?> AddUser(CancellationToken cancellationToken, User user, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.PostRequest(ApiConstants.UsersPath, token);
            request.Content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            Console.WriteLine("Adding user to the database. Please wait.");
            int? result = null;

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadOnly<Dictionary<string, dynamic>>(response, (responseModel) =>
                {
                    if (responseModel.ErrorExists())
                    {
                        responseModel.PrintErrors();
                    }
                    else
                    {
                        result = int.Parse(((IResponse.Only<Dictionary<string, dynamic>>)responseModel).Result.GetValueOrDefault("id").ToString());
                        Console.WriteLine("User records has been added successfully.\n");
                    }
                }, cancellationToken);
            }

            return result;
        }

        public async Task<int?> AddCertificate(CancellationToken cancellationToken, Certificate certificate, string token)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.PostRequest(ApiConstants.CertificatesPath, token);
            request.Content = new StringContent(JsonConvert.SerializeObject(certificate), Encoding.UTF8, "application/json");

            Console.WriteLine("Saving certificate information. Please wait.");
            int? result = null;

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadOnly<Dictionary<string, dynamic>>(response, (responseModel) =>
                {
                    if (responseModel.ErrorExists())
                    {
                        responseModel.PrintErrors();
                    }
                    else
                    {
                        result = int.Parse(((IResponse.Only<Dictionary<string, dynamic>>)responseModel).Result.GetValueOrDefault("id").ToString());
                        Console.WriteLine("Certificate information has been saved successfully.\n");
                    }
                }, cancellationToken);
            }

            return result;
        }
    }
}