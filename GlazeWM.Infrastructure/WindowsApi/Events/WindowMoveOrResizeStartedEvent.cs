using System;
using GlazeWM.Infrastructure.Bussing;

namespace GlazeWM.Infrastructure.WindowsApi.Events
{
  public class WindowMoveOrResizeStartedEvent : Event
  {
    public IntPtr WindowHandle { get; }

    public WindowMoveOrResizeStartedEvent(IntPtr windowHandle)
    {
      WindowHandle = windowHandle;
    }
  }
}
