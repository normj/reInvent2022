using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.Common;

public class PreviewMessage
{
    public PreviewMessage(string serviceName, string modelUrl)
    {
        this.ServiceName = serviceName;
        this.ModelUrl = modelUrl;
    }

    public string ServiceName { get; }
    public string ModelUrl { get; }
}
