using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;
using WinDurango.UI.Controls;
using WinDurango.UI.Utils;


namespace WinDurango.UI.Pages
{
    public sealed partial class AboutPage : Page
    {

        public AboutPage()
        {
            this.InitializeComponent();
            _ = LoadContributorsAsync();
        }

        private async Task LoadContributorsAsync()
        {
            try
            {
                // Load Locally saved contributors first
                LoadLocalContributors();
                
                // Now load from Github
                var githubContributors = await GitHubContributorManager.GetUIContributorsAsync();
                if (githubContributors.Count > 0)
                {
                    contributorList.Children.Clear();
                    foreach (var contributor in githubContributors)
                    {
                        if (contributor.type != "Bot") // Skip bot accounts
                        {
                            contributorList.Children.Add(new ContributorInfo(
                                contributor.login, 
                                contributor.avatar_url, 
                                contributor.html_url));
                        }
                    }
                }
                
                loadingText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Logger.WriteException($"Failed to load GitHub contributors: {ex.Message}");
                loadingText.Text = "Failed to load GitHub contributors, showing local data";
            }
        }

        private void LoadLocalContributors()
        {
            if (File.Exists("Assets/contributors.txt"))
            {
                string[] lines = File.ReadAllLines("Assets/contributors.txt");
                foreach (var contributor in lines)
                {
                    string[] info = contributor.Split(";");
                    string name = info[0].Replace("WD_CONTRIB_SEMICOLON", ";");
                    string avatar = info[1].Replace("WD_CONTRIB_SEMICOLON", ";");
                    string link = info[2].Replace("WD_CONTRIB_SEMICOLON", ";");

                    contributorList.Children.Add(new ContributorInfo(name, avatar, link));
                }
            }
        }
    }
}
