# Cub

**Cub by NeonBear** - a Windows 11 debloater with a switch for every action. One self-contained app with its own icon, no terminal window, no .NET install needed on the target PC.

Current version: **2.0**

> **Status:** I (the AI that wrote this) could not run it on Windows, so treat new features as untested until you have tried them. Run it on a spare PC or make a backup first.

---

## Install

### Option 1 - one PowerShell command

Open **PowerShell** (right-click Start > Terminal) and paste:

```powershell
$msi = "$env:TEMP\Cub.msi"; Invoke-WebRequest "https://github.com/Crashed-Again/cub-debloater-for-win11/releases/latest/download/Cub.msi" -OutFile $msi; Start-Process msiexec.exe -ArgumentList "/i `"$msi`" /passive" -Verb RunAs -Wait
```

It downloads the newest installer from this repo's Releases page and installs it (Windows will ask for admin permission). Afterwards open **Cub** from the Start menu or the desktop shortcut.

To remove it later: Settings > Apps > Installed apps > Cub > Uninstall, or run:

```powershell
winget uninstall Cub
```

(If winget does not find it, use Settings > Apps.)

> The command needs a published release. Push a version tag (for example `git tag v2.0.0 && git push origin v2.0.0`) and the included GitHub Action builds `Cub.msi` and attaches it to a release automatically. See "Publishing a release" below.

### Option 2 - download the installer yourself

Go to the repo's **Releases** page and download `Cub.msi`, then double-click it.

### Option 3 - build it yourself

See "Build" below.

---

## What's new in 2.0

- **New design** inspired by the tPay dashboard: floating rounded sidebar with icons, a top bar with a search box, one setting per line, lime-green accents.
- **Slightly transparent window:** a little of your desktop shows through Cub. Switch it off with **Slightly transparent window** on the Appearance page. The title bar and window border take on the app's own dark colour (Windows 11), so there is no grey frame.
- **Rounded everything:** dropdowns and text boxes are now rounded and dark like the rest of the app (no more square white boxes).
- **Search** at the top finds any setting across all pages.
- **Wallpaper picker:** 11 named wallpapers in a scrolling strip. The default is **Midnight Bloom**. You can still use your own image. The old wallpapers were removed.
- **Fixed the ms-gamingoverlay popup:** Cub now registers do-nothing handlers for the Game Bar links, so Windows stops asking for an app.
- **Drivers and runtimes page:**
  - *Install missing runtimes* (on by default): Visual C++ 2015-2022 / 2013 / 2012 / 2010, .NET Desktop Runtime 8 and 10, DirectX End-User Runtime, WebView2. Each can be switched off, and only runtimes that are missing get installed.
  - *Install and update drivers* (on by default): downloads missing and newer drivers from Windows Update.
- Version number is **2.0**.

---

## Build

1. Install the free [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Double-click `build.bat` (or run `dotnet publish -c Release -o out`).
3. Your app is at `out\Cub.exe`. Windows shows it as **Cub** (company **NeonBear**) with the bear icon.

The app asks for administrator rights when it opens.

### Build the installer (.msi)

1. Install the .NET 8 SDK.
2. Double-click `build-msi.bat`. It publishes the app, installs the WiX v5 tool if needed (`dotnet tool install --global wix --version 5.0.2`) and builds the installer.
3. Your installer is at `out\Cub-2.0.0.msi`.

The installer puts the app in `C:\Program Files\Cub`, adds Start menu and desktop shortcuts, and registers Cub under Settings > Apps. Installing a newer .msi upgrades the old version in place.

### Publishing a release

The workflow in `.github/workflows/release.yml` builds the installer on GitHub:

1. Commit and push the code.
2. Create and push a tag: `git tag v2.0.0` then `git push origin v2.0.0`.
3. Wait for the **Build and release** action to finish. A release appears with `Cub.msi` (fixed name, used by the PowerShell command) and `Cub-2.0.0.msi`.

You can also run it by hand from the **Actions** tab; the installer is then available as a downloadable artifact.

> Tip: if `bin`, `obj` or `out` folders were committed before, remove them from the repo once with `git rm -r --cached bin obj out`. The included `.gitignore` keeps them out from now on.

### Changing the version number

Edit **one line** in `Cub.csproj`:

```xml
<Version>2.0.0</Version>
```

The sidebar, file Properties and the .msi name follow it (2.0.0 is shown as "2.0" in the app). Use three numbers, like `2.1.0`.

---

## Using it

1. Pick a page on the left, or type in the search box at the top to find a setting. Flip the switches you want. Anything switched off is left alone.
2. Optionally pick a **preset** or load a saved **profile** at the bottom.
3. Press **Apply changes**. A confirmation lists any risky steps you turned on.
4. Watch the progress bar and live log on the **Apply** page, then restart your PC.

## Pages

| Page | What you can control |
|---|---|
| **Appearance** | Dark mode, slightly transparent window, taskbar icons left, Explorer opens to This PC, wallpaper picker (11 built-in or your own file), mouse acceleration off, Sticky Keys off, show file extensions (on by default), classic right-click menu (on by default) |
| **AI** | Remove Copilot, disable Recall and Click to Do, no web results in Start search |
| **Apps** | Master switch plus one switch per built-in app (about 35), and a box to add your own package name (wildcards like `*Spotify*` work) |
| **Start menu** | Hide Recommended, grid view for All apps, and which apps stay pinned |
| **Privacy and security** | Telemetry, location tracking, Xbox Game Bar / ms-gamingoverlay popup (on by default), Smart App Control, BitLocker, hibernation |
| **Performance** | Ultimate Performance power plan |
| **Software** | QuickLook (Space to preview a file), browser picker (Google Chrome by default), optional browser update, remove Edge, a winget app search, and the list of apps to install |
| **Drivers and runtimes** | Install missing runtimes, install and update drivers |
| **Apply** | Restore point switch, progress bar, live log, Restart PC button |

## Wallpapers

Pick one in the strip on the Appearance page (the **Set wallpaper** switch must be on):

Midnight Bloom (default), Azure Dark Bloom, Coral Dark Bloom, Amber Dark Bloom, Frost Bloom, Cobalt Bloom, Emerald Bloom, Crimson Bloom, Dusk Valley, Night Valley, Moonlit Hills.

Use **Use my own image...** for any PNG or JPG. The chosen image is copied to your Pictures folder and applied with the Fill style.

## Presets and profiles

- **Recommended** - the default set of options.
- **Light touch** - appearance and AI tweaks only.
- **Aggressive** - every switch on, including Photos, Paint, Snipping Tool, Terminal and every runtime.
- **Nothing** - clears every switch.

**Save profile / Load profile** store your whole setup as a `.json` file. Your last choices are also remembered automatically in `%AppData%\Cub\last-profile.json`.

## Finding apps to install

On the **Software** page, type a name in the search box and press Search (or Enter). Results come from `winget search`. Double-click a result, or select one or more and press **Add selected**, to put its ID in the install list below. You can also type or delete IDs in that list by hand. Everything in the list is installed or updated when you press Apply.

## Browser step

The browser step always runs **last**. Google Chrome is selected by default; change it in the dropdown.

1. The browser you picked is installed through winget. If it is already installed it is updated, unless you switch off **Update the browser if it is already installed**.
2. Only if that succeeded, and **Remove Edge** is on, Edge is uninstalled. This is so you are never left without a browser.
3. Choose **Keep Edge** to just update it, or **Don't change my browser** to skip the step.

The Edge WebView2 runtime is kept because many apps depend on it.

## Drivers and runtimes

- **Runtimes** are checked with `winget list` first. Anything already installed is skipped.
- **Drivers** come from Windows Update (the same source as Settings > Windows Update > Advanced options > Optional updates). The scan and install can take several minutes, and some drivers need a restart. Graphics-vendor tools such as the NVIDIA or AMD apps are not installed automatically; add them through the winget search if you want them.

## What it never touches

Calculator, Clock, Notepad and the Microsoft Store are not in the removal list. Photos, Paint, Snipping Tool and Windows Terminal are listed but off by default.

## Things to know before you apply

- **Look:** the matching title bar colour needs Windows 11. Windows cannot blur the inside of a normal desktop app without making its text and controls look washed out, so Cub uses plain transparency instead of blur. The UI uses Poppins if it is installed, otherwise Segoe UI.
- **Default browser:** Windows does not let programs set the default browser silently. Cub installs the browser; you confirm it in Settings > Apps > Default apps.
- **ms-gamingoverlay popup:** Cub turns Game Bar off and registers do-nothing handlers for `ms-gamingoverlay`, `ms-gamebar` and `ms-gamebarservices`. Sign out and back in if the popup still shows once.
- **BitLocker:** drives get decrypted. This can take a long time and runs in the background.
- **Smart App Control:** once off, it cannot be turned back on without resetting Windows.
- **Hibernation:** turning it off also turns off Fast Startup.
- **Telemetry:** Home and Pro editions still send a minimal "required" level. That is a Windows limit.
- **Start menu:** hiding Recommended and the custom pins use policy settings that depend on your Windows edition and build. Some builds ignore them.
- **Edge removal:** Microsoft sometimes blocks the uninstall. If so, the log says so and Edge stays.
- **Restore point:** one is created first by default. Windows can still refuse, for example if System Protection is off.
- **Run as the right user:** most settings apply to the account that launches the app, so run it from the account you actually use.

## Undoing things

Use the restore point for most changes. Removed apps can be reinstalled from the Microsoft Store. Individual settings can be flipped back in Windows Settings.

## Project layout

| File | Purpose |
|---|---|
| `Cub.csproj` | Project settings: name, icon, company, **version** |
| `Program.cs` | Entry point and admin check |
| `MainForm.cs` | The window: sidebar, top bar, pages, search, presets, profiles |
| `Ui.cs` | Theme, title bar colours and custom controls (toggle, cards, chips, icons, wallpaper tiles) |
| `Engine.cs` | The code that actually applies each tweak |
| `Wallpapers.cs` | The list of built-in wallpapers and their names |
| `WingetSearch.cs` | Runs `winget search` and reads the results |
| `Wallpapers\full`, `Wallpapers\thumbs` | The wallpaper images (embedded into the exe) |
| `app.manifest` | Asks Windows for administrator rights |
| `icon.ico` | The Cub icon |
| `build.bat` | One-click build of the .exe |
| `build-msi.bat` | Builds the .exe and the installable .msi |
| `installer\Installer.wxs` | WiX definition of the installer |
| `.github\workflows\release.yml` | Builds the installer on GitHub and attaches it to a release |

## Troubleshooting

- **Build fails:** make sure the .NET 8 SDK is installed, then paste the error message so it can be fixed.
- **The PowerShell command says 404 / not found:** no release exists yet. Push a version tag (see "Publishing a release") or download the installer from the Actions tab.
- **A setting did not change:** some need a restart or sign-out. Check the log for lines starting with `!`.
- **winget not found:** install or update "App Installer" from the Microsoft Store. QuickLook, browsers, runtimes and extra apps need it.

Use at your own risk.

---

Cub by NeonBear. Copyright (c) 2026 NeonBear.
