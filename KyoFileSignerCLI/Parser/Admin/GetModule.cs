using KyoFileSignerCLI.Models;
using ConsoleRouting;
using Microsoft.Extensions.DependencyInjection;
using KyoFileSignerCLI.Http;
using KyoFileSignerCLI.Authentication;

namespace KyoFileSignerCLI.Parser.Admin
{
    [Module, Command("get")]
    public class GetModule
    {
        // [Command]
        // public void Users([Alt("email")] Flag<string> emailAddress, Flag<string> password)
        // {
        //     var authenticationRequest = Program.GetService().GetRequiredService<AuthenticationRequest>();
        //     var token = authenticationRequest.Login(cancellationToken: CancellationToken.None, emailAddress: emailAddress, password: password).Result;
        //     if (token == null)
        //     {
        //         return;
        //     }

        //     AppUser.TOKEN = token;

        //     Console.WriteLine("\nGetting all users. Please wait.");
        //     var adminRequest = Program.GetService().GetRequiredService<AdminRequest>();
        //     var users = adminRequest.GetAllUsers(CancellationToken.None, token).Result;
        //     if (users == null)
        //     {
        //         return;
        //     }

        //     foreach (var user in users)
        //     {
        //         Console.WriteLine($"{user.Id} {user.EmailAddress} {user.FirstName} {user.LastName}");
        //     }
        // }
        [Command]
        public void Users(
        [Alt("email")] Flag<string> emailAddress,
        Flag<string> password,
        [Alt("user-id")][Optional] Flag<int> userId,
        [Alt("first-name")][Optional] Flag<string> firstName,
        [Alt("last-name")][Optional] Flag<string> lastName,
        [Alt("user-email")][Optional] Flag<string> userEmailAddress,
        [Alt("status")][Optional] Flag<string> status)
        {
            Console.Clear();

            var authenticationRequest = Program.GetService().GetRequiredService<AuthenticationRequest>();

            if (!emailAddress.IsSet || !password.IsSet)
            {
                Console.WriteLine("Argument needs email: --email <value>");
                Console.WriteLine("Argument needs password: --password <value>\n");
                return;
            }

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

            var users = new List<User>();
            var adminRequest = Program.GetService().GetRequiredService<AdminRequest>();

            if (userId.IsSet)
            {
                var user = adminRequest.GetUser(CancellationToken.None, userId.Value, token).Result;
                // Null check is redundant as GetUser will throw an error if the associated User is not found
                if (user == null)
                {
                    return;
                }
                users.Add(user);
            }
            else
            {
                var allUsers = adminRequest.GetAllUsers(CancellationToken.None, token).Result;
                if (allUsers == null)
                {
                    return;
                }
                users = allUsers;
            }

            foreach (var user in users)
            {
                Console.WriteLine($"{user.Id} {user.EmailAddress} {user.FirstName} {user.LastName} {user.Status}");
            }
        }

        [Command]
        public void Certificates(
        [Alt("email")] Flag<string> emailAddress,
        Flag<string> password,
        [Alt("certificate-id")][Optional] Flag<int> certificateId,
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

            Console.WriteLine("\nGetting certificate(s). Please wait.");

            var certificates = new List<Certificate>();
            var adminRequest = Program.GetService().GetRequiredService<AdminRequest>();

            if (certificateId.IsSet)
            {
                var certificate = adminRequest.GetCertificate(CancellationToken.None, certificateId.Value, token).Result;
                certificates.Add(certificate);
            }
            else
            {
                var allCertificates = adminRequest.GetAllCertificates(CancellationToken.None, token).Result;
                if (allCertificates == null)
                {
                    return;
                }
                certificates = allCertificates;
            }

            foreach (var certificate in certificates)
            {
                Console.WriteLine($"{certificate.Id} {certificate.Name} {certificate.Thumbprint} {certificate.Password} {certificate.Expiry} {certificate.IsDefault} {certificate.Status}");
            }
        }
    }
}