using System.ComponentModel.DataAnnotations;

namespace GlazeWM.Domain.UserConfigs
{
  public class WorkspaceConfig
  {
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// If a monitor index is provided, change to device name format (eg. "\\.\DISPLAY1").
    /// </summary>
    private string _bindToMonitor;
    public string BindToMonitor
    {
      get => _bindToMonitor;
      set => _bindToMonitor = int.TryParse(value, out var monitorIndex)
        ? $@"\\.\DISPLAY{monitorIndex}"
        : value;
    }

    public string DisplayName { get; set; }
    public bool KeepAlive { get; set; }
  }
}
