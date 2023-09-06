using GlazeWM.Domain.Common;

namespace GlazeWM.Domain.Containers
{
  public sealed class RootContainer : Container
  {
    public RootContainer(Guid id = Guid.NewGuid()) : base(id, ContainerType.Root)
  }
}
