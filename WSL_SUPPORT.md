# WSL Support for SourceGit

This document describes how to use SourceGit with repositories stored in Windows Subsystem for Linux (WSL).

## Overview

SourceGit now supports using Git from WSL, allowing you to:
- Work with repositories stored in the WSL filesystem
- Use the Git executable from your WSL distribution
- Access both Windows and WSL repositories from the same application
- Configure WSL Git globally or per-repository

## Prerequisites

1. Windows 10/11 with WSL2 installed
2. A WSL distribution with Git installed (e.g., Ubuntu, Debian)
3. SourceGit running on Windows

## Setup

### 1. Install Git in WSL

Open your WSL terminal and install Git:

```bash
# For Ubuntu/Debian
sudo apt update
sudo apt install git

# For other distributions, use their respective package managers
```

Verify Git is installed:
```bash
git --version
```

### 2. Enable WSL Support in SourceGit (Global)

1. Open SourceGit
2. Go to **Preferences** (click the gear icon or use `Ctrl+Shift+P`)
3. Navigate to the **GIT** tab
4. Check the box **"Enable WSL Git integration"**
5. (Optional) Specify a WSL distribution name in the **"WSL Distribution"** field
   - Leave empty to use your default WSL distribution
   - Common names: `Ubuntu`, `Ubuntu-22.04`, `Debian`, etc.
6. Click **Save**

**Note:** This sets WSL Git as the default for all repositories. See [Per-Repository Configuration](#per-repository-configuration) for more granular control.

## Usage

### Opening WSL Repositories

#### Method 1: Using Windows Network Path
1. Navigate to `\\wsl$\<distro-name>\` in Windows Explorer
2. Find your repository (e.g., `\\wsl$\Ubuntu\home\username\myrepo`)
3. Drag and drop the folder into SourceGit, or use "Add Repository"

#### Method 2: Using wsl.localhost Path
1. Navigate to `\\wsl.localhost\<distro-name>\` in Windows Explorer
2. Find your repository (e.g., `\\wsl.localhost\Ubuntu\home\username\myrepo`)
3. Drag and drop the folder into SourceGit, or use "Add Repository"

### Path Conversions

SourceGit automatically handles path conversions between Windows and WSL:

- **Windows → WSL**: `C:\Users\username\project` → `/mnt/c/Users/username/project`
- **WSL → Windows**: `/home/username/project` → `\\wsl.localhost\Ubuntu\home\username\project`
- **WSL mnt paths**: `/mnt/c/project` → `C:\project`

### SSH Keys

If you use SSH keys for Git operations:

1. **Windows SSH Keys**: Place your keys in `C:\Users\<username>\.ssh\`
   - SourceGit will automatically convert the path to WSL format
   - Example: `C:\Users\john\.ssh\id_rsa` → `/mnt/c/Users/john/.ssh/id_rsa`

2. **WSL SSH Keys**: You can also use keys stored in WSL
   - Path format: `\\wsl.localhost\Ubuntu\home\username\.ssh\id_rsa`
   - SourceGit will convert this to `/home/username/.ssh/id_rsa` in WSL

## How It Works

When WSL support is enabled:

1. SourceGit detects and uses `git` from your WSL distribution
2. All Git commands are executed through `wsl.exe`
3. Paths are automatically converted between Windows and WSL formats
4. Environment variables are properly set for WSL context

Example command transformation:
```
# What you see:
git status

# What actually runs:
wsl.exe -d Ubuntu cd '/home/username/repo' && git --no-pager status
```

## Troubleshooting

### Git Version Shows as Invalid

**Problem**: SourceGit shows a warning that Git is invalid.

**Solution**: 
- Ensure Git is installed in WSL: `wsl git --version`
- Check that the correct distribution is specified in preferences
- Try unchecking and re-checking "Enable WSL Git integration"

### Repository Operations Fail

**Problem**: Git operations fail with path errors.

**Solution**:
- Verify the repository path is accessible from WSL
- Try opening WSL terminal and running `cd <path>` to test accessibility
- Ensure file permissions are correct in WSL

### Performance Issues

**Problem**: Operations are slower than expected.

**Solution**:
- Store repositories in WSL filesystem (`/home/...`) rather than Windows filesystem (`/mnt/c/...`)
- WSL2 has much better performance for Linux filesystem operations
- Avoid accessing Windows files from WSL when possible

### SSH Keys Not Working

**Problem**: SSH authentication fails.

**Solution**:
- Verify SSH key permissions: `chmod 600 ~/.ssh/id_rsa` in WSL
- Test SSH connection: `wsl ssh -T git@github.com`
- Check that the key path is correctly specified in SourceGit

## Best Practices

1. **Repository Location**:
   - For best performance, store repositories in WSL filesystem: `/home/username/projects/`
   - Access them via: `\\wsl.localhost\Ubuntu\home\username\projects\`

2. **Git Configuration**:
   - Configure Git in WSL separately from Windows Git
   - Set user name and email in WSL: 
     ```bash
     git config --global user.name "Your Name"
     git config --global user.email "your.email@example.com"
     ```

3. **Line Endings**:
   - Set `core.autocrlf=input` in WSL to avoid line ending issues
   - SourceGit respects the Git configuration in WSL

4. **Credential Storage**:
   - Use Git credential helpers in WSL for storing credentials
   - Example: `git config --global credential.helper store`

## Per-Repository Configuration

Instead of enabling WSL globally, you can configure Git executable on a per-repository basis. This is useful when you have a mix of Windows and WSL repositories.

### Using Per-Repository Settings

1. Open the repository in SourceGit
2. Click **⚙️ Configure** (or Repository → Configure)
3. In the **GIT** tab, find **"Git Executable (Override)"**
4. Enter the WSL git path in format: `wsl:Ubuntu:/usr/bin/git`
5. Click **OK**

This repository will now use WSL Git, while other repositories use the global setting (which could be Windows Git).

**See [PER_REPOSITORY_GIT.md](PER_REPOSITORY_GIT.md) for complete documentation on per-repository configuration.**

## Known Limitations

- Interactive rebase editor might have limitations
- Some Windows-specific tools may not work in WSL context
- Credential managers might need separate configuration in WSL

## Feedback and Issues

If you encounter any issues with WSL support, please report them on the SourceGit GitHub repository with:
- Your Windows version
- WSL distribution and version
- Git version in WSL
- Detailed error messages or unexpected behavior

---

**Note**: This feature requires Windows 10 version 1903 or later with WSL2 installed and configured.
