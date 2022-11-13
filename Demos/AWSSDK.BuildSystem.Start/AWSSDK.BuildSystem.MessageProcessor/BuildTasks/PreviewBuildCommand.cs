using AWSSDK.BuildSystem.Common;
using Microsoft.Extensions.Logging;

namespace AWSSDK.BuildSystem.MessageProcessor.BuildTasks
{
    public class PreviewBuildCommand : BaseBuildCommand
    {
        PreviewMessage _message;

        public PreviewBuildCommand(ILogger<PreviewBuildCommand> logger, PreviewMessage message)
            : base(logger)
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
            Logger.LogInformation("Run Preview MSBuild targets");
            await Task.Delay(2000);
        }
    }
}
