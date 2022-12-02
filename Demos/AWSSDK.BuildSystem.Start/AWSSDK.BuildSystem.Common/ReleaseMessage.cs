using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.Common
{
    public class ReleaseMessage
    {
#nullable disable warnings        
        public ReleaseMessage() { }
#nullable restore warnings
        public ReleaseMessage(string buildId) 
        { 
            this.BuildId= buildId;
        }

        public ReleaseMessage(string buildId, IList<ServiceModel> services)
            : this(buildId)
        {
            this.BuildId = buildId;
            this.Services = services;
        }


        public string BuildId { get; set; }

        public IList<ServiceModel> Services { get; set; } = new List<ServiceModel>();
    }
    
    public class ServiceModel
    {
        public ServiceModel(string serviceName, string modelUrl)
        {
            ServiceName = serviceName;
            ModelUrl = modelUrl;
        }

        public string ServiceName { get; set; }

        public string ModelUrl { get; set; }
    }
}
