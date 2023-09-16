namespace KyoFileSignerCLI.Common
{
    public class ApiConstants
    {
        public const string ApiBaseUrl = "https://localhost:7777/api/v1";
        public const string LoginPath = "/auth/login";
        public const string RegisterPath = "/auth/register";
        public const string ChangePasswordPath = "/auth/changepassword";
        public const string ResetPasswordPath = "/auth/resetpassword";
        public const string UsersPath = "/users";
        public const string CertificatesPath = "/certificates";
        public const string TasksPath = "/tasks";
        public static string UserPathById(int userId)
        {
            return $"/users/{userId}";
        }
        public static string CertificatePathById(int certificateId)
        {
            return $"/certificates/{certificateId}";
        }
        public static string TaskPathById(int taskId)
        {
            return $"/tasks/{taskId}";
        }
        public static string DownloadPath(int taskId)
        {
            return $"/tasks/{taskId}/download";
        }
    }
}