using System.Linq;
using GlazeWM.Domain.UserConfigs;
using GlazeWM.Domain.UserConfigs.Commands;
using GlazeWM.Domain.Windows.Commands;
using GlazeWM.Infrastructure.Bussing;
using static GlazeWM.Domain.Common.WindowRuleType;

namespace GlazeWM.Domain.Windows.CommandHandlers
{
  internal sealed class RunWindowRulesHandler : ICommandHandler<RunWindowRulesCommand>
  {
    private readonly Bus _bus;
    private readonly UserConfigService _userConfigService;
    private readonly WindowService _windowService;

    public RunWindowRulesHandler(
      Bus bus,
      UserConfigService userConfigService,
      WindowService windowService)
    {
      _bus = bus;
      _userConfigService = userConfigService;
      _windowService = windowService;
    }

    public CommandResponse Handle(RunWindowRulesCommand command)
    {
      var subjectContainer = command.SubjectContainer;
      var ruleTypes = command.RuleTypes;

      var windowRules = ruleTypes
        .Where(
          // Avoid running certain rules again when they should only be run once.
          ruleType => ruleType switch
          {
            FirstFocus or
              FirstTitleChange or
              Manage => !_windowService.HasRanRuleType(subjectContainer as Window, ruleType),
            _ => true,
          }
        )
        .Select(ruleType => _userConfigService.GetWindowRules(subjectContainer as Window, ruleType));

      // Return early if there are no window rules to run.
      if (!windowRules.Any())
        return CommandResponse.Ok;

      var windowRuleCommands = windowRules
        .SelectMany(rule => rule.CommandList)
        .Select(CommandParsingService.FormatCommand);

      _bus.Invoke(
        new RunWithSubjectContainerCommand(windowRuleCommands, subjectContainer)
      );

      foreach (var ruleType in ruleTypes)
        _windowService.AddRanRuleType(subjectContainer as Window, ruleType);

      return CommandResponse.Ok;
    }
  }
}
