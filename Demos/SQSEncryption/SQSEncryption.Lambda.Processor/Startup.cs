using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SQSEncryption.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQSEncryption.Lambda.Processor
{
    [LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMessageEncryption, MessageEncryption>();
        }
    }
}
