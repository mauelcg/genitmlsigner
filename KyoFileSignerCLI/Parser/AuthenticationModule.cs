using ConsoleRouting;
using KyoFileSignerCLI.Authentication;
using KyoFileSignerCLI.Http;
using KyoFileSignerCLI.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KyoFileSignerCLI.Parser
{
    [Module]
    public class AuthenticationModule
    {
        [Command]
        public void Register([Alt("email")] Flag<string> emailAddress, Flag<string> password, [Alt("first-name")][Optional] Flag<string> firstName, [Alt("last-name")][Optional] Flag<string> lastName)
        {
            Console.Clear();

            var request = Program.GetService().GetRequiredService<AuthenticationRequest>();

            RegisterUser registerUser = new()
            {
                Email = emailAddress.Value,
                Password = password.Value,
                FirstName = firstName.Value ?? "User",
                LastName = lastName.Value ?? "User"
            };

            request.Register(cancellationToken: CancellationToken.None, registerUser).Wait();
        }

        [Command]
        public void Login([Alt("email")] Flag<string> emailAddress, Flag<string> password)
        {
            Console.Clear();

            var request = Program.GetService().GetRequiredService<AuthenticationRequest>();

            LoginUser loginUser = new()
            {
                Email = emailAddress.Value,
                Password = password.Value
            };

            var token = request.Login(cancellationToken: CancellationToken.None, loginUser).Result;
            if (token == null)
            {
                return;
            }

            AppUser.TOKEN = token;
        }

        [Command]
        public void ResetPassword([Alt("email")] Flag<string> emailAddress)
        {
            Console.Clear();

            var request = Program.GetService().GetRequiredService<AuthenticationRequest>();

            ResetPassword resetPassword = new()
            {
                Email = emailAddress.Value
            };
        }
    }
}