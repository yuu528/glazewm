using System;
using GlazeWM.Domain.Common;
using GlazeWM.Infrastructure.WindowsApi;

namespace GlazeWM.Domain.Windows
{
  public sealed class MinimizedWindow : Window
  {
    /// <inheritdoc />
    public override ContainerType Type { get; } = ContainerType.MinimizedWindow;

    public WindowType PreviousState;

    public MinimizedWindow(
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
