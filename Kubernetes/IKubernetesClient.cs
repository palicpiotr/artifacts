using k8s.Models;

namespace Artifacts.Kubernetes;

public interface IKubernetesClient : IDisposable
{
    Task<V1PodList> ListPodsAsync(string? namespaceName = null, CancellationToken cancellationToken = default);
    Task<V1NamespaceList> ListNamespacesAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<string> ListKubeConfigContexts();
    Task<V1DeploymentList> ListDeploymentsAsync(CancellationToken cancellationToken = default);
}
