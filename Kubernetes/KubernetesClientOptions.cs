namespace Artifacts.Kubernetes;

public sealed class KubernetesClientOptions
{
    public bool InCluster { get; init; }
    public string? KubeConfigPath { get; init; }
    public string? ContextName { get; init; }
    public bool UseDefaultConfig { get; init; } = true;
}
