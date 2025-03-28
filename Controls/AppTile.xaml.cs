using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage.Streams;
using WinDurango.UI.Dialogs;
using WinDurango.UI.Pages;
using WinDurango.UI.Pages.Dialog;
using WinDurango.UI.Settings;
using WinDurango.UI.Utils;

namespace WinDurango.UI.Controls
{
    public sealed partial class AppTile
    {
        private Package _package;
        private readonly string _familyName;
        private string _Name;
        private string _Publisher;
        private string _Version;
        private Uri _Logo;
        
        private AppListEntry appListEntry;

        private async void HandleUnregister(object sender, SplitButtonClickEventArgs e)
        {
            if ((bool)unregisterCheckbox.IsChecked)
            {
                var confirmation =
                    new Confirmation(Localization.Locale.GetLocalizedText("/Packages/UninstallConfirmation", _Name),
                        "Uninstall?");
                Dialog.BtnClicked answer = await confirmation.Show();

                if (answer != Dialog.BtnClicked.Yes)
                    return;
                confirmation.Remove();
            }

            if ((bool)unpatchCheckbox.IsChecked && await WinDurangoPatcher.UnpatchPackage(_package, null))
            {
                await WinDurangoPatcher.UnpatchPackage(_package, null);
            }

            if ((bool)unregisterCheckbox.IsChecked)
            {
                var controller = new ProgressDialog($"Uninstalling {_Name}...", $"Uninstalling {_Name}", isIndeterminate: true).GetController();
                await controller.CreateAsync(async () =>
                {
                    await Packages.RemovePackage(_package, controller);
                });
                NoticeDialog good = new NoticeDialog($"{_Name} has been uninstalled.", "Uninstalled");
                await good.ShowAsync();
            }

            App.InstalledPackages.RemovePackage(_package);
            _ = App.MainWindow.AppsListPage.InitAppListAsync();
        }

        private void XboxInfo(object sender, RoutedEventArgs e)
        {
            //add a dialog that shows xbox info from the package
        }
        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            Logger.WriteDebug($"Opening app installation folder {_package.InstalledPath}");
            _ = Process.Start(new ProcessStartInfo(_package.InstalledPath) { UseShellExecute = true });
        }

        private async void ShowNotImplemented(object sender, RoutedEventArgs e)
        {
            Logger.WriteWarning($"Not implemented");
            NoticeDialog impl = new NoticeDialog($"This feature has not been implemented yet.", "Not Implemented");
            await impl.ShowAsync();
        }

        private async void ShowModManager(object sender, RoutedEventArgs e)
        {
            PageDialog pgd = new PageDialog(typeof(ModManPage), _package.InstalledPath, $"Installed mods for {_package.DisplayName}");
            pgd.XamlRoot = App.MainWindow.Content.XamlRoot;
            await pgd.ShowAsync();
        }

        private async void ShowSaveManager(object sender, RoutedEventArgs e)
        {
            PageDialog pgd = new PageDialog(typeof(SaveManagerPage), _familyName, $"{_package.DisplayName} saves");
            pgd.XamlRoot = App.MainWindow.Content.XamlRoot;
            await pgd.ShowAsync();
        }

        private async void RepatchPackage(object sender, RoutedEventArgs args)
        {
            var progress = new ProgressDialog($"Repatching {_Name}...", $"Repatching {_Name}", isIndeterminate: true).GetController();
            await progress.CreateAsync(async () =>
            {
                await WinDurangoPatcher.UnpatchPackage(_package, progress);
                await WinDurangoPatcher.PatchPackage(_package, true, progress);
            });
            NoticeDialog good = new NoticeDialog($"WinDurango was reinstalled in package {_Name}", "Reinstalled");
            await good.ShowAsync();

            App.MainWindow.ReloadAppList();
        }

