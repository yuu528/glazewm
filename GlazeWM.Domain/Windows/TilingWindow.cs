using System;
using GlazeWM.Domain.Containers;
using GlazeWM.Infrastructure;
using GlazeWM.Infrastructure.WindowsApi;

namespace GlazeWM.Domain.Windows
{
  public sealed class TilingWindow : Window, IResizable
  {
    public double SizePercentage { get; set; }

    private readonly ContainerService _containerService =
      ServiceLocator.GetRequiredService<ContainerService>();

    public override int Width => _containerService.GetWidthOfResizableContainer(this);
    public override int Height => _containerService.GetHeightOfResizableContainer(this);
    public override int X => _containerService.GetXOfResizableContainer(this);
    public override int Y => _containerService.GetYOfResizableContainer(this);

    public TilingWindow(
      IntPtr handle,
      Rect floatingPlacement,
      RectDelta borderDelta,
      double sizePercentage = 1,
      Guid id = Guid.NewGuid()
    ) : base(handle, floatingPlacement, borderDelta, id, ContainerType.TilingWindow)
    {
      SizePercentage = sizePercentage;
    }
  }
}
