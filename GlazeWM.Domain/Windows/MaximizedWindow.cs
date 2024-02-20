using System;
using GlazeWM.Domain.Common;
using GlazeWM.Infrastructure.WindowsApi;

namespace GlazeWM.Domain.Windows
{
  public sealed class MaximizedWindow : Window
  {
    /// <inheritdoc />
    public override ContainerType Type { get; } = ContainerType.MaximizedWindow;

    public WindowType PreviousState;

    public MaximizedWindow(
      IntPtr handle,
      Rect floatingPlacement,
      RectDelta borderDelta,
      WindowType previousState
    ) : base(handle, floatingPlacement, borderDelta)
    {
      PreviousState = previousState;
    }
  }
}
