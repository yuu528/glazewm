using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using static LarsWM.Infrastructure.WindowsApi.WindowsApiService;
using LarsWM.Infrastructure.WindowsApi;

namespace LarsWM.Bar
{
  public class AppBarWindow : Window
  {
    private bool IsAppBarRegistered;
    private bool IsInAppBarResize;

    static AppBarWindow()
    {
      ShowInTaskbarProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(false));
      MinHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
      MinWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
      MaxHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
      MaxWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
    }

    public AppBarWindow()
    {
      this.WindowStyle = WindowStyle.None;
      this.ResizeMode = ResizeMode.NoResize;
      this.Topmost = true;
    }

    public AppBarDockMode DockMode
    {
      get { return (AppBarDockMode)GetValue(DockModeProperty); }
      set { SetValue(DockModeProperty, value); }
    }

    public static readonly DependencyProperty DockModeProperty =
        DependencyProperty.Register("DockMode", typeof(AppBarDockMode), typeof(AppBarWindow),
            new FrameworkPropertyMetadata(AppBarDockMode.Left, DockLocation_Changed));

    public MonitorInfo Monitor
    {
      get { return (MonitorInfo)GetValue(MonitorProperty); }
      set { SetValue(MonitorProperty, value); }
    }

    public static readonly DependencyProperty MonitorProperty =
        DependencyProperty.Register("Monitor", typeof(MonitorInfo), typeof(AppBarWindow),
            new FrameworkPropertyMetadata(null, DockLocation_Changed));

    public int DockedWidthOrHeight
    {
      get { return (int)GetValue(DockedWidthOrHeightProperty); }
      set { SetValue(DockedWidthOrHeightProperty, value); }
    }

    public static readonly DependencyProperty DockedWidthOrHeightProperty =
        DependencyProperty.Register("DockedWidthOrHeight", typeof(int), typeof(AppBarWindow),
            new FrameworkPropertyMetadata(200, DockLocation_Changed, DockedWidthOrHeight_Coerce));

    private static object DockedWidthOrHeight_Coerce(DependencyObject d, object baseValue)
    {
      var @this = (AppBarWindow)d;
      var newValue = (int)baseValue;

      switch (@this.DockMode)
      {
        case AppBarDockMode.Left:
        case AppBarDockMode.Right:
          return BoundIntToDouble(newValue, @this.MinWidth, @this.MaxWidth);

        case AppBarDockMode.Top:
        case AppBarDockMode.Bottom:
          return BoundIntToDouble(newValue, @this.MinHeight, @this.MaxHeight);

        default: throw new NotSupportedException();
      }
    }

    private static int BoundIntToDouble(int value, double min, double max)
    {
      if (min > value)
      {
        return (int)Math.Ceiling(min);
      }
      if (max < value)
      {
        return (int)Math.Floor(max);
      }

      return value;
    }

