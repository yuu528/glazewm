using System.Linq;
using GlazeWM.Domain.Common.Enums;
using GlazeWM.Domain.Containers.Commands;
using GlazeWM.Domain.Containers.Events;
using GlazeWM.Domain.Windows;
using GlazeWM.Domain.Workspaces;
using GlazeWM.Infrastructure.Bussing;

namespace GlazeWM.Domain.Containers.CommandHandlers
{
  internal sealed class ChangeContainerLayoutHandler : ICommandHandler<ChangeContainerLayoutCommand>
  {
    private readonly Bus _bus;
    private readonly ContainerService _containerService;

    public ChangeContainerLayoutHandler(Bus bus, ContainerService containerService)
    {
      _bus = bus;
      _containerService = containerService;
    }

    public CommandResponse Handle(ChangeContainerLayoutCommand command)
    {
      var container = command.Container;
      var newLayout = command.NewLayout;

      var layoutContainer = container is SplitContainer
        ? container as SplitContainer
        : container.Parent as SplitContainer;

      var currentLayout = layoutContainer.Layout;

      if (currentLayout == newLayout)
        return CommandResponse.Ok;

      layoutContainer.Layout = newLayout;

      var inverseLayoutChildren = layoutContainer.Children
        .OfType<SplitContainer>()
        .Where(child => child.Layout != newLayout);

      // Flatten any child split containers with the same layout as the layout container.
      foreach (var inverseLayoutChild in inverseLayoutChildren)
        _bus.Invoke(new FlattenSplitContainerCommand(inverseLayoutChild));

      _containerService.ContainersToRedraw.Add(layoutContainer);

      _bus.Emit(new LayoutChangedEvent(newLayout));

      return CommandResponse.Ok;
    }

    private void ChangeWindowLayout(Window window, Layout newLayout)
    {
      var parent = window.Parent as SplitContainer;
      var currentLayout = parent.Layout;

      if (currentLayout == newLayout)
        return;

      var hasResizableSiblings = window.SiblingsOfType<IResizable>().Any();

      // If the window is an only child of a workspace, change layout of the workspace.
      if (window.Parent is Workspace && !hasResizableSiblings)
      {
        ChangeWorkspaceLayout(parent as Workspace, newLayout);
        return;
      }

      // If the window is an only child and the parent is a normal split container, then flatten
      // the split container.
      if (!hasResizableSiblings)
      {
        _bus.Invoke(new FlattenSplitContainerCommand(parent));
        return;
      }

      // Create a new split container to wrap the window.
      var splitContainer = new SplitContainer
      {
        Layout = newLayout,
      };

      // Replace the window with the wrapping split container. The window has to be attached to
      // the split container after the replacement.
      _bus.Invoke(new ReplaceContainerCommand(splitContainer, parent, window.Index));

      // The child window takes up the full size of its parent split container.
      (window as IResizable).SizePercentage = 1;
      _bus.Invoke(new DetachContainerCommand(window));
      _bus.Invoke(new AttachContainerCommand(window, splitContainer));
    }

    private void ChangeWorkspaceLayout(Workspace workspace, Layout newLayout)
    {
      var currentLayout = workspace.Layout;

      if (currentLayout == newLayout)
        return;

      workspace.Layout = newLayout;

      // Flatten any top-level split containers with the same layout as the workspace. Clone the
      // list since the number of workspace children changes when split containers are flattened.
      foreach (var child in workspace.Children.ToList())
      {
        var childSplitContainer = child as SplitContainer;

        if (childSplitContainer?.Layout != newLayout)
          continue;

        _bus.Invoke(new FlattenSplitContainerCommand(childSplitContainer));
      }

      _containerService.ContainersToRedraw.Add(workspace);
    }
  }
}
