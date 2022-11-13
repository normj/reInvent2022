using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.MessageProcessor.BuildTasks
{
    public abstract class BaseBuildCommand
    {
        protected ILogger Logger { get; }

        public BaseBuildCommand(ILogger logger)
        {
            Logger = logger;
        }

        protected async Task PrepareWorkspaceAsync()
        {
            Logger.LogInformation("Preparing Git Workspace");
            await Task.Delay(3000);
        }

        protected async Task UploadArtifactsAsync(string localDirectory, string prefix)
        {
            Logger.LogInformation("Uploading build artifacts");
            await Task.Delay(1000);
        }

        public abstract Task ExecuteAsync();
    }
}
