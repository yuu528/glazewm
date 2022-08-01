using System.Linq;
using GlazeWM.Domain.Common.Utils;
using GlazeWM.Infrastructure.Bussing;
using GlazeWM.Infrastructure.WindowsApi.Events;
using Microsoft.Extensions.Logging;

namespace GlazeWM.Domain.Windows.EventHandlers
{
  internal class WindowMoveOrResizeStartedHandler : IEventHandler<WindowMoveOrResizeStartedEvent>
  {
    private readonly WindowService _windowService;
    private readonly ILogger<WindowMoveOrResizeStartedHandler> _logger;

    public WindowMoveOrResizeStartedHandler(
      WindowService windowService,
      ILogger<WindowMoveOrResizeStartedHandler> logger
    )
    {
      _windowService = windowService;
      _logger = logger;
    }

    public void Handle(WindowMoveOrResizeStartedEvent @event)
    {
      var window = _windowService.GetWindows()
        .FirstOrDefault(window => window.Hwnd == @event.WindowHandle);

      if (window is null)
        return;

      _logger.LogWindowEvent("Window move/resize started", window);
    }
  }
}