    private static void MinMaxHeightWidth_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      d.CoerceValue(DockedWidthOrHeightProperty);
    }

    private static void DockLocation_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var @this = (AppBarWindow)d;

      if (@this.IsAppBarRegistered)
      {
        @this.OnDockLocationChanged();
      }
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
      base.OnDpiChanged(oldDpi, newDpi);

      OnDockLocationChanged();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
      base.OnSourceInitialized(e);

      // add the hook, setup the appbar
      var source = (HwndSource)PresentationSource.FromVisual(this);

      if (!ShowInTaskbar)
      {
        var exstyle = (ulong)GetWindowLongPtr(source.Handle, GWL_EXSTYLE);
        exstyle |= (ulong)((uint)WS_EX.WS_EX_TOOLWINDOW);
        SetWindowLongPtr(source.Handle, GWL_EXSTYLE, unchecked((IntPtr)exstyle));
      }

      source.AddHook(WndProc);

      var appBarData = GetAppBarData();
      SHAppBarMessage(AppBarMessage.NEW, ref appBarData);

      // set our initial location
      this.IsAppBarRegistered = true;
      OnDockLocationChanged();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);

      if (e.Cancel)
      {
        return;
      }

      if (IsAppBarRegistered)
      {
        var abd = GetAppBarData();
        SHAppBarMessage(AppBarMessage.REMOVE, ref abd);
        IsAppBarRegistered = false;
      }
    }

    private int WpfDimensionToDesktop(double dim)
    {
      var dpi = VisualTreeHelper.GetDpi(this);

      return (int)Math.Ceiling(dim * dpi.PixelsPerDip);
    }

    private double DesktopDimensionToWpf(double dim)
    {
      var dpi = VisualTreeHelper.GetDpi(this);

      return dim / dpi.PixelsPerDip;
    }

    private void OnDockLocationChanged()
    {
      if (IsInAppBarResize)
      {
        return;
      }

      var appBarData = GetAppBarData();
      appBarData.rc = (Rectangle)GetSelectedMonitor().ViewportBounds;

      SHAppBarMessage(AppBarMessage.QUERYPOS, ref appBarData);

      var dockedWidthOrHeightInDesktopPixels = WpfDimensionToDesktop(DockedWidthOrHeight);
      switch (DockMode)
      {
        case AppBarDockMode.Top:
          appBarData.rc.Bottom = appBarData.rc.Top + dockedWidthOrHeightInDesktopPixels;
          break;
        case AppBarDockMode.Bottom:
          appBarData.rc.Top = appBarData.rc.Bottom - dockedWidthOrHeightInDesktopPixels;
          break;
        case AppBarDockMode.Left:
          appBarData.rc.Right = appBarData.rc.Left + dockedWidthOrHeightInDesktopPixels;
          break;
        case AppBarDockMode.Right:
          appBarData.rc.Left = appBarData.rc.Right - dockedWidthOrHeightInDesktopPixels;
          break;
        default: throw new NotSupportedException();
      }

      SHAppBarMessage(AppBarMessage.SETPOS, ref appBarData);
      IsInAppBarResize = true;
      try
      {
        WindowBounds = appBarData.rc;
      }
      finally
      {
        IsInAppBarResize = false;
      }
    }

    private MonitorInfo GetSelectedMonitor()
    {
      var monitor = this.Monitor;
      var allMonitors = MonitorInfo.GetAllMonitors();
      if (monitor == null || !allMonitors.Contains(monitor))
      {
        monitor = allMonitors.First(f => f.IsPrimary);
      }

      return monitor;
    }

    private AppBarData GetAppBarData()
    {
      return new AppBarData()
      {
        cbSize = Marshal.SizeOf(typeof(AppBarData)),
        hWnd = new WindowInteropHelper(this).Handle,
        uCallbackMessage = AppBarMessageId,
        uEdge = (int)DockMode
      };
    }

    private static int _AppBarMessageId;
    public static int AppBarMessageId
    {
      get
      {
        if (_AppBarMessageId == 0)
        {
          _AppBarMessageId = RegisterWindowMessage("AppBarMessage_EEDFB5206FC3");
        }

        return _AppBarMessageId;
      }
    }

    public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg == WindowMessage.WINDOWPOSCHANGING && !IsInAppBarResize)
      {
        var wp = Marshal.PtrToStructure<WindowPos>(lParam);
        wp.flags |= SWP.SWP_NOMOVE | SWP.SWP_NOSIZE;
        Marshal.StructureToPtr(wp, lParam, false);
      }
      else if (msg == WindowMessage.ACTIVATE)
      {
        var abd = GetAppBarData();
        SHAppBarMessage(AppBarMessage.ACTIVATE, ref abd);
      }
      else if (msg == WindowMessage.WINDOWPOSCHANGED)
      {
        var abd = GetAppBarData();
        SHAppBarMessage(AppBarMessage.WINDOWPOSCHANGED, ref abd);
      }
      else if (msg == AppBarMessageId)
      {
        switch ((ABN)(int)wParam)
        {
          case ABN.POSCHANGED:
            OnDockLocationChanged();
            handled = true;
            break;
        }
      }

      return IntPtr.Zero;
    }

    private Rectangle WindowBounds
    {
      set
      {
        this.Left = DesktopDimensionToWpf(value.Left);
        this.Top = DesktopDimensionToWpf(value.Top);
        this.Width = DesktopDimensionToWpf(value.Width);
        this.Height = DesktopDimensionToWpf(value.Height);
      }
    }
  }

  public enum AppBarDockMode
  {
    Left,
    Top,
    Right,
    Bottom
  }
}
