using System;
using GlazeWM.Infrastructure.WindowsApi;

namespace GlazeWM.Domain.Windows
{
  public sealed class MaximizedWindow : Window
  {
    public MaximizedWindow(
      IntPtr handle,
      Rect floatingPlacement,
      RectDelta borderDelta,
      Guid id = Guid.NewGuid()
    ) : base(handle, floatingPlacement, borderDelta, id, ContainerType.MinimizedWindow)
    {
    }
  }
}
