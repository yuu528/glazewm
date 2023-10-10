using System;
using System.Collections.Generic;
using GlazeWM.Domain.Common;
using GlazeWM.Domain.Containers;
using GlazeWM.Infrastructure.Bussing;

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
