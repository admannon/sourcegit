# Per-Repository Git Executable Configuration

This document explains how to configure different Git executables for different repositories in SourceGit.

## Overview

While SourceGit allows you to configure a global Git executable (or enable WSL Git globally), you can also override this setting on a per-repository basis. This is useful when:

- You have repositories in both Windows and WSL filesystems
- Different repositories require different WSL distributions
- You need to use different Git versions for different projects
- Testing or developing with multiple Git installations

## Configuring Repository-Specific Git Executable

### Via UI

1. Open the repository in SourceGit
2. Click the **⚙️ Configure** button in the toolbar (or go to Repository → Configure)
3. In the **GIT** tab, find the **"Git Executable (Override)"** field
4. Either:
   - Type the path to the git executable directly
   - Click the folder icon to browse for the executable
   - Leave empty to use the global setting
5. The effective git executable being used is shown in gray text to the right
6. Click **OK** to save

### Path Formats

#### Windows Native Git
```
C:\Program Files\Git\bin\git.exe
```

#### WSL Git
Use the special WSL format:
```
wsl:Ubuntu:/usr/bin/git
wsl:Ubuntu-22.04:/usr/bin/git
wsl:Debian:/usr/bin/git
```

Format: `wsl:<distribution>:<path-in-wsl>`

#### macOS/Linux
```
/usr/bin/git
/usr/local/bin/git
/opt/homebrew/bin/git
```

## Use Cases

### Mixed Windows/WSL Repositories

**Global Setting:** Windows Git  
**Repository in WSL:** Override with `wsl:Ubuntu:/usr/bin/git`

This allows you to work with both Windows-based and WSL-based repositories from the same SourceGit instance.

### Multiple WSL Distributions

**Repository 1 (Ubuntu):** `wsl:Ubuntu:/usr/bin/git`  
**Repository 2 (Debian):** `wsl:Debian:/usr/bin/git`

Each repository uses Git from its native WSL distribution.

### Testing Different Git Versions

**Stable Projects:** Use global git (e.g., Git 2.43)  
**Test Repository:** Override with path to Git 2.45 beta

### Development Setup

**Production Repos:** Global Git with standard configuration  
**Development Repos:** Override with custom-built Git for testing patches

## How It Works

When a repository has a Git executable override configured:

1. SourceGit stores the override path in `.git/sourcegit.settings` (JSON file)
2. When executing Git commands for that repository, the override is used instead of the global setting
3. If the override is empty or invalid, SourceGit falls back to the global setting
4. Path conversion and WSL integration work automatically based on the configured path

## Priority Order

For each repository, SourceGit determines which Git executable to use in this order:

1. **Repository Override** (if configured and valid)
2. **Global Setting** (from Preferences → GIT → Install Path)
3. **Auto-Detection** (searches system PATH and registry)

## Configuration File

The repository-specific configuration is stored in:
```
<repository>/.git/sourcegit.settings
```

Example content:
```json
{
  "GitExecutableOverride": "wsl:Ubuntu:/usr/bin/git",
  "DefaultRemote": "origin",
  ...
}
```

## Clearing Override

To remove a repository-specific override and return to using the global setting:

1. Open Repository → Configure
2. Clear the **"Git Executable (Override)"** field
3. Click **OK**

The repository will now use the global Git executable setting.

## Troubleshooting

### Override Not Taking Effect

**Symptoms:** Commands still use global Git even with override set

**Solutions:**
- Verify the override path is correct and the file exists
- Check that the path format is correct (especially for WSL: `wsl:distro:path`)
- Try closing and reopening the repository
- Check the effective git executable shown in gray text

### Invalid Path Error

**Symptoms:** Git commands fail after setting override

**Solutions:**
- Verify the git executable path is correct
- For WSL paths, ensure the distribution name is correct: `wsl --list`
- For WSL paths, verify git is installed in that location: `wsl -d Ubuntu which git`
- Clear the override to return to global setting

### WSL Path Not Working

**Symptoms:** WSL override configured but commands fail

**Solutions:**
- Ensure WSL is installed and accessible: `wsl --status`
- Verify the distribution exists: `wsl --list`
- Verify git is installed in WSL: `wsl -d <distro> git --version`
- Use the correct WSL format: `wsl:distro:/path`

## Best Practices

1. **Use Global Setting When Possible**: Only override when necessary for specific repositories
2. **Document Custom Settings**: Add a note in your repository's README if it requires a specific Git version
3. **Test After Configuration**: Run a simple git command (like `git status`) to verify the override works
4. **Backup Settings**: The `.git/sourcegit.settings` file contains all repository-specific preferences
5. **Team Consideration**: Remember that repository overrides are local to your machine and not shared with the team

## Examples

### Example 1: WSL Repository with Windows SourceGit

```
Global: C:\Program Files\Git\bin\git.exe
Repository: \\wsl.localhost\Ubuntu\home\user\myproject
Override: wsl:Ubuntu:/usr/bin/git
```

This repository is stored in WSL and uses WSL's git, while other repositories use Windows git.

### Example 2: Testing Beta Git Version

```
Global: C:\Program Files\Git\bin\git.exe
Test Repo Override: C:\Git-Beta\bin\git.exe
```

Test repository uses a beta version while production repositories use stable git.

### Example 3: Multiple WSL Distributions

```
Global: (empty, auto-detect)
Ubuntu Repo: wsl:Ubuntu:/usr/bin/git
Debian Repo: wsl:Debian:/usr/bin/git
Fedora Repo: wsl:Fedora:/usr/bin/git
```

Each WSL repository uses git from its own distribution.

---

**Note**: Repository-specific Git executable overrides are stored locally and do not affect other users or machines. This feature is designed for flexibility in complex development environments.
