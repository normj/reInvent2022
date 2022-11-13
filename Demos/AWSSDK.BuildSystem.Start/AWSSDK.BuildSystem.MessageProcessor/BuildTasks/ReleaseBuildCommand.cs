using AWSSDK.BuildSystem.Common;
using Microsoft.Extensions.Logging;
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

        public ReleaseBuildCommand(ILogger<PreviewBuildCommand> logger, ReleaseMessage message)
            : base(logger)
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
            Logger.LogInformation("Run Release MSBuild targets");
            await Task.Delay(2000);
        }

        private async Task GenerateAPIDocsAsync()
        {
            Logger.LogInformation("Generate API Docs");
            await Task.Delay(1500);
        }

        private async Task SignBuildArtifactsAsync()
        {
            Logger.LogInformation("Sign build artifacts");
            await Task.Delay(500);
        }
    }
}
