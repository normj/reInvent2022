using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.MessageProcessor
{
    public class AppSettings
    {
        public string BuildMessagesQueueUrl { get; set; }
        public string KmsKeyArn { get; set; }

        public string BuildStatusTopicArn { get; set; }
    }
}