        private async void UnpatchPackage(object sender, RoutedEventArgs args)
        {
            var progress = new ProgressDialog($"Unpatching {_Name}...", $"Unpatching {_Name}", isIndeterminate: true).GetController();
            await progress.CreateAsync(async () =>
            {
                await WinDurangoPatcher.UnpatchPackage(_package, progress);
            });
            if (!progress.Failed)
            {
                NoticeDialog good = new NoticeDialog($"WinDurango has been uninstalled from package {_Name}", "Uninstalled");
                await good.ShowAsync();
            }
            App.MainWindow.ReloadAppList();
        }

        // BUG: calling second time after failure does not show the failure dialog?
        private async void PatchPackage(object sender, RoutedEventArgs args)
        {
            var progress = new ProgressDialog($"Patching {_Name}...", $"Patching {_Name}", isIndeterminate: true).GetController();
            await progress.CreateAsync(async () =>
            {
                await WinDurangoPatcher.PatchPackage(_package, false, progress);
            });

            if (!progress.Failed)
            {
                NoticeDialog good = new NoticeDialog($"WinDurango has been installed in package {_Name}", "Installed");
                await good.ShowAsync();
            }
            App.MainWindow.ReloadAppList();
        }

        // TODO: This can probably be improved
        public AppTile(string familyName)
        {
            _familyName = familyName;
            this.InitializeComponent();

            _package = Packages.GetPackageByFamilyName(_familyName);
            try
            {
                _Name = _package.DisplayName ?? _package.Id.Name;
            }
            catch
            {
                _Name = _package.Id.Name;
            }
            _Publisher = _package.PublisherDisplayName ?? _package.Id.PublisherId;
            _Version = $"{_package.Id.Version.Major.ToString() ?? "U"}.{_package.Id.Version.Minor.ToString() ?? "U"}.{_package.Id.Version.Build.ToString() ?? "U"}.{_package.Id.Version.Revision.ToString() ?? "U"}";
            _Logo = _package.Logo;

            ManifestInfo mfInfo = _package.GetProperties();

            string ss = String.Empty;

            // TODO: This seems slow.
            if (!string.IsNullOrEmpty(mfInfo.SplashScreen))
            {
                ss = Path.Combine(_package.InstalledPath, mfInfo.SplashScreen);
                // if it doesn't exist it probably has some scale thing
                if (!File.Exists(ss))
                {
                    for (int i = 100; i < 400; i += 100)
                    {
                        string path = Path.Combine(_package.InstalledPath, Path.GetDirectoryName(mfInfo.SplashScreen), Path.GetFileNameWithoutExtension(mfInfo.SplashScreen) + $".scale-{i}.png");
                        if (File.Exists(path))
                        {
                            ss = path;
                            break;
                        }
                    }
                }
            } else if (!string.IsNullOrEmpty(mfInfo.WideLogo))
            {
                ss = Path.Combine(_package.InstalledPath, mfInfo.WideLogo);
                // if it doesn't exist it probably has some scale thing
                if (!File.Exists(ss))
                {
                    for (int i = 100; i < 400; i += 100)
                    {
                        string path = Path.Combine(_package.InstalledPath, Path.GetDirectoryName(mfInfo.WideLogo), Path.GetFileNameWithoutExtension(mfInfo.WideLogo) + $".scale-{i}.png");
                        if (File.Exists(path))
                        {
                            ss = path;
                            break;
                        }
                    }
                }
            }

            IReadOnlyList<AppListEntry> appListEntries = null;
            try
            {
                appListEntries = _package.GetAppListEntries();
            }
            catch
            {
                Logger.WriteWarning($"Could not get the applist entries of \"{_Name}\"");
            } 
            appListEntry = appListEntries?.FirstOrDefault() ?? null;

            if (appListEntry == null)
                Logger.WriteWarning($"Could not get the applist entry of \"{_Name}\"");

            if (String.IsNullOrEmpty(ss) || !File.Exists(ss))
            {
                try
                {
                    if (appListEntry != null)
                    {
                        RandomAccessStreamReference logoStream = appListEntry.DisplayInfo.GetLogo(new Size(320, 180));
                        BitmapImage logoImage = new();
                        using IRandomAccessStream stream = logoStream.OpenReadAsync().GetAwaiter().GetResult();
                        logoImage.SetSource(stream);
                        appLogo.Source = logoImage;
                    }
                    else
                    {
                        BitmapImage logoImage = new(_Logo);
                        appLogo.Source = logoImage;
                    }
                }
                catch (Exception)
                {
                    BitmapImage logoImage = new(_Logo);
                    appLogo.Source = logoImage;
                }
            }
            else
            {
                appLogo.Source = new BitmapImage(new Uri(ss));
            }
            infoExpander.Header = _Name;

            MenuFlyout rcFlyout = new();

            bool isPatched = false;

            installedPackage instPackage = App.InstalledPackages.GetPackage(_package.Id.FamilyName);
            if (instPackage != null)
                isPatched = instPackage.IsPatched;

            MenuFlyoutItem patchButton = new MenuFlyoutItem
            {
                Text = isPatched ? "Repatch" : "Patch",
                Name = "patchButton"
            };

            if (isPatched)
            {
                patchButton.Click += RepatchPackage;
                MenuFlyoutItem unpatchButton = new MenuFlyoutItem
                {
                    Text = "Unpatch",
                    Name = "unpatchButton"
                };
                unpatchButton.Click += UnpatchPackage;
                rcFlyout.Items.Add(unpatchButton);
            }
            else
            {
                patchButton.Click += PatchPackage;
            }

            rcFlyout.Items.Add(patchButton);

            expanderVersion.Text = $"Publisher: {_Publisher}\nVersion {_Version}";

            RightTapped += (sender, e) =>
            {
                rcFlyout.ShowAt(sender as FrameworkElement, e.GetPosition(sender as UIElement));
            };

            startButton.Tapped += (_, _) => StartApp();
        }

