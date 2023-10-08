namespace GlazeWM.Domain.Common
{
  public enum WindowRuleType
  {
    /// <summary>
    /// When a window is initially managed.
    /// </summary>
    Manage,

    /// <summary>
    /// When a window receives native focus.
    /// </summary>
    Focus,

    /// <summary>
    /// When a window initially receives native focus.
    /// </summary>
    FirstFocus,

    /// <summary>
    /// When the title of a window changes.
    /// </summary>
    TitleChange,

    /// <summary>
    /// When the title of a initialy window changes.
    /// </summary>
    FirstTitleChange,
  }
}
