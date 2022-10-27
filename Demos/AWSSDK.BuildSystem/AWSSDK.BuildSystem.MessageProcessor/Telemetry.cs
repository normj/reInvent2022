using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.MessageProcessor
{
    public static class Telemetry
    {
        public const string ServiceName = "AWSSDK Builder Processor";
        public const string ServiceVersion = "1.0.0";


        public static readonly ActivitySource RootActivitySource = new(ServiceName);
    }
}