        public async void ShowControllerInteractDialog()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            dialog.Title = _package.DisplayName;
            StackPanel optionsPanel = new StackPanel();
            optionsPanel.HorizontalAlignment = HorizontalAlignment.Center;
            dialog.Content = optionsPanel;

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "NON-FUNCTIONAL";

            // todo: onclick for all

            // we need to either move the uninstall options somewhere else or figure out a way to make it work with controller
            Button uninstallButton = new Button();
            uninstallButton.Content = "Uninstall";
            uninstallButton.Margin = new Thickness(0, 0, 0, 10);

            Button manageSavesButton = new Button();
            manageSavesButton.Content = "Manage saves";
            manageSavesButton.Margin = new Thickness(0, 0, 0, 10);

            Button manageModsButton = new Button();
            manageModsButton.Content = "Manage mods";
            manageModsButton.Margin = new Thickness(0, 0, 0, 10);

            optionsPanel.Children.Add(textBlock);
            optionsPanel.Children.Add(uninstallButton);
            optionsPanel.Children.Add(manageSavesButton);
            optionsPanel.Children.Add(manageModsButton);

            await dialog.ShowAsync();
        }
        
            
        public async void StartApp()
        {
            if (_package.Status.LicenseIssue)
            {
                Logger.WriteError($"Could not launch {_Name} due to licensing issue.");
                _ = new NoticeDialog($"There is a licensing issue... Do you own this package?", $"Could not launch {_Name}").ShowAsync();
                return;
            }

            if (appListEntry == null)
            {
                _ = new NoticeDialog($"Could not get the applist entry of \"{_Name}\"", $"Could not launch {_Name}").ShowAsync();
                return;
            }
            Logger.WriteInformation($"Launching {_Name}");
            if (await appListEntry.LaunchAsync() == false)
                _ = new NoticeDialog($"Failed to launch \"{_Name}\"!", $"Could not launch {_Name}").ShowAsync();
        }
    }

}
