using System.Text.Json.Serialization;

namespace WPF_UnityInspector.Models
{
    public sealed class BuildReportData
    {
        [JsonRequired]
        public int SchemaVersion { get; init; }

        public string ProjectName { get; init; } = string.Empty;

        public string UnityVersion { get; init; } = string.Empty;

        public string AssetSizeDefinition { get; init; } = string.Empty;

        [JsonRequired]
        public BuildInfo Build { get; init; } = new();

        [JsonRequired]
        public List<AssetBuildInfo> Assets { get; init; } = new();

        [JsonRequired]
        public List<BuildMessage> Messages { get; init; } = new();
    }

    public sealed class BuildInfo
    {
        public string BuildGuid { get; init; } = string.Empty;

        public string Platform { get; init; } = string.Empty;

        public string Result { get; init; } = string.Empty;

        public string OutputPath { get; init; } = string.Empty;

        public DateTimeOffset BuildStartedAtUtc { get; init; }

        public DateTimeOffset BuildEndedAtUtc { get; init; }

        public DateTimeOffset ReportGeneratedAtUtc { get; init; }

        public double BuildTimeSeconds { get; init; }

        public long ReportedOutputSizeBytes { get; init; }

        public long ArtifactSizeBytes { get; init; }

        public string ArtifactSizeSource { get; init; } = string.Empty;

        public int WarningCount { get; init; }

        public int ErrorCount { get; init; }

        public int ReportedWarningCount { get; init; }

        public int ReportedErrorCount { get; init; }
    }

    public sealed class AssetBuildInfo
    {
        public string Name { get; init; } = string.Empty;

        public string Path { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;

        public long PackedSizeBytes { get; init; }
    }

    public sealed class BuildMessage
    {
        public string Type { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;
    }
}