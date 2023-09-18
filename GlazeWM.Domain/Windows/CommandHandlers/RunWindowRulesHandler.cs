using System;
using System.Linq;
using GlazeWM.Domain.Common.Utils;
using GlazeWM.Domain.Containers;
using GlazeWM.Domain.Containers.Commands;
using GlazeWM.Domain.Monitors;
using GlazeWM.Domain.UserConfigs;
using GlazeWM.Domain.UserConfigs.Commands;
using GlazeWM.Domain.Windows.Commands;
using GlazeWM.Domain.Windows.Events;
using GlazeWM.Domain.Workspaces;
using GlazeWM.Infrastructure.Bussing;
using GlazeWM.Infrastructure.WindowsApi;
using Microsoft.Extensions.Logging;
using static GlazeWM.Infrastructure.WindowsApi.WindowsApiService;

namespace GlazeWM.Domain.Windows.CommandHandlers
{
  internal sealed class RunWindowRulesHandler : ICommandHandler<RunWindowRulesCommand>
  {
    private readonly Bus _bus;
    private readonly ContainerService _containerService;
    private readonly ILogger<RunWindowRulesHandler> _logger;
    private readonly MonitorService _monitorService;
    private readonly WindowService _windowService;
    private readonly UserConfigService _userConfigService;

    public RunWindowRulesHandler(
      Bus bus,
      ContainerService containerService,
      ILogger<RunWindowRulesHandler> logger,
      MonitorService monitorService,
      WindowService windowService,
      UserConfigService userConfigService)
    {
      _bus = bus;
      _containerService = containerService;
      _logger = logger;
      _monitorService = monitorService;
      _windowService = windowService;
      _userConfigService = userConfigService;
    }

    public CommandResponse Handle(RunWindowRulesCommand command)
    {
      var windowHandle = command.WindowHandle;

      var (onManageRules, onDelayRules) = _userConfigService.GetWindowRules(window);
      var onManageRules = _userConfigService.GetWindowRules(window, RuleType.OnManage);

      var onManageCommands = onManageRules
        .SelectMany(rule => rule.CommandList)
        .Select(CommandParsingService.FormatCommand);

      // foreach (var onDelayRule in _userConfigService.onDelayRules)
      foreach (var onDelayRule in _userConfigService.GetWindowRules(RuleType.OnDelay))
      {
        // Promise.All()
      }

      return CommandResponse.Ok;
    }
  }
}
