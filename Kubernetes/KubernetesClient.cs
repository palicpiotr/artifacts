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

    public void Dispose()
    {
        _client.Dispose();
    }
}
