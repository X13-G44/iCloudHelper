# iCloudHelper

I originally made this program for my mother because she was having trouble downloading and sorting photos from Apple iCloud into local directories.

## What is iCloudHelper

iCloudHelper is a free tool designed to simplify downloading images from Apple iCloud and organizing downloaded images into local directories.
The application offers a simple graphical user interface and is particularly suitable for users with little computer experience.
The program consists of two components, **AutoUnzip** and **QuickSort**, which fulfill this task.

## Features

- **AutoUnzip:** Automatically extracts photos from the downloaded iCloud `.zip` archive.
- **QuickSort:** Helps you quickly sort and organize the extracted photos into target folders.
- Intuitive graphical interface for easy operation.
- Installer included for a straightforward setup process.

## Who is it for?

iCloudHelper is intended for everyday users who want to simplify the process of handling and sorting large numbers of photos downloaded from iCloud, without requiring any technical knowledge.

## Requirements

- Microsoft Windows operating system Windows
- Microsoft .NET Framework 4.8 (or higher)
- Maybe the app "Apple-Devices" from Apple (It is need to show the HEIC image files).

## Installation

1. Download the installer from the [Releases](https://github.com/X13-G44/iCloudHelper/releases) section.
2. Launch the installer and follow the on-screen instructions to complete the installation.
3. Execute the installed app **AutoUnzip** for start monitoring and setup the configuration.

## Usage

### Main Workflow

1. **Download your photos from iCloud** using Apple's website.
2. **AutoUnzip** automatically extract your photos from the iCloud `.zip` file.
3. **Use QuickSort** to organize the extracted photos into folders of your choice.

---

### User Guide (To be completed)

_Add detailed step-by-step instructions for users here._

---

## Development status

There are still some points that have not been implemented or - in my opinion - still need improvement.

These are currently:
- [X] Slow loading and display of images in the UI (especially for HEIC)
- [X] Support for multiple languages ​​in the UI
- [X] I don't yet like the current layout and display design of the *Features*, *File Preview*, and *File Selection* areas
- [X] Display all dialog messages in the UI (with consistent design), instead of Windows `MessageBox.Show` dialogs
- [ ] Extension for HEIC image files to read the recording date.
- [ ] Automatically download image files from iCloud without using a web browser.

---

## License

This project is licensed under the Apache License v2.0.  
See the [LICENSE](LICENSE) file for details.

## Support

For questions or support, please open an issue on the [GitHub Issues page](https://github.com/X13-G44/iCloudHelper/issues).
