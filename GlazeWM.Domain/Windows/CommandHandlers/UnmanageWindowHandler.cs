using System.Diagnostics;
using GlazeWM.Domain.Containers;
using GlazeWM.Domain.Containers.Commands;
using GlazeWM.Domain.Windows.Commands;
using GlazeWM.Infrastructure.Bussing;
using static GlazeWM.Infrastructure.WindowsApi.WindowsApiService;

namespace GlazeWM.Domain.Windows.CommandHandlers
{
  internal sealed class UnmanageWindowHandler : ICommandHandler<UnmanageWindowCommand>
  {
    private readonly Bus _bus;
    private readonly ContainerService _containerService;
    private readonly WindowService _windowService;

    public UnmanageWindowHandler(
      Bus bus,
      ContainerService containerService,
      WindowService windowService)
    {
      _bus = bus;
      _containerService = containerService;
      _windowService = windowService;
    }

    public CommandResponse Handle(UnmanageWindowCommand command)
    {
      var window = command.Window;

      // Get container to switch focus to after the window has been removed.
      var focusTarget = WindowService.GetFocusTargetAfterRemoval(window);

      if (window is IResizable)
        _bus.Invoke(new DetachAndResizeContainerCommand(window));
      else
        _bus.Invoke(new DetachContainerCommand(window));

      _bus.Invoke(new RedrawContainersCommand());

      var foregroundWindow = GetForegroundWindow();
      var desktopWindow = _windowService.DesktopWindowHandle;

      var className = WindowService.GetClassNameOfHandle(foregroundWindow);
      var processName = WindowService.GetProcessOfHandle(foregroundWindow).ProcessName;
      Debug.WriteLine("===========");
      Debug.WriteLine($"WM focus target: {focusTarget}");
      Debug.WriteLine($"OS focus: className {className}");
      Debug.WriteLine($"OS focus: processName {processName}");

      // If focus has been set to the desktop window, then immediately reassign focus.
      // This happens after all windows have been closed.
      if (foregroundWindow == desktopWindow)
      {
        Debug.WriteLine("Desktop window has OS focus");
        _bus.Invoke(new SetNativeFocusCommand(focusTarget));
        return CommandResponse.Ok;
      }

      // If focus has been retained by the closed window, then immediately reassign focus.
      // This happens with certain Electron apps (eg. Discord, Slack).
      if (foregroundWindow == window.Handle && !WindowService.IsHandleVisible(window.Handle))
      {
        Debug.WriteLine("Original window still has OS focus (even though it's hidden)");
        _bus.Invoke(new SetNativeFocusCommand(focusTarget));
        return CommandResponse.Ok;
      }

      // The OS automatically switches focus to a different window after closing. If
      // there are focusable windows, then set focus *after* the OS sets focus. This will
      // cause focus to briefly flicker to the OS focus target and then to the WM's focus
      // target.
      _containerService.PendingFocusContainer = focusTarget;

      return CommandResponse.Ok;
    }
  }
}
