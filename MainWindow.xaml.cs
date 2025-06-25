using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Pages;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;

namespace WinDurango.UI
{
    public sealed partial class MainWindow : Window
    {
        public readonly string AppName = "WinDurango";
        public static readonly UiConfig Settings = App.Settings;
        public AppsListPage AppsListPage;
        public SettingsPage SettingsPage;
        public AboutPage AboutPage;
        public AppMode currentMode;

        public enum AppMode
        {
            DESKTOP,
            CONTROLLER
        }

        private void NavigationInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                if (contentFrame.Content?.GetType() != typeof(SettingsPage))
                {
                    _ = contentFrame.Navigate(typeof(SettingsPage));
                }
            }

            else if (args.InvokedItemContainer is NavigationViewItem item)
            {
                string tag = item.Tag.ToString();
                
                if (tag == "CheckUpdates")
                {
                    _ = CheckForUpdatesAsync();
                    return;
                }
                
                Type pageType = tag switch
                {
                    "AppsListPage" => typeof(AppsListPage),
                    "AboutPage" => typeof(AboutPage),
                    "NotImplementedPage" => typeof(NotImplementedPage),
                    _ => typeof(NotImplementedPage)
                };

                if (contentFrame.Content?.GetType() != pageType && contentFrame.Navigate(pageType) && contentFrame.Content is AppsListPage appsList)
                    AppsListPage = appsList;
            }
        }

        public void ReloadAppList()
        {
            _ = AppsListPage?.InitAppListAsync();
        }

        public void LoadSettings()
        {
            ExtendsContentIntoTitleBar = Settings.Settings.Theme != UiConfigData.ThemeSetting.System;
            switch (Settings.Settings.Theme)
            {
                case UiConfigData.ThemeSetting.Mica:
                    this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.Base };
                    break;
                case UiConfigData.ThemeSetting.MicaAlt:
                    this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
                    break;
                case UiConfigData.ThemeSetting.Fluent:
                    this.SystemBackdrop = new DesktopAcrylicBackdrop();
                    break;
                case UiConfigData.ThemeSetting.FluentThin:
                    this.SystemBackdrop = new DesktopAcrylicBackdrop();
                    break;
                case UiConfigData.ThemeSetting.System:
                    this.SystemBackdrop = null;
                    break;
            }
        }

        private void OnNavigate(object sender, NavigatingCancelEventArgs e)
        {
            Logger.WriteDebug($"Switching to page {e.SourcePageType.Name}");
        }

        public MainWindow()
        {
            Closed += App.OnClosed;
            Title = AppName;
            AppWindow.SetIcon("ms-appx:///Assets/icon.ico");
            this.Activate();
            LoadSettings();

            this.InitializeComponent();
            contentFrame.Navigating += OnNavigate;

            contentFrame.Navigate(typeof(AppsListPage));
            AppsListPage = (AppsListPage)contentFrame.Content;
            
            // Controller init
            ControllerManager.Instance.Initialize(this);
            
            // Check for updates on startup
            _ = CheckForUpdatesAsync();
        }

        private async void appTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> missing = [];

            if (String.IsNullOrEmpty(FSHelper.FindFileOnPath("vcruntime140d.dll")))
                missing.Add("Microsoft Visual C++ Redistributable", null);


            if (App.Settings.Settings.ShowDevNotice)
            {
                var devNotice = new NoticeDialog("This UI is very early in development, and mainly developed by a C# learner... There WILL be bugs, and some things will NOT work...\n\nDevelopers, check Readme.md in the repo for the todolist.", "Important");
                await devNotice.ShowAsync();
                
                // We only show this notification once from now on
                Settings.Set("ShowDevNotice", false);
                Settings.Save();
            }

            
            if (missing.Count != 0)
            {
                // todo: properly provide download link
                string notice = $"You are missing the following dependencies, which may be required to run some packages.\n";
                foreach (KeyValuePair<string, string> ms in missing)
                {
                    notice += $"\n - {ms.Key}";
                }

                var missingNotice = new NoticeDialog(notice, "Missing dependencies");
                await missingNotice.ShowAsync();
            }

            if (ExtendsContentIntoTitleBar)
            {
                SetupTitleBar();
            }
        }

        private void appTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ExtendsContentIntoTitleBar)
            {
                SetupTitleBar();
            }
        }

        public void SwitchMode(AppMode mode)
        {
            currentMode = mode;
            navView.PaneDisplayMode = currentMode == AppMode.CONTROLLER ? NavigationViewPaneDisplayMode.Top : NavigationViewPaneDisplayMode.LeftCompact;
            controllerIndicator.Visibility = currentMode == AppMode.CONTROLLER ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetupTitleBar()
        {
            AppWindowTitleBar titleBar = AppWindow.TitleBar;
            double scaleAdjustment = appTitleBar.XamlRoot.RasterizationScale;
            rightPaddingColumn.Width = new GridLength(titleBar.RightInset / scaleAdjustment);
            leftPaddingColumn.Width = new GridLength(titleBar.LeftInset / scaleAdjustment);
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var release = await UpdateManager.CheckForUpdatesAsync();
                if (release != null)
                {
                    var dialog = new Confirmation(
                        $"Update {release.tag_name} is available!\n\n{release.body}\n\nWould you like to download and install it?",
                        "Update Available");
                    
                    if (await dialog.Show() == Dialog.BtnClicked.Yes)
                    {
                        await UpdateManager.DownloadAndInstallUpdateAsync(release);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException($"Update check failed: {ex.Message}");
            }
        }
    }
}
