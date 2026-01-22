using System;
using System.Collections.Generic;
using System.Text;

namespace Artifacts.Models;

internal sealed record ImageDef(string ContextName, string Namespace, string DeploymentName, string ContainerName, string ImageTag);
