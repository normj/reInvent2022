using AWSSDK.BuildSystem.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSSDK.BuildSystem.MessageProcessor.BuildTasks
{
    public class ReleaseBuildCommand : BaseBuildCommand
    {
        ReleaseMessage _message;

        public ReleaseBuildCommand(ReleaseMessage message)
        {
            _message = message;
        }

        public override async Task ExecuteAsync()
        {
            await PrepareWorkspaceAsync();

            await RunReleaseMSBuildAsync();

            await GenerateAPIDocsAsync();

            await UploadArtifactsAsync("/tmp/workspace", "/release/");

        }

        private async Task RunReleaseMSBuildAsync()
        {
            using var activity = Telemetry.RootActivitySource.StartActivity("Release MSBuild Target");

            await Task.Delay(2000);
        }

        private async Task GenerateAPIDocsAsync()
        {
            using var activity = Telemetry.RootActivitySource.StartActivity("Generate API Docs");

            await Task.Delay(1500);
        }

        private async Task SignBuildArtifactsAsync()
        {
            using var activity = Telemetry.RootActivitySource.StartActivity("Sign Build Artifacts");

            await Task.Delay(500);
        }
    }
}
