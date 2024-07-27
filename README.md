# Gitostory

![Gitostory Logo](Images/gitostory_logo.png)

## Overview

**Gitostory** is a Unity package that enables you to view and compare previous versions of your assets without altering the current state.

With Gitostory, you can:
- View the history of any asset directly within Unity.
- Compare previous versions of prefabs, scripts, and other assets without rolling back.
- Rollback to any previous version if needed.
- Gain insights and side by side comparison for prefabs.

## Prerequisites

- A Unity project with an initialized Git repository (`.git` folder present).
- Git installed on your system.

## Installation

1. **Download the Package**:
    - Download latest release from releases section or clone this repo.

2. **Add to Your Project**:
    - Open your Unity project.
    - Drag and drop the Gitostory package folder into your project's `Assets` folder.

3. **Configure Gitostory If Needed**:
    - Gitostory will most likely work without any configuration. If it doesn't, please check your project's Git path.
    - Open the Gitostory settings via `Window > Gitostory > Gitostory Settings`.
    - Set the `Temporary Root Folder` and `Repository Root Folder` as needed.

## Usage

### Viewing History

1. Right-click on an asset.
2. Select `Gitostory > Show History`.

![Show History](Images/gitostory_usg.png)

### Comparing Versions

1. In the history window, select a commit.
2. Click `Open in Prefab Comparison` or the appropriate comparison option.

![Comparison View](Images/gitostory_prefab.png) 

### Rolling Back

1. In the history window, select a commit.
2. Click `Rollback`.
3. Confirm the rollback in the prompt.


## Contributing

All pull requests makes this package better will be reviewed and accepted.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Support

For support and questions, just open a ticket.


## Dependencies

Gitostory is using [LibGit2Sharp](https://github.com/libgit2/libgit2sharp) library to provide Git functionalities within the Unity Editor. I'm grateful for the contributions of the LibGit2Sharp community. **LibGit2Sharp is used as-is**.

