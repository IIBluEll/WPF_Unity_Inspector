namespace WPF_UnityInspector.Models
{
    public sealed class AssetTypeSummary
    {
        public string Type { get; init; } = string.Empty;
        public int AssetCount { get; init; }
        public long PackedSizeBytes { get; init; }
        public double SharePercent { get; init; }
    }
}
