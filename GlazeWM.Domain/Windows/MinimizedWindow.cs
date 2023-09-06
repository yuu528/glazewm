using System;
using GlazeWM.Infrastructure.WindowsApi;

namespace GlazeWM.Domain.Windows
{
  public sealed class MinimizedWindow : Window
  {
    public WindowType PreviousState;

    public MinimizedWindow(
      IntPtr handle,
      Rect floatingPlacement,
      RectDelta borderDelta,
      WindowType previousState,
      Guid id = Guid.NewGuid()
    ) : base(handle, floatingPlacement, borderDelta, id, ContainerType.MinimizedWindow)
    {
      PreviousState = previousState;
    }
  }
}
