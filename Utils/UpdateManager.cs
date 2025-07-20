using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WinDurango.UI.Utils;

namespace WinDurango.UI.Utils
{
    public class UpdateManager
    {
        private static readonly HttpClient httpClient = new();
        private const string GITHUB_API_URL = "https://api.github.com/repos/WinDurango/WinDurango.UI/releases/latest";

        public class GitHubRelease
        {
            public string tag_name { get; set; }
            public string name { get; set; }
            public string body { get; set; }
            public bool prerelease { get; set; }
            public GitHubAsset[] assets { get; set; }
        }

        public class GitHubAsset
        {
            public string name { get; set; }
            public string browser_download_url { get; set; }
        }

        public static async Task<GitHubRelease> CheckForUpdatesAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WinDurango-UI");
                var response = await httpClient.GetStringAsync(GITHUB_API_URL);
                var release = JsonSerializer.Deserialize<GitHubRelease>(response);
                
                if (IsNewerVersion(release.tag_name, App.Version))
                {
                    Logger.WriteInformation($"Update available: {release.tag_name}");
                    return release;
                }
                
                Logger.WriteInformation("No updates available");
                return null;
            }
            catch (Exception ex)
            {
                Logger.WriteException($"Failed to check for updates: {ex.Message}");
                return null;
            }
        }

        private static bool IsNewerVersion(string remoteVersion, string currentVersion)
        {
            try
            {
                var remote = Version.Parse(remoteVersion.TrimStart('v'));
                var current = Version.Parse(currentVersion.Split('_')[0]);
                return remote > current;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> DownloadAndInstallUpdateAsync(GitHubRelease release)
        {
            try
            {
                var asset = Array.Find(release.assets, a => a.name.EndsWith(".exe") || a.name.EndsWith(".msix"));
                if (asset == null) return false;

                var tempPath = Path.Combine(Path.GetTempPath(), asset.name);
                
                using var response = await httpClient.GetAsync(asset.browser_download_url);
                using var fileStream = File.Create(tempPath);
                await response.Content.CopyToAsync(fileStream);

                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });

                Environment.Exit(0);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteException($"Failed to download/install update: {ex.Message}");
                return false;
            }
        }
    }
}

// Need to test if update works