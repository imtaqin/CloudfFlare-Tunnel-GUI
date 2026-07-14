using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CFTunnel.Services
{
    public class AppSettings
    {
        public string? ApiTokenProtected { get; set; }
        public string ApiEmail { get; set; } = string.Empty;

        private static string Dir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CFTunnel");

        private static string FilePath => Path.Combine(Dir, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings();
            }
            catch { }
            return new AppSettings();
        }

        public void Save()
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }

        // Token is DPAPI-protected per Windows user; unreadable from other accounts or machines.
        public string GetApiToken()
        {
            if (string.IsNullOrEmpty(ApiTokenProtected))
                return string.Empty;
            try
            {
                var data = ProtectedData.Unprotect(Convert.FromBase64String(ApiTokenProtected), null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return string.Empty;
            }
        }

        public void SetApiToken(string token)
        {
            ApiTokenProtected = string.IsNullOrEmpty(token)
                ? null
                : Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(token), null, DataProtectionScope.CurrentUser));
        }
    }
}
