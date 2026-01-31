using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceGit.Native
{
    /// <summary>
    /// WSL (Windows Subsystem for Linux) integration utilities
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static partial class WSL
    {
        private static bool? _isWSLAvailable = null;
        private static string _defaultDistro = null;

        /// <summary>
        /// Check if WSL is available on the system
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                if (_isWSLAvailable.HasValue)
                    return _isWSLAvailable.Value;

                _isWSLAvailable = CheckWSLAvailability();
                return _isWSLAvailable.Value;
            }
        }

        /// <summary>
        /// Get the default WSL distribution name
        /// </summary>
        public static string DefaultDistro
        {
            get
            {
                if (_defaultDistro != null)
                    return _defaultDistro;

                _defaultDistro = GetDefaultDistribution();
                return _defaultDistro;
            }
        }

        /// <summary>
        /// Check if a path is a WSL path (starts with \\wsl$\ or \\wsl.localhost\)
        /// </summary>
        public static bool IsWSLPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return path.StartsWith(@"\\wsl$\", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith(@"\\wsl.localhost\", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Convert a Windows path to WSL path format
        /// </summary>
        public static string ConvertWindowsPathToWSL(string windowsPath, string distro = null)
        {
            if (string.IsNullOrEmpty(windowsPath))
                return windowsPath;

            // If it's already a WSL path, extract the Linux portion
            if (IsWSLPath(windowsPath))
            {
                // Format: \\wsl$\Ubuntu\home\user\... or \\wsl.localhost\Ubuntu\home\user\...
                var match = REG_WSL_PATH().Match(windowsPath);
                if (match.Success && match.Groups.Count >= 3)
                {
                    return "/" + match.Groups[2].Value.Replace('\\', '/');
                }
                return windowsPath;
            }

            // Convert Windows drive path to WSL /mnt/ path
            // Example: C:\Users\... -> /mnt/c/Users/...
            if (windowsPath.Length >= 2 && windowsPath[1] == ':')
            {
                var driveLetter = char.ToLower(windowsPath[0]);
                var pathPart = windowsPath.Substring(2).Replace('\\', '/');
                return $"/mnt/{driveLetter}{pathPart}";
            }

            return windowsPath;
        }

        /// <summary>
        /// Convert a WSL path to Windows path format
        /// </summary>
        public static string ConvertWSLPathToWindows(string wslPath, string distro = null)
        {
            if (string.IsNullOrEmpty(wslPath))
                return wslPath;

            // If it's already a Windows-style WSL path, return as-is
            if (IsWSLPath(wslPath))
                return wslPath;

            var useDistro = distro ?? DefaultDistro;
            if (string.IsNullOrEmpty(useDistro))
                useDistro = "Ubuntu";

            // Convert /mnt/c/... style paths to C:\...
            if (wslPath.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase) && wslPath.Length > 6)
            {
                var driveLetter = char.ToUpper(wslPath[5]);
                var pathPart = wslPath.Substring(6).Replace('/', '\\');
                return $"{driveLetter}:{pathPart}";
            }

            // Convert Linux paths to \\wsl.localhost\distro\path format
            if (wslPath.StartsWith("/"))
            {
                var pathPart = wslPath.Substring(1).Replace('/', '\\');
                return $@"\\wsl.localhost\{useDistro}\{pathPart}";
            }

            return wslPath;
        }

        /// <summary>
        /// Find git executable in WSL
        /// </summary>
        public static string FindGitExecutable(string distro = null)
        {
            if (!IsAvailable)
                return null;

            var useDistro = distro ?? DefaultDistro;
            var args = string.IsNullOrEmpty(useDistro) ? "which git" : $"-d {useDistro} which git";

            var start = new ProcessStartInfo();
            start.FileName = "wsl.exe";
            start.Arguments = args;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.StandardOutputEncoding = Encoding.UTF8;

            try
            {
                using var proc = Process.Start(start);
                if (proc == null)
                    return null;

                var output = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit();

                if (proc.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    // Return the path to git in WSL format (e.g., /usr/bin/git)
                    return output;
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        /// <summary>
        /// Get git version from WSL
        /// </summary>
        public static string GetGitVersion(string distro = null)
        {
            if (!IsAvailable)
                return null;

            var useDistro = distro ?? DefaultDistro;
            var args = string.IsNullOrEmpty(useDistro) ? "git --version" : $"-d {useDistro} git --version";

            var start = new ProcessStartInfo();
            start.FileName = "wsl.exe";
            start.Arguments = args;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.StandardOutputEncoding = Encoding.UTF8;

            try
            {
                using var proc = Process.Start(start);
                if (proc == null)
                    return null;

                var output = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit();

                if (proc.ExitCode == 0)
                    return output;
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private static bool CheckWSLAvailability()
        {
            try
            {
                var start = new ProcessStartInfo();
                start.FileName = "wsl.exe";
                start.Arguments = "--status";
                start.UseShellExecute = false;
                start.CreateNoWindow = true;
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;

                using var proc = Process.Start(start);
                if (proc == null)
                    return false;

                proc.WaitForExit();
                return proc.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private static string GetDefaultDistribution()
        {
            try
            {
                var start = new ProcessStartInfo();
                start.FileName = "wsl.exe";
                start.Arguments = "-l -q";
                start.UseShellExecute = false;
                start.CreateNoWindow = true;
                start.RedirectStandardOutput = true;
                start.StandardOutputEncoding = Encoding.Unicode; // WSL uses UTF-16

                using var proc = Process.Start(start);
                if (proc == null)
                    return null;

                var output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    // The first line is the default distribution
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0)
                    {
                        // Clean up the distribution name (remove default marker and whitespace)
                        var distro = lines[0].Trim();
                        distro = distro.Replace("(Default)", "").Trim();
                        // Remove any null characters that might appear
                        distro = distro.Replace("\0", "");
                        return distro;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        [GeneratedRegex(@"^\\\\wsl[\.$]\\([^\\]+)\\(.*)$", RegexOptions.IgnoreCase)]
        private static partial Regex REG_WSL_PATH();
    }
}
