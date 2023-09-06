using System;
using GlazeWM.Infrastructure.WindowsApi;

namespace GlazeWM.Domain.Windows
{
  public sealed class FloatingWindow : Window
  {
    public override int Width => FloatingPlacement.Width;

    public override int Height => FloatingPlacement.Height;

    public override int X => FloatingPlacement.X;

    public override int Y => FloatingPlacement.Y;

    public FloatingWindow(
      IntPtr handle,
      Rect floatingPlacement,
      RectDelta borderDelta,
      Guid id = Guid.NewGuid()
    ) : base(handle, floatingPlacement, borderDelta, id, ContainerType.FloatingWindow)
    {
    }
  }
}
