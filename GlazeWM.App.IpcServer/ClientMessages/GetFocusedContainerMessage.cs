using CommandLine;

namespace GlazeWM.App.IpcServer.ClientMessages
{
  [Verb("focused_container", HelpText = "Get the focused container.")]
  public class GetFocusedContainerMessage
  {
  }
}
