using System;
using GlazeWM.Domain.Common;
using GlazeWM.Infrastructure.WindowsApi;

namespace GlazeWM.Domain.Windows
{
  public sealed class FullscreenWindow : Window
  {
    /// <inheritdoc />
    public override ContainerType Type { get; } = ContainerType.FullscreenWindow;

    public FullscreenWindow(
      IntPtr handle,
      Rect floatingPlacement,
      RectDelta borderDelta
    ) : base(handle, floatingPlacement, borderDelta)
    {
    }
  }
}
