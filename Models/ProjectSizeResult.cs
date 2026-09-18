namespace WPF_UnityInspector.Models
{
    public sealed class ProjectSizeResult
    {
        public long DiskSizeBytes { get; init; }
        public long SourceSizeBytes { get; init; }
        public long AssetsSizeBytes { get; init; }
        public int SkippedEntryCount { get; init; }
        public int SkippedLinkCount { get; init; }

        public bool IsPartial => SkippedEntryCount > 0;
    }
}
