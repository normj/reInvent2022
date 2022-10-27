using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.MessageProcessor.BuildTasks
{
    public abstract class BaseBuildCommand
    {
        protected async Task PrepareWorkspaceAsync()
        {
            using var activity = Telemetry.RootActivitySource.StartActivity("Prepare workspace");

            await Task.Delay(3000);
        }

        protected async Task UploadArtifactsAsync(string localDirectory, string prefix)
        {
            using var activity = Telemetry.RootActivitySource.StartActivity("Upload Artifacts");

            await Task.Delay(1000);
        }

        public abstract Task ExecuteAsync();
    }
}
