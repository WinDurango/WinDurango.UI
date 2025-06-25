using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;
using WinDurango.UI.Utils;

// Controller Support is broken, need to figure out a fix

namespace WinDurango.UI.Utils
{
    public class ControllerManager
    {
        private static ControllerManager _instance;
        public static ControllerManager Instance => _instance ??= new ControllerManager();

        private Gamepad _currentGamepad;
        private GamepadReading _previousReading;
        private readonly DispatcherTimer _pollTimer;
        private MainWindow _mainWindow;
        private int _selectedIndex = 0;
        private List<Control> _navigableElements = new();

        public bool IsControllerMode { get; private set; }

        private ControllerManager()
        {
            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _pollTimer.Tick += PollController;
            
            Gamepad.GamepadAdded += OnGamepadAdded;
            Gamepad.GamepadRemoved += OnGamepadRemoved;
        }

        public void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            CheckForGamepads();
        }

        private void CheckForGamepads()
        {
            var gamepads = Gamepad.Gamepads;
            if (gamepads.Count > 0)
            {
                _currentGamepad = gamepads[0];
                EnableControllerMode();
            }
        }

        private void OnGamepadAdded(object sender, Gamepad gamepad)
        {
            if (_currentGamepad == null)
            {
                _currentGamepad = gamepad;
                EnableControllerMode();
            }
        }

        private void OnGamepadRemoved(object sender, Gamepad gamepad)
        {
            if (_currentGamepad == gamepad)
            {
                _currentGamepad = Gamepad.Gamepads.FirstOrDefault();
                if (_currentGamepad == null)
                {
                    DisableControllerMode();
                }
            }
        }

        private void EnableControllerMode()
        {
            IsControllerMode = true;
            _mainWindow?.SwitchMode(MainWindow.AppMode.CONTROLLER);
            _pollTimer.Start();
            Logger.WriteInformation("Controller mode enabled");
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
            if (_currentGamepad == null) return;

            var reading = _currentGamepad.GetCurrentReading();
            
            // Check for button presses (only trigger on press, not hold)
            if (WasButtonPressed(GamepadButtons.DPadUp, reading))
                NavigateUp();
            else if (WasButtonPressed(GamepadButtons.DPadDown, reading))
                NavigateDown();
            else if (WasButtonPressed(GamepadButtons.DPadLeft, reading))
                NavigateLeft();
            else if (WasButtonPressed(GamepadButtons.DPadRight, reading))
                NavigateRight();
            else if (WasButtonPressed(GamepadButtons.A, reading))
                ActivateSelected();
            else if (WasButtonPressed(GamepadButtons.B, reading))
                GoBack();

            _previousReading = reading;
        }

        private bool WasButtonPressed(GamepadButtons button, GamepadReading currentReading)
        {
            return (currentReading.Buttons & button) != 0 && 
                   (_previousReading.Buttons & button) == 0;
        }

        private void NavigateUp()
        {

        }

        private void NavigateDown()
        {
          
        }

        private void NavigateLeft()
        {
       
        }

        private void NavigateRight()
        {

        }

        private void ActivateSelected()
        {

        }

        private void GoBack()
        {

        }

        private void UpdateNavigableElements()
        {
            _navigableElements.Clear();

        }

        private void FocusElement(Control element)
        {
            element.Focus(FocusState.Programmatic);
        }
    }
}

// TODO: add proper functionality for controller navigation, it's just very basic right now