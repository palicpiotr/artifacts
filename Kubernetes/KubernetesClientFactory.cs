using k8s;
using k8s.KubeConfigModels;

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
        else if (!string.IsNullOrWhiteSpace(_options.KubeConfigPath) || _options.UseDefaultConfig)
        {
            var kubeConfig = LoadKubeConfig(_options.KubeConfigPath);
            config = KubernetesClientConfiguration.BuildConfigFromConfigObject(
                kubeConfig,
                _options.ContextName);
        }
        else
        {
            throw new InvalidOperationException("No Kubernetes configuration specified.");
        }

        return new KubernetesClient(new k8s.Kubernetes(config));
    }

    private static K8SConfiguration LoadKubeConfig(string? kubeConfigPath)
    {
        var files = ResolveKubeConfigFiles(kubeConfigPath);
        if (files.Length == 0)
        {
            return KubernetesClientConfiguration.LoadKubeConfig();
        }

        var firstFile = files[0];
        return KubernetesClientConfiguration.LoadKubeConfig(firstFile.FullName, false);
    }

    private static FileInfo[] ResolveKubeConfigFiles(string? kubeConfigPath)
    {
        var pathValue = !string.IsNullOrWhiteSpace(kubeConfigPath)
            ? kubeConfigPath
            : Environment.GetEnvironmentVariable("KUBECONFIG");

        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return [];
        }

        return pathValue
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(path => new FileInfo(path))
            .ToArray();
    }
}
