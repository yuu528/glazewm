using CommandLine;

namespace GlazeWM.App.IpcServer.ClientMessages
{
  [Verb("binding_mode", HelpText = "Get the active binding mode.")]
  public class GetBindingModeMessage
  {
  }
}
