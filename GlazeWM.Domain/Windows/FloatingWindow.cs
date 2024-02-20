using System;
using GlazeWM.Domain.Common;
using GlazeWM.Infrastructure.WindowsApi;

namespace GlazeWM.Domain.Windows
{
  public sealed class FloatingWindow : Window
  {
    /// <inheritdoc />
    public override ContainerType Type { get; } = ContainerType.FloatingWindow;

    public override int Width => FloatingPlacement.Width;

    public override int Height => FloatingPlacement.Height;

    public override int X => FloatingPlacement.X;

    public override int Y => FloatingPlacement.Y;

    public FloatingWindow(
      IntPtr handle,
      Rect floatingPlacement,
      RectDelta borderDelta
    ) : base(handle, floatingPlacement, borderDelta)
    {
    }
  }
}
