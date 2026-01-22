using k8s;
using k8s.Models;

namespace Artifacts.Kubernetes;

public sealed class KubernetesClient(k8s.Kubernetes client) : IKubernetesClient
{
    private readonly k8s.Kubernetes _client = client ?? throw new ArgumentNullException(nameof(client));

    public Task<V1PodList> ListPodsAsync(string? namespaceName = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            return _client.ListPodForAllNamespacesAsync(cancellationToken: cancellationToken);
        }

        return _client.ListNamespacedPodAsync(namespaceName, cancellationToken: cancellationToken);
    }

    public Task<V1NamespaceList> ListNamespacesAsync(CancellationToken cancellationToken = default)
    {
        return _client.ListNamespaceAsync(cancellationToken: cancellationToken);
    }

    public IReadOnlyList<string> ListKubeConfigContexts()
    {
        var config = LoadKubeConfig();
        if (config?.Contexts is null || !config.Contexts.Any())
        {
            return [];
        }

        return [.. config.Contexts
            .Select(context => context.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))];
    }

    private static k8s.KubeConfigModels.K8SConfiguration LoadKubeConfig()
    {
        var files = ResolveKubeConfigFiles();
        if (files.Length == 0)
        {
            return KubernetesClientConfiguration.LoadKubeConfig();
        }

        var firstFile = files[0];
        return KubernetesClientConfiguration.LoadKubeConfig(firstFile.FullName, false);
    }

    private static FileInfo[] ResolveKubeConfigFiles()
    {
        var pathValue = Environment.GetEnvironmentVariable("KUBECONFIG");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return [];
        }

        return pathValue
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(path => new FileInfo(path))
            .ToArray();
    }

    public Task<V1DeploymentList> ListDeploymentsAsync(CancellationToken cancellationToken = default)
    {
        return _client.ListDeploymentForAllNamespacesAsync(cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
