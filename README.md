# ShowPicOnly

[![Latest release](https://img.shields.io/github/v/release/muhammetozeski/ShowPicOnly)](https://github.com/muhammetozeski/ShowPicOnly/releases/latest)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows%20x64-0078D4)
![UI](https://img.shields.io/badge/UI-Windows%20Forms-5C2D91)

ShowPicOnly shows one picture in a borderless window that stays above all other windows. The picture is what you
copied last: an image, an image file copied in File Explorer, or text, which is drawn as a picture. It keeps a
screenshot, a note or a reference image in view while you work in other programs.

## Screenshots

![Screenshot 1](https://i.ibb.co/C69f8Zc/image.png)

![Screenshot 2](https://i.ibb.co/gzLxxfT/image.png)

![Screenshot 3](https://i.ibb.co/ggXz8RX/image.png)

![Screenshot 4](https://i.ibb.co/qWD6rfC/image.png)

![Screenshot 5](https://i.ibb.co/z6NXGXX/image.png)

## Usage

Start `ShowPicOnly.exe`. The window opens at the right edge of the primary screen and shows the first of these that
exists:

1. the image file given on the command line, for example through **Open with** in File Explorer,
2. the clipboard content,
3. a short help text.

| Action | Result |
| --- | --- |
| Drag with the left button | Moves the window. |
| Drag the gray box in the bottom right corner | Resizes the window. Hold **Shift** to keep the aspect ratio of the picture. |
| Right click | Shows the current clipboard content. When the clipboard has nothing to show, the window returns to the original size of the picture. |
| Middle click | Closes the window. |
| Press the left and right buttons together | Removes the window from the taskbar and shows an icon in the notification area. Doing it again brings the window back to the taskbar. |
| Left click the notification area icon | Hides or shows the window. |
| Right click the notification area icon | Closes the program. |

The clipboard can hold:

- **an image**, which is shown at its original size,
- **files copied in File Explorer**, where the first file is shown when it is an image,
- **text**, which is drawn in gray bold Arial letters on a near-white background.

Pixels of exactly `#FFFAFF` (RGB 255, 250, 255) are see-through, because the window uses that color as its
transparency key. Every start opens a separate window, so several pictures can stay on screen at the same time.

## Security

- The program contains no network code and does not connect anywhere.
- It reads the clipboard only when it starts and when you right click the window.
- It opens only the file given on the command line or the first file copied to the clipboard.
- It writes only error reports next to the executable, and only when a picture cannot be loaded: `ShowPicOnly.log`,
  or `ShowPicOnly.<process id>.log` while another ShowPicOnly window is writing the first file.
- The release executables are signed, so you can check that they were not changed after the build.

## Installation

Download one of the executables from the [latest release](https://github.com/muhammetozeski/ShowPicOnly/releases/latest)
and put it in its own folder, since error reports are written next to it.

| File | Requirements |
| --- | --- |
| `ShowPicOnly.exe` | None. Native AOT build. |
| `ShowPicOnly-FrameworkDependent-RequiresNET10.exe` | [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) |

The executables are signed with a self-issued certificate; running `Install-Certificate.cmd` from the release's
`SignatureTrust.zip` once lets Windows verify that signature, and it does not remove the SmartScreen warning.

## Building

Requirements:

- .NET 10 SDK
- Visual Studio with the **Desktop development with C++** workload, for the Native AOT build only

```powershell
dotnet build -c Release
.\Publish.ps1
```

`Publish.ps1` writes both release executables to the `publish` folder. When `vswhere` does not list the Visual Studio
installation that has the C++ tools, pass its `vcvarsall.bat`:

```powershell
.\Publish.ps1 -VcVarsAllPath "C:\Path\To\Visual Studio\VC\Auxiliary\Build\vcvarsall.bat"
```

Windows Forms is not officially supported with trimming and Native AOT, so the project turns off the SDK error with
`_SuppressWinFormsTrimError`. The startup, clipboard and mouse handling code of this program was run in the Native AOT
build before the release. If the portable executable misbehaves, use the framework-dependent one.

## Startup time

Median time from process start until the window was shown and idle, measured on the author's computer on a hidden
desktop:

| Build | Time |
| --- | --- |
| Previous code, framework-dependent on .NET 10 | about 2.2 s |
| 1.1.0, framework-dependent | about 0.3 s |
| 1.1.0, portable (Native AOT) | about 0.15 s |

The previous code changed the window title while the control box was disabled, and Windows Forms destroys and
recreates the window for that change. It happened twice during every start.

## Project structure

| File | Contents |
| --- | --- |
| `Program.cs` | Entry point |
| `PictureWindow.cs` | The window: styles, mouse handling, notification area icon |
| `PictureLoader.cs` | Pictures from a file, the clipboard or text |
| `Palette.cs` | Colors |
| `ErrorLog.cs` | Error log next to the executable |
| `Publish.ps1` | Builds the release executables |
