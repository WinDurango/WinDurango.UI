using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;


namespace WinDurango.UI.Pages.Settings
{
    public sealed partial class UiSettings : Page
    {
        public UiSettings()
        {
            this.InitializeComponent();

            ComboBoxItem psbSelected = PatchSourceButton.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag.ToString() == App.Settings.Settings.DownloadSource.ToString());
            if (psbSelected != null && (ComboBoxItem)PatchSourceButton.SelectedItem != psbSelected)
            {
                PatchSourceButton.SelectedItem = psbSelected;
            }

            ComboBoxItem themeSelected = themeButton.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag.ToString() == App.Settings.Settings.Theme.ToString());
            if (themeSelected != null && (ComboBoxItem)themeButton.SelectedItem != themeSelected)
            {
                themeButton.SelectedItem = themeSelected;
            }

            HorizontalScrollingToggle.IsOn = App.Settings.Settings.AppViewIsHorizontalScrolling;
        }

        private void OnThemeSelected(object sender, RoutedEventArgs e)
        {
            if (themeButton.SelectedItem is not ComboBoxItem sel)
            {
                return;
            }

            if (!Enum.TryParse(sel.Tag.ToString(), out UiConfigData.ThemeSetting theme))
            {
                return;
            }

            if (App.Settings.Settings.Theme == theme)
            {
                return;
            }

            App.Settings.Set("Theme", theme);
        }

        private async void OnSourceSelected(object sender, RoutedEventArgs e)
        {
            if (PatchSourceButton.SelectedItem is not ComboBoxItem sel)
            {
                return;
            }

            if (!Enum.TryParse(sel.Tag.ToString(), out UiConfigData.PatchSource source))
            {
                return;
            }

            if (App.Settings.Settings.DownloadSource == source)
            {
                return;
            }

            PatchSourceButton.SelectedItem = sel.Content;
            App.Settings.Set("DownloadSource", source);
        }

        private async void OnDebugLogToggled(object sender, RoutedEventArgs e)
        {
            App.Settings.Set("DebugLoggingEnabled", ((ToggleSwitch)sender).IsOn);
        }

        private void OpenAppData(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(App.DataDir) { UseShellExecute = true });
        }

        public void OnToggleSetting(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch && toggleSwitch.Tag is string settingName)
            {
                App.Settings.Set(settingName, toggleSwitch.IsOn);
            }
        }

        private void OnDebugLogToggleLoaded(object sender, RoutedEventArgs e)
        {
            if (!Debugger.IsAttached || ((ToggleSwitch)sender).IsEnabled)
            {
                return;
            }

            ((ToggleSwitch)sender).IsEnabled = false;
            ((ToggleSwitch)sender).IsOn = true;
            ((ToggleSwitch)sender).OnContent = "Enable debug logging (currently debugging)";
        }

        private void HorizontalScrollingToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                App.Settings.Set("AppViewIsHorizontalScrolling", toggleSwitch.IsOn);
                App.MainWindow.AppsListPage.SwitchScrollDirection(toggleSwitch.IsOn);
            }
        }

        private async void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            try
            {
                var release = await UpdateManager.CheckForUpdatesAsync();
                if (release != null)
                {
                    var dialog = new Confirmation(
                        $"Update {release.tag_name} is available!\n\n{release.body}\n\nWould you like to download and install it?",
                        "Update Available");
                    
                    if (await dialog.Show() == Dialogs.Dialog.BtnClicked.Yes)
                    {
                        await UpdateManager.DownloadAndInstallUpdateAsync(release);
                    }
                }
                else
                {
                    var noUpdateDialog = new NoticeDialog("You are already running the latest version.", "No Updates Available");
                    await noUpdateDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var errorDialog = new NoticeDialog($"Failed to check for updates: {ex.Message}", "Update Check Failed");
                await errorDialog.ShowAsync();
            }
        }
    }
}
