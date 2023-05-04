using System;

namespace GlazeWM.Domain.Common.Enums
{
  public enum Layout
  {
    Vertical,
    Horizontal,
  }

  public static class LayoutExtensions
  {
    public static Layout Inverse(this Layout Layout)
    {
      return Layout switch
      {
        Layout.Vertical => Layout.Horizontal,
        Layout.Horizontal => Layout.Vertical,
        _ => throw new ArgumentOutOfRangeException(nameof(Layout)),
      };
    }
  }
}
