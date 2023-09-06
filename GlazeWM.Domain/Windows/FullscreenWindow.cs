using System;
using GlazeWM.Infrastructure.WindowsApi;

namespace GlazeWM.Domain.Windows
{
  public sealed class FullscreenWindow : Window
  {
    public FullscreenWindow(
      IntPtr handle,
      Rect floatingPlacement,
      RectDelta borderDelta,
      Guid id = Guid.NewGuid()
    ) : base(handle, floatingPlacement, borderDelta, id, ContainerType.FullscreenWindow)
    {
    }
  }
}
