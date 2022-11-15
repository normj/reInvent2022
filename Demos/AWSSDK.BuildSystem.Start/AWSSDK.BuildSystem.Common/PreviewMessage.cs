using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.Common;

public class PreviewMessage
{
    public PreviewMessage(string buildId, string serviceName, string modelUrl)
    {
        this.BuildId = buildId;
        this.ServiceName = serviceName;
        this.ModelUrl = modelUrl;
    }

    public string BuildId { get; }
    public string ServiceName { get; }
    public string ModelUrl { get; }
}
