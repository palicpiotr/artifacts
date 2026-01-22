using Artifacts.Kubernetes;
using Artifacts.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton(new KubernetesClientOptions { UseDefaultConfig = true });
services.AddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();

using var provider = services.BuildServiceProvider();
using var bootstrapClient = provider.GetRequiredService<IKubernetesClientFactory>().Create();

var contexts = bootstrapClient.ListKubeConfigContexts();
if (contexts.Count == 0)
{
    Console.Error.WriteLine("No kubeconfig contexts found.");
    return;
}

Console.WriteLine("Available contexts:");
foreach (var context in contexts)
{
    Console.WriteLine(context);
}

Console.WriteLine("Enter context names separated by commas:");
var input = Console.ReadLine() ?? string.Empty;
var requestedContexts = input
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Distinct(StringComparer.Ordinal)
    .ToArray();

if (requestedContexts.Length == 0)
{
    Console.Error.WriteLine("No contexts selected.");
    return;
}

var unknownContexts = requestedContexts
    .Where(context => !contexts.Contains(context, StringComparer.Ordinal))
    .ToArray();

if (unknownContexts.Length > 0)
{
    Console.Error.WriteLine($"Unknown contexts: {string.Join(", ", unknownContexts)}");
    return;
}

List<ImageDef> images = [];

foreach (var contextName in requestedContexts)
{
    var options = new KubernetesClientOptions
    {
        UseDefaultConfig = true,
        ContextName = contextName
    };

    using var client = new KubernetesClientFactory(options).Create();
    var deployments = await client.ListDeploymentsAsync();

    if (deployments.Items.Count == 0)
    {
        Console.WriteLine($"[{contextName}] No deployments found.");
        continue;
    }

    foreach (var deployment in deployments.Items)
    {
        var deploymentName = deployment.Metadata?.Name ?? "(unknown)";
        var deploymentNamespace = deployment.Metadata?.NamespaceProperty ?? "(unknown)";
        var containers = deployment.Spec?.Template?.Spec?.Containers ?? [];

        if (containers.Count == 0)
        {
            Console.WriteLine($"[{contextName}] {deploymentNamespace}/{deploymentName} | (no containers)");
            continue;
        }

        foreach (var container in containers)
        {
            var image = container.Image ?? string.Empty;
            var imageTag = GetImageTag(image);
            var containerName = container.Name ?? "(unknown)";
            Console.WriteLine($"[{contextName}] {deploymentNamespace}/{deploymentName} | {containerName} | {imageTag}");

            images.Add(new ImageDef(contextName, deploymentNamespace, deploymentName, containerName, imageTag));
        }
    }
}

static string GetImageTag(string image)
{
    if (string.IsNullOrWhiteSpace(image))
    {
        return "(unknown)";
    }

    var atIndex = image.LastIndexOf('@');
    if (atIndex >= 0 && atIndex + 1 < image.Length)
    {
        return image[(atIndex + 1)..];
    }

    var slashIndex = image.LastIndexOf('/');
    var colonIndex = image.LastIndexOf(':');
    if (colonIndex > slashIndex && colonIndex + 1 < image.Length)
    {
        return image[(colonIndex + 1)..];
    }

    return "latest";
}
