using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WinDurango.UI.Utils;

namespace WinDurango.UI.Utils
{
    public class GitHubContributorManager
    {
        private static readonly HttpClient httpClient = new();
        private const string UI_REPO_API = "https://api.github.com/repos/WinDurango/WinDurango.UI/contributors";
        private const string CORE_REPO_API = "https://api.github.com/repos/WinDurango/WinDurango/contributors";

        public class GitHubContributor
        {
            public string login { get; set; }
            public string avatar_url { get; set; }
            public string html_url { get; set; }
            public int contributions { get; set; }
            public string type { get; set; }
        }

        public static async Task<List<GitHubContributor>> GetUIContributorsAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WinDurango-UI");
                var contributors = await GetContributorsFromRepo(UI_REPO_API);
                contributors.Sort((a, b) => b.contributions.CompareTo(a.contributions));
                
                Logger.WriteInformation($"Retrieved {contributors.Count} UI contributors from GitHub");
                return contributors;
            }
            catch (Exception ex)
            {
                Logger.WriteException($"Failed to fetch GitHub contributors: {ex.Message}");
                return new List<GitHubContributor>();
            }
        }

        private static async Task<List<GitHubContributor>> GetContributorsFromRepo(string apiUrl)
        {
            try
            {
                var response = await httpClient.GetStringAsync(apiUrl);
                var contributors = JsonSerializer.Deserialize<GitHubContributor[]>(response);
                return new List<GitHubContributor>(contributors ?? Array.Empty<GitHubContributor>());
            }
            catch (Exception ex)
            {
                Logger.WriteException($"Failed to fetch contributors from {apiUrl}: {ex.Message}");
                return new List<GitHubContributor>();
            }
        }
    }
}