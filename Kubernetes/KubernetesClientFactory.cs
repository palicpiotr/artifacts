using k8s;

namespace Artifacts.Kubernetes;

public sealed class KubernetesClientFactory(KubernetesClientOptions options) : IKubernetesClientFactory
{
    private readonly KubernetesClientOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    public IKubernetesClient Create()
    {
        KubernetesClientConfiguration config;

        if (_options.InCluster)
        {
            config = KubernetesClientConfiguration.InClusterConfig();
        }
        else if (!string.IsNullOrWhiteSpace(_options.KubeConfigPath))
        {
            config = KubernetesClientConfiguration.BuildConfigFromConfigFile(
                _options.KubeConfigPath,
                _options.ContextName);
        }
        else if (_options.UseDefaultConfig)
        {
            config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        }
        else
        {
            throw new InvalidOperationException("No Kubernetes configuration specified.");
        }

        return new KubernetesClient(new k8s.Kubernetes(config));
    }
}
