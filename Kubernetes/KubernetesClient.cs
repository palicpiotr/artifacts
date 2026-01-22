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
        var config = KubernetesClientConfiguration.LoadKubeConfig();
        if (config?.Contexts is null || !config.Contexts.Any())
        {
            return [];
        }

        return [.. config.Contexts
            .Select(context => context.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))];
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
