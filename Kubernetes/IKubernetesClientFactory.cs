namespace Artifacts.Kubernetes;

public interface IKubernetesClientFactory
{
    IKubernetesClient Create();
}
