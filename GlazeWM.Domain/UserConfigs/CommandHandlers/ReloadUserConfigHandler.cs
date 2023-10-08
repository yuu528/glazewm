using System.Linq;
using GlazeWM.Domain.Containers;
using GlazeWM.Domain.Containers.Commands;
using GlazeWM.Domain.UserConfigs.Commands;
using GlazeWM.Domain.UserConfigs.Events;
using GlazeWM.Domain.Windows;
using GlazeWM.Domain.Windows.Commands;
using GlazeWM.Domain.Workspaces.Commands;
using GlazeWM.Infrastructure.Bussing;

namespace GlazeWM.Domain.UserConfigs.CommandHandlers
{
  internal sealed class ReloadUserConfigHandler : ICommandHandler<ReloadUserConfigCommand>
  {
    private readonly Bus _bus;
    private readonly ContainerService _containerService;
    private readonly WindowService _windowService;

    public ReloadUserConfigHandler(
      Bus bus,
      ContainerService containerService,
      WindowService windowService)
    {
      _bus = bus;
      _containerService = containerService;
      _windowService = windowService;
    }

    public CommandResponse Handle(ReloadUserConfigCommand command)
    {
      // Re-evaluate user config file and set its values in state.
      _bus.Invoke(new EvaluateUserConfigCommand());

      _bus.Invoke(new UpdateWorkspacesFromConfigCommand(_userConfigService.WorkspaceConfigs));

      // Run matching window rules.
      foreach (var window in _windowService.GetWindows())
        _bus.Invoke(new RunWindowRulesCommand(
          window,
          new List<WindowRuleType>() { WindowRuleType.OnManage }
        ));

      // Redraw full container tree.
      _containerService.ContainersToRedraw.Add(_containerService.ContainerTree);

      _bus.Emit(new UserConfigReloadedEvent());

      return CommandResponse.Ok;
    }
  }
}
