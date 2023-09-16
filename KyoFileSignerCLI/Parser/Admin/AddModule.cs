using ConsoleRouting;
using KyoFileSignerCLI.Models;
using Microsoft.Extensions.DependencyInjection;
using KyoFileSignerCLI.Http;
using KyoFileSignerCLI.Authentication;

namespace KyoFileSignerCLI.Parser.Admin
{
    [Module, Command("add")]
    public class AddModule
    {
        [Command]
        // dotnet run add certificate --email Root@ddp.kyocera.com --password Root@Password --certificate-name Kyocera --certificate-password Kyocera@Pass --status active
        public void Certificate([Alt("email")] Flag<string> emailAddress,
        Flag<string> password,
        [Alt("certificate-name")] Flag<string> certificateName,
        [Alt("thumbprint")][Optional] Flag<string> thumbprint,
        [Alt("certificate-password")] Flag<string> certificatePassword,
        [Alt("expiry")] Flag<string> expiry,
        [Alt("status")] Flag<string> status,
        [Alt("is-default")][Optional] Flag<bool> isDefault)
        {
            if (!status.HasValue)
            {
                Console.WriteLine("Append:\n--email-address <value>\n--status <value>\n--status <value>");
                return;
            }

            var authenticationRequest = Program.GetService().GetRequiredService<AuthenticationRequest>();

            // Request request = new Request();

            // var result = request.Login(emailAddress: emailAddress, password: password).Result;

            // Console.WriteLine(result!["message"]);

            // if (!result.ContainsKey("token"))
            //     return;

            // Certificate certificate = new Certificate
            // {
            //     Name = certificateName,
            //     Thumbprint = thumbprint.Value ?? "",
            //     Password = certificatePassword,
            //     Expiry = DateTime.Parse(expiry.Value),
            //     Status = status.ToString(),
            //     IsDefault = isDefault.Value
            // };

            // var postResult = request.PostCertificate(certificate, result["token"]).Result;
        }

        [Command]
        public void User([Alt("email")] Flag<string> emailAddress,
        Flag<string> password,
        [Alt("first-name")] Flag<string> firstName,
        [Alt("last-name")] Flag<string> lastName,
        [Alt("user-email")] Flag<string> userEmailAddress,
        [Alt("status")][Optional] Flag<string> status)
        {
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

            AppUser.TOKEN = token;

            var adminRequest = Program.GetService().GetRequiredService<AdminRequest>();
            User user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = userEmailAddress,
            };
            if (!status.IsSet)
            {
                user.Status = UserStatus.DISABLED.ToString();
            }
            else {
                user.Status = status.Value.ToUpper();
            }

            var userId = adminRequest.AddUser(CancellationToken.None, user, token).Result;
            if (userId == null)
            {
                return;
            }

            Console.WriteLine($"{userId} {user.EmailAddress} {user.FirstName} {user.LastName} {user.Status}");
        }
    }
}