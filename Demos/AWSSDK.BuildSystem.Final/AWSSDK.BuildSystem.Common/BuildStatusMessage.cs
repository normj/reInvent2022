using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.Common
{
    public class BuildStatusMessage
    {
        public BuildStatusMessage() { }


        public BuildStatusMessage(bool success, BuildType buildType, string buildId, string? buildException)
        {
            Success = success;
            BuildType = buildType;
            BuildId = buildId;
            BuildException = buildException;
        }

        public bool Success { get; set; }
        public BuildType BuildType { get; set; }
        public string BuildId {get;set;}
        public string? BuildException { get; set; }
    }
}
