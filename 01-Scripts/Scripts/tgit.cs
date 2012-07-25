// build this with "csc /t:winexe tgit.cs"

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    var cmd = args.ElementAtOrDefault(0);
    if (cmd == null) cmd = "log";
    cmd = String.Format("/command:\"{0}\"", cmd);

    var path = args.ElementAtOrDefault(1);
    if (path == null) path = ".";
    path = String.Format("/path:\"{0}\"", path);

    var tgit = @"C:\Program Files\TortoiseGit\bin\TortoiseProc.exe";
    var p = Process.Start(tgit, String.Join(",", new []{cmd, path}));

    if (cmd != "commit") {
      Thread.Sleep(100);
      MoveWindow(p.MainWindowHandle, 960, 0, 960, 1160, true);
      // MoveWindow(p.MainWindowHandle, 683, 0, 683, 728, true);
    }
  }

  [DllImport("user32.dll", SetLastError = true)]
  internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
}