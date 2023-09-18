using System;
using GlazeWM.Domain.Containers;

namespace GlazeWM.Domain.Windows.Commands
{
  public class RunWindowRulesCommand : Command
  {
    public Container SubjectContainer { get; }
    public List<WindowRuleType> RuleTypes { get; }

    public RunWindowRulesCommand(
      Container subjectContainer,
      List<WindowRuleType> ruleTypes)
    {
      SubjectContainer = subjectContainer;
      RuleTypes = ruleTypes;
    }
  }
}
