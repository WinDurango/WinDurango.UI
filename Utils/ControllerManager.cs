using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.XInput;
using WinDurango.UI.Utils;

namespace WinDurango.UI.Utils
{
    public class ControllerManager
    {
        private static ControllerManager _instance;
        public static ControllerManager Instance => _instance ??= new ControllerManager();

        private Controller _controller;
        private State _previousState;
        private readonly DispatcherTimer _pollTimer;
        private MainWindow _mainWindow;
        private int _selectedIndex = 0;
        private List<Control> _navigableElements = new();

        public bool IsControllerMode { get; private set; }

        private ControllerManager()
        {
            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _pollTimer.Tick += PollController;
        }

        public void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            CheckForGamepads();
        }

        private void CheckForGamepads()
        {
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    var controller = new Controller((UserIndex)i);
                    if (controller.IsConnected)
                    {
                        _controller = controller;
                        Logger.WriteInformation($"XInput controller found at index {i}");
                        EnableControllerMode();
                        return;
                    }
                }
                Logger.WriteInformation("No XInput controllers found");
            }
            catch (Exception ex)
            {
                Logger.WriteException($"XInput detection failed: {ex.Message}");
            }
        }

        private void EnableControllerMode()
        {
            IsControllerMode = true;
            _mainWindow?.SwitchMode(MainWindow.AppMode.CONTROLLER);
            _pollTimer.Start();
            Logger.WriteInformation($"Controller mode ENABLED - Timer running: {_pollTimer.IsEnabled}");
            
            try
            {
                var testState = _controller.GetState();
                Logger.WriteInformation($"XInput test successful - PacketNumber: {testState.PacketNumber}");
            }
            catch (Exception ex)
            {
                Logger.WriteException($"XInput test failed: {ex.Message}");
            }
        }

        private void DisableControllerMode()
        {
            IsControllerMode = false;
            _mainWindow?.SwitchMode(MainWindow.AppMode.DESKTOP);
            _pollTimer.Stop();
            Logger.WriteInformation("Controller mode disabled");
        }

        private void PollController(object sender, object e)
        {
            if (_controller == null || !_controller.IsConnected)
            {
                CheckForGamepads();
                return;
            }

            try
            {
                var state = _controller.GetState();
                var gamepad = state.Gamepad;
                
                if (WasButtonPressed(GamepadButtonFlags.DPadUp, gamepad.Buttons, _previousState.Gamepad.Buttons))
                {
                    Logger.WriteInformation("D-Pad UP");
                    NavigateUp();
                }
                else if (WasButtonPressed(GamepadButtonFlags.DPadDown, gamepad.Buttons, _previousState.Gamepad.Buttons))
                {
                    Logger.WriteInformation("D-Pad DOWN");
                    NavigateDown();
                }
                else if (WasButtonPressed(GamepadButtonFlags.A, gamepad.Buttons, _previousState.Gamepad.Buttons))
                {
                    Logger.WriteInformation("A button");
                    ActivateSelected();
                }
                else if (WasButtonPressed(GamepadButtonFlags.B, gamepad.Buttons, _previousState.Gamepad.Buttons))
                {
                    Logger.WriteInformation("B button");
                    GoBack();
                }
                else if (WasButtonPressed(GamepadButtonFlags.Start, gamepad.Buttons, _previousState.Gamepad.Buttons))
                {
                    Logger.WriteInformation("Menu button");
                    OpenMenu();
                }

                _previousState = state;
            }
            catch (Exception ex)
            {
                Logger.WriteException($"XInput polling error: {ex.Message}");
                _controller = null;
            }
        }

        private bool WasButtonPressed(GamepadButtonFlags button, GamepadButtonFlags current, GamepadButtonFlags previous)
        {
            return (current & button) != 0 && (previous & button) == 0;
        }

        private void NavigateUp()
        {
            UpdateNavigableElements();
            if (_navigableElements.Count > 0)
            {
                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                FocusElement(_navigableElements[_selectedIndex]);
            }
        }

        private void NavigateDown()
        {
            UpdateNavigableElements();
            if (_navigableElements.Count > 0)
            {
                _selectedIndex = Math.Min(_navigableElements.Count - 1, _selectedIndex + 1);
                FocusElement(_navigableElements[_selectedIndex]);
            }
        }

        private void ActivateSelected()
        {
            if (_navigableElements.Count > 0 && _selectedIndex < _navigableElements.Count)
            {
                var element = _navigableElements[_selectedIndex];
                if (element is Button button)
                {
                    // Simulate click event
                    button.Command?.Execute(button.CommandParameter);
                }
                else if (element is ToggleSwitch toggle)
                {
                    toggle.IsOn = !toggle.IsOn;
                }
            }
        }

        private void GoBack()
        {
            if (_mainWindow?.contentFrame?.CanGoBack == true)
            {
                _mainWindow.contentFrame.GoBack();
            }
        }

        private void OpenMenu()
        {
            if (_mainWindow?.navView != null)
            {
                _mainWindow.navView.IsPaneOpen = !_mainWindow.navView.IsPaneOpen;
            }
        }

        private void UpdateNavigableElements()
        {
            _navigableElements.Clear();
            if (_mainWindow?.contentFrame?.Content is FrameworkElement page)
            {
                FindNavigableElements(page);
            }
        }

        private void FindNavigableElements(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is Control control && control.IsTabStop && control.Visibility == Visibility.Visible)
                {
                    _navigableElements.Add(control);
                }
                
                FindNavigableElements(child);
            }
        }

        private void FocusElement(Control element)
        {
            element.Focus(FocusState.Programmatic);
        }
    }
}