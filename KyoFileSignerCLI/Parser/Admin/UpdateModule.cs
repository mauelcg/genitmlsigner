using ConsoleRouting;
using KyoFileSignerCLI.Models;
using Microsoft.Extensions.DependencyInjection;
using KyoFileSignerCLI.Http;
using KyoFileSignerCLI.Authentication;

namespace KyoFileSignerCLI.Parser.Admin
{
    [Module, Command("update")]
    public class UpdateModule
    {
        [Command]
        public void User([Alt("email")] Flag<string> emailAddress,
            Flag<string> password,
            [Alt("user-id")] Flag<int> userId,
            [Alt("first-name")][Optional] Flag<string> firstName,
            [Alt("last-name")][Optional] Flag<string> lastName,
            [Optional] Flag<string> status)
        {
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

            AppUser.TOKEN = token;

            var adminRequest = Program.GetService().GetRequiredService<AdminRequest>();
            var user = adminRequest.GetUser(CancellationToken.None, userId.Value, token).Result;
            if (user == null)
            {
                return;
            }
            Console.WriteLine($"{user.Id} {user.EmailAddress} {user.FirstName} {user.LastName} {user.Status}\n");

            Dictionary<string, object> updates = new()
            {
                { "firstName", firstName.Value ?? user.FirstName },
                { "lastName", lastName.Value ?? user.LastName },
            };

            var newStatus = user.Status;

            if (status.IsSet)
            {
                // Can we do this in back end instead?
                switch (status.Value.ToUpper())
                {
                    case "ACTIVE":
                        newStatus = "ACTIVE";
                        break;
                    case "DISABLED":
                        newStatus = "DISABLED";
                        break;
                    default:
                        Console.WriteLine("Invalid status provided. The options are: ACTIVE | DISABLED.");
                        return;
                }
            }

            updates.Add("status", newStatus);
            user.Updates = updates;

            user = adminRequest.PatchUser(CancellationToken.None, user, token).Result;
            Console.WriteLine($"{user.Id} {user.EmailAddress} {user.FirstName} {user.LastName} {user.Status}\n");
        }


        [Command]
        public void Certificate([Alt("email")] Flag<string> emailAddress,
        Flag<string> password,
        [Alt("certificate-id")] Flag<int> certificateId,
        [Alt("certificate-name")][Optional] Flag<string> certificateName,
        [Alt("thumbprint")][Optional] Flag<string> thumbprint,
        [Alt("certificate-password")][Optional] Flag<string> certificatePassword,
        [Alt("expiry")][Optional] Flag<string> expiry,
        [Alt("status")][Optional] Flag<CertificateStatus> status,
        [Alt("is-default")][Optional] Flag<bool> isDefault)
        {
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

            AppUser.TOKEN = token;

            var adminRequest = Program.GetService().GetRequiredService<AdminRequest>();
            var certificate = adminRequest.GetCertificate(CancellationToken.None, certificateId.Value, token).Result;
            if (certificate == null)
            {
                return;
            }

            Dictionary<string, object> updates = new()
            {
                { "name", certificateName.Value ?? "" },
                { "thumbprint", thumbprint.Value ?? "" },
                { "password", certificatePassword.Value ?? "" },
                { "expiry", expiry.Value ?? "" },
                { "isDefault", isDefault.Value },
            };

            Console.WriteLine(status.IsSet);
            Console.WriteLine(status.HasValue);

            if (status.IsSet)
            {
                updates.Add("status", status.Value);
            }

            certificate.Updates = updates;
            adminRequest.PatchCertificate(CancellationToken.None, certificate, token).Wait();
        }
    }
}