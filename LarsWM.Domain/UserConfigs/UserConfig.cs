﻿using System;
using System.Collections.Generic;

namespace LarsWM.Domain.UserConfigs
{
  public class UserConfig
  {
    public Guid Id = Guid.NewGuid();

    // TODO: allow regular expressions
    // eg. for WMP's "now playing" toolbar: StartsWith("WMP9MediaBarFlyout"))
    public List<string> WindowClassesToIgnore = new List<string> {
            // Tray on primary screen
            "Shell_TrayWnd",
            // Trays on secondary screens
            "Shell_SecondaryTrayWnd",
            // ??
            "TaskManagerWindow",
            // Microsoft Text Framework service IME
            "MSCTFIME UI",
            // Desktop window (holds wallpaper & desktop icons)
            "SHELLDLL_DefView",
            // Background for lock screen
            "LockScreenBackstopFrame",
            // ??
            "Progman",
            // Windows 7 open Start Menu
            "DV2ControlHost",
            // Some Windows 8 thing
            "Shell_CharmWindow",

            /*
             * Consider adding:
             * "MsgrIMEWindowClass", // Window live messenger notification
             * "SysShadow", // Windows live messenger shadow-hack
             * "Button", // UI component, e.g. Start Menu button
             * "Frame Alternate Owner", // Edge
             * "MultitaskingViewFrame", // The original Win + Tab view
             */
        };

    public List<string> ProcessNamesToIgnore = new List<string> {
            "SearchUI",
            "ShellExperienceHost",
            "LockApp",
            "PeopleExperienceHost",
            "StartMenuExperienceHost",
        };

    public int InnerGap = 20;
    public int OuterGap = 20;

    public double ResizePercentage { get; set; } = 0.02;
  }
}
