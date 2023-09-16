using System.Text;
using KyoFileSignerCLI.Authentication;
using KyoFileSignerCLI.Common;
using Newtonsoft.Json;
namespace KyoFileSignerCLI.Http
{
    public class AuthenticationRequest : Request
    {
        private readonly AppClientFactory _appClientFactory;
        public AuthenticationRequest(AppClientFactory appClientFactory)
        {
            _appClientFactory = appClientFactory ?? throw new ArgumentNullException(nameof(appClientFactory));
        }

        public async Task Register(CancellationToken cancellationToken, RegisterUser registerUser)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.PostRequest(ApiConstants.RegisterPath);
            request.Content = new StringContent(JsonConvert.SerializeObject(registerUser), Encoding.UTF8, "application/json");

            Console.WriteLine("Connecting to the server. Please wait.");

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadOnly<bool>(response, (responseModel) =>
               {
                   if (responseModel.ErrorExists())
                   {
                       responseModel.PrintErrors();
                   }
                   else
                   {
                       Console.WriteLine("Your registration request has been sent. Please wait until the administrator processes your request.\n");
                   }
               }, cancellationToken);
            }
        }
        public async Task<string?> Login(CancellationToken cancellationToken, LoginUser loginUser)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.PostRequest(ApiConstants.LoginPath);
            request.Content = new StringContent(JsonConvert.SerializeObject(loginUser), Encoding.UTF8, "application/json");

            Console.WriteLine("Connecting to the server. Please wait.");
            string? token = null;

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadOnly<Dictionary<string, dynamic>>(response, (responseModel) =>
                {
                    try
                    {
                        // Sometimes during Status200, an error might occur such as 'Invalid credentials', causing Result to be null
                        token = ((IResponse.Only<Dictionary<string, dynamic>>)responseModel).Result.GetValueOrDefault("token").ToString();

                        if (string.IsNullOrEmpty(token))
                        {
                            throw new InvalidDataException("Invalid token value");
                        }

                        Console.WriteLine("You have successfully logged in.\n");
                    }
                    catch (Exception err)
                    {
                        responseModel.PrintErrors();
                        return;
                        // throw;
                    }
                    responseModel.PrintErrors();
                }, cancellationToken);
            }

            return token;
        }

        public async Task<string?> ResetPassword(CancellationToken cancellationToken, ResetPassword resetPassword)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.PostRequest(ApiConstants.ResetPasswordPath);
            request.Content = new StringContent(JsonConvert.SerializeObject(resetPassword), Encoding.UTF8, "application/json");

            Console.WriteLine("Request to reset password has been sent. Please wait.");
            string? token = null;

            using (var response = await appClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                await ReadOnly<Dictionary<string, dynamic>>(response, (responseModel) =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(token))
                        {
                            throw new InvalidDataException("Invalid token value");
                        }

                        Console.WriteLine("You have successfully logged in.");
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                        throw;
                    }

                    responseModel.PrintErrors();
                }, cancellationToken);
            }

            return token;
        }

        public async Task ChangePassword(CancellationToken cancellationToken, ChangePassword changePassword)
        {
            var appClient = _appClientFactory.Create();
            var request = AppClient.PostRequest(ApiConstants.ChangePasswordPath);
            request.Content = new StringContent(JsonConvert.SerializeObject(changePassword), Encoding.UTF8, "application/json");

            Console.WriteLine("Please wait while we update your credentials.");

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
                       Console.WriteLine("Password changed successfully.");
                   }
               }, cancellationToken);
            }
        }
    }
}