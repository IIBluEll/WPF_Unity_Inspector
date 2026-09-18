using System.IO;
using System.Security;
using WPF_UnityInspector.Models;

namespace WPF_UnityInspector.Services
{
    public sealed class ProjectSizeService
    {
        public Task<ProjectSizeResult> CalculateProjectSize_async(
            string projectPath,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () => CalculateProjectSize(projectPath, cancellationToken),
                cancellationToken);
        }

        private static ProjectSizeResult CalculateProjectSize(
            string projectPath,
            CancellationToken cancellationToken)
        {
            string rootPath = Path.GetFullPath(projectPath);

            long diskSizeBytes = 0;
            long sourceSizeBytes = 0;
            long assetsSizeBytes = 0;
            int skippedEntryCount = 0;
            int skippedLinkCount = 0;

            Stack<(string Path, bool IsSource, bool IsAssets)> directories = new();
            directories.Push((rootPath, false, false));

            while ( directories.Count > 0 )
            {
                cancellationToken.ThrowIfCancellationRequested();

                var current = directories.Pop();
                bool isRoot = string.Equals(
                    current.Path,
                    rootPath,
                    StringComparison.OrdinalIgnoreCase);

                try
                {
                    foreach ( string entryPath in Directory.EnumerateFileSystemEntries(current.Path) )
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        FileAttributes attributes;

                        try
                        {
                            attributes = File.GetAttributes(entryPath);
                        }
                        catch ( Exception exception ) when ( IsFileSystemError(exception) )
                        {
                            skippedEntryCount++;
                            continue;
                        }

                        if ( (attributes & FileAttributes.ReparsePoint) != 0 )
                        {
                            skippedLinkCount++;
                            continue;
                        }

                        if ( (attributes & FileAttributes.Directory) != 0 )
                        {
                            bool isSource = current.IsSource;
                            bool isAssets = current.IsAssets;

                            if ( isRoot )
                            {
                                string name = Path.GetFileName(entryPath) ?? string.Empty;

                                isAssets = string.Equals(
                                    name,
                                    "Assets",
                                    StringComparison.OrdinalIgnoreCase);

                                isSource = isAssets ||
                                    string.Equals(name, "Packages", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(name, "ProjectSettings", StringComparison.OrdinalIgnoreCase);
                            }

                            directories.Push((entryPath, isSource, isAssets));
                            continue;
                        }

                        try
                        {
                            long fileSizeBytes = new FileInfo(entryPath).Length;

                            diskSizeBytes += fileSizeBytes;

                            if ( current.IsSource )
                            {
                                sourceSizeBytes += fileSizeBytes;
                            }

                            if ( current.IsAssets )
                            {
                                assetsSizeBytes += fileSizeBytes;
                            }
                        }
                        catch ( Exception exception ) when ( IsFileSystemError(exception) )
                        {
                            skippedEntryCount++;
                        }
                    }
                }
                catch ( Exception exception ) when ( IsFileSystemError(exception) )
                {
                    skippedEntryCount++;
                }
            }

            return new ProjectSizeResult
            {
                DiskSizeBytes = diskSizeBytes,
                SourceSizeBytes = sourceSizeBytes,
                AssetsSizeBytes = assetsSizeBytes,
                SkippedEntryCount = skippedEntryCount,
                SkippedLinkCount = skippedLinkCount
            };
        }

        private static bool IsFileSystemError(Exception exception)
        {
            return exception is IOException or UnauthorizedAccessException or SecurityException;
        }
    }
}
