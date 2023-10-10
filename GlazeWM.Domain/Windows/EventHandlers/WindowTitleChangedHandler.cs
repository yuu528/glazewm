using System.Collections.Generic;
using System.Linq;
using GlazeWM.Domain.Common;
using GlazeWM.Domain.Containers.Commands;
using GlazeWM.Domain.Monitors.Commands;
using GlazeWM.Domain.Windows.Commands;
using GlazeWM.Infrastructure.Bussing;
using GlazeWM.Infrastructure.Common.Events;

namespace GlazeWM.Domain.Windows.EventHandlers
{
  internal sealed class WindowTitleChangedHandler : IEventHandler<WindowTitleChangedEvent>
  {
    private readonly Bus _bus;
    private readonly WindowService _windowService;

    public WindowTitleChangedHandler(Bus bus, WindowService windowService)
    {
      _bus = bus;
      _windowService = windowService;
    }

    public void Handle(WindowTitleChangedEvent @event)
    {
      var windowHandle = @event.WindowHandle;

      var window = _windowService.GetWindows()
        .FirstOrDefault(window => window.Handle == windowHandle);

      if (window is null)
        return;

      _bus.Invoke(new RunWindowRulesCommand(
        window,
        new List<WindowRuleType>() {
          WindowRuleType.FirstTitleChange,
          WindowRuleType.TitleChange
        }
      ));
      _bus.Invoke(new RedrawContainersCommand());
      _bus.Invoke(new SyncNativeFocusCommand());
    }
  }
}
