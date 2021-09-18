using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using static LarsWM.Infrastructure.WindowsApi.WindowsApiService;
using LarsWM.Infrastructure.WindowsApi;
using LarsWM.Domain.Monitors;

namespace LarsWM.Bar
{
  public enum AppBarPosition
  {
    Left,
    Top,
    Right,
    Bottom
  }

  public class AppBarWindow : Window
  {
    private Monitor _monitor = null;
    private bool _isAppBarRegistered = false;
    private bool _isInAppBarResize = false;

    static AppBarWindow()
    {
      MinHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
      MinWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
      MaxHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
      MaxWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
    }

    public AppBarWindow(Monitor monitor)
    {
      _monitor = monitor;
      WindowStyle = WindowStyle.None;
      ResizeMode = ResizeMode.NoResize;
      Topmost = true;
      ShowInTaskbar = false;
    }

    public AppBarPosition DockMode
    {
      get { return (AppBarPosition)GetValue(DockModeProperty); }
      set { SetValue(DockModeProperty, value); }
    }

    public static readonly DependencyProperty DockModeProperty =
      DependencyProperty.Register("DockMode", typeof(AppBarPosition), typeof(AppBarWindow),
      new FrameworkPropertyMetadata(AppBarPosition.Left, DockLocation_Changed));

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
        case AppBarPosition.Left:
        case AppBarPosition.Right:
          return BoundIntToDouble(newValue, @this.MinWidth, @this.MaxWidth);

        case AppBarPosition.Top:
        case AppBarPosition.Bottom:
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

      if (@this._isAppBarRegistered)
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
      _isAppBarRegistered = true;
      OnDockLocationChanged();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);

      if (e.Cancel)
      {
        return;
      }

      if (_isAppBarRegistered)
      {
        var abd = GetAppBarData();
        SHAppBarMessage(AppBarMessage.REMOVE, ref abd);
        _isAppBarRegistered = false;
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
      if (_isInAppBarResize)
      {
        return;
      }

      var appBarData = GetAppBarData();

      var monitorRect = new Rectangle()
      {
        Left = _monitor.Screen.WorkingArea.Left,
        Right = _monitor.Screen.WorkingArea.Right,
        Top = _monitor.Screen.WorkingArea.Top,
        Bottom = _monitor.Screen.WorkingArea.Bottom,
      };

      appBarData.rc = monitorRect;

      SHAppBarMessage(AppBarMessage.QUERYPOS, ref appBarData);

      var dockedWidthOrHeightInDesktopPixels = WpfDimensionToDesktop(DockedWidthOrHeight);
      switch (DockMode)
      {
        case AppBarPosition.Top:
          appBarData.rc.Bottom = appBarData.rc.Top + dockedWidthOrHeightInDesktopPixels;
          break;
        case AppBarPosition.Bottom:
          appBarData.rc.Top = appBarData.rc.Bottom - dockedWidthOrHeightInDesktopPixels;
          break;
        case AppBarPosition.Left:
          appBarData.rc.Right = appBarData.rc.Left + dockedWidthOrHeightInDesktopPixels;
          break;
        case AppBarPosition.Right:
          appBarData.rc.Left = appBarData.rc.Right - dockedWidthOrHeightInDesktopPixels;
          break;
        default: throw new NotSupportedException();
      }

      SHAppBarMessage(AppBarMessage.SETPOS, ref appBarData);
      _isInAppBarResize = true;
      try
      {
        WindowBounds = appBarData.rc;
      }
      finally
      {
        _isInAppBarResize = false;
      }
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

    private int _AppBarMessageId;
    public int AppBarMessageId
    {
      get
      {
        if (_AppBarMessageId == 0)
        {
          _AppBarMessageId = RegisterWindowMessage($"AppBarMessage_{_monitor.Name}");
        }

        return _AppBarMessageId;
      }
    }

    public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg == (int)WindowMessage.WINDOWPOSCHANGING && !_isInAppBarResize)
      {
        var wp = Marshal.PtrToStructure<WindowPos>(lParam);
        wp.flags |= SWP.SWP_NOMOVE | SWP.SWP_NOSIZE;
        Marshal.StructureToPtr(wp, lParam, false);
      }
      else if (msg == (int)WindowMessage.ACTIVATE)
      {
        var abd = GetAppBarData();
        SHAppBarMessage(AppBarMessage.ACTIVATE, ref abd);
      }
      else if (msg == (int)WindowMessage.WINDOWPOSCHANGED)
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
        Left = DesktopDimensionToWpf(value.Left);
        Top = DesktopDimensionToWpf(value.Top);
        Width = DesktopDimensionToWpf(value.Right - value.Left);
        Height = DesktopDimensionToWpf(value.Bottom - value.Top);
      }
    }
  }
}
