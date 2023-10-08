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

namespace GlazeWM.Domain.Windows.CommandHandlers
{
  internal sealed class RunWindowRulesHandler : ICommandHandler<RunWindowRulesCommand>
  {
    private readonly Bus _bus;
    private readonly UserConfigService _userConfigService;

    public RunWindowRulesHandler(Bus bus, UserConfigService userConfigService)
    {
      _bus = bus;
      _userConfigService = userConfigService;
    }

    public CommandResponse Handle(RunWindowRulesCommand command)
    {
      var subjectContainer = command.SubjectContainer;
      var ruleTypes = command.RuleTypes;

      var windowRules = ruleTypes
        .Where(
          // Avoid running certain rules again when they should only be run once.
          ruleType => ruleType switch {
            FirstFocus or
            FirstTitleChange or
            Manage => !_userConfigService.HasRunRuleType(subjectContainer.Id),
            _ => true,
          }
        )
        .Select(ruleType => _userConfigService.GetWindowRules(windowRules, ruleType));

      // Return early if there are no window rules to run.
      if (!windowRules.Any())
        return CommandResponse.Ok;

      var windowRuleCommands = windowRules
        .SelectMany(rule => rule.CommandList)
        .Select(CommandParsingService.FormatCommand);

      _bus.Invoke(
        new RunWithSubjectContainerCommand(subjectContainer, windowRuleCommands)
      );

      _userConfigService.AddToRanRules(subjectContainer.Id, ruleTypes);

      return CommandResponse.Ok;
    }
  }
}
