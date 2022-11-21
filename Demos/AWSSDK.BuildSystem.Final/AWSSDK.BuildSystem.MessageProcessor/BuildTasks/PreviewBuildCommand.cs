using AWSSDK.BuildSystem.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.MessageProcessor.BuildTasks
{
    public class PreviewBuildCommand : BaseBuildCommand
    {
        PreviewMessage _message;

        public PreviewBuildCommand(PreviewMessage message)
        {
            _message = message;
        }

        public override async Task ExecuteAsync()
        {
            await PrepareWorkspaceAsync();

            await RunPreviewMSBuildAsync();

            await UploadArtifactsAsync("/tmp/workspace", "/preview/");
        }

        private async Task RunPreviewMSBuildAsync()
        {
            using var activity = Telemetry.RootActivitySource.StartActivity("Preview MSBuild Target");

            if(_message.ServiceName == "EC2")
            {
                throw new ApplicationException("Service model is invalid");
            }

            await Task.Delay(2000);
        }
    }
}
