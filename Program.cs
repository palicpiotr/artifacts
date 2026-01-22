using Artifacts.Kubernetes;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton(new KubernetesClientOptions { UseDefaultConfig = true });
services.AddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();

using var provider = services.BuildServiceProvider();
using var client = provider.GetRequiredService<IKubernetesClientFactory>().Create();

var namespaces = await client.ListNamespacesAsync();

if (namespaces.Items.Count == 0)
{
    Console.Error.WriteLine("No namespaces found.");
}

foreach (var item in namespaces.Items)
{
    Console.WriteLine(item.Metadata?.Name ?? "(unknown)");
}
