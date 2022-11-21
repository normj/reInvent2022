using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.Orchestrator.EventBus
{
    public static class Telemetry
    {
        public const string ServiceName = "AWSSDK Orchestrator EventBus";
        public const string ServiceVersion = "1.0.0";


        public static readonly ActivitySource RootActivitySource = new(ServiceName);
    }
}
