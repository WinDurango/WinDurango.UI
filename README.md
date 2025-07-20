# WinDurango.UI
[![Join our Discord](https://img.shields.io/discord/1280176159010848790?color=2c9510&label=WinDurango%20Discord&logo=Discord&logoColor=white)](https://discord.gg/mHN2BgH7MR)
[![View stargazers](https://img.shields.io/github/stars/WinDurango-project/WinDurango.UI)](https://github.com/WinDurango-project/WinDurango.UI/stargazers)   
GUI for WinDurango, which allows you to install the compatibility layer into packages, as well as manage mods and saves. (+ more coming later probs)

You can get the latest builds from [GitHub Actions](https://github.com/WinDurango/WinDurango.UI/actions).   
For building you'll need Visual Studio 2022 with the following:
- .NET desktop development
- Windows application development

# Roadmap

## Features
 - [X] Patching
 - [X] Allow for package removal from UI instead of just completely uninstalling.
 - [X] Installation options
 - [X] Save Manager
 - [X] Mod Manager
 - [X] Scan for already installed EraOS/XUWP stuff
 - [X] Allow for any existing installed package to be added to the applist
 - [X] Built in updater 
 - [X] Controller support 
 - [ ] Setup program (maybe MSIX?)
 - [ ] Fitting place for extra xbox-specific info
 - [ ] Resize content to fit to screen
 - [X] Allow for search

## Bugs/Improvements
 - [X] Make the applist not go offscreen (lol)
 - [X] Fix UI load speed when loading a lot of packages on startup
 - [X] Applist scrolling
 - [X] Fix icon in the titlebar
 - [X] Repo contributors on the about screen
 - [ ] Get Fluent Thin working
 - [ ] Use InfoBar for many progress related things (such as registering package, etc) 
 - [ ] Add versioning to the InstalledPackages json (as in versioning the JSON file itself)
 - [ ] Make the Package stuff not rely on UI so much, handle that somewhere else.
 - [X] Fix crash when installation errors
 - [ ] Cleanup, lots and lots of it.
