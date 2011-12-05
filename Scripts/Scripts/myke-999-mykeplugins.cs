// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;

[Connector(name = "myke-plugins", priority = 999.1, description =
  "Deploys myke plugins (Far Manager, Emacs).")]

public class MykePlugins : Git {
  public MykePlugins(FileInfo file) : base(file) {
  }

  public override bool accept() {
    return base.accept() && dir.IsChildOrEquivalentTo("%SCRIPTS_HOME%".Expand()) && file.Name.StartsWith("myke") && file.Extension != ".cs";
  }

  [Action]
  public virtual ExitCode compile() {
    Console.batch("regedit /s \"%SCRIPTS_HOME%\\myke-far.reg\"");
    Console.batch("copy \"%SCRIPTS_HOME%\\myke-backend.el\" \"%HOME%\\.emacs.d\\solutions\\myke-backend.el\" /Y");
    Console.batch("copy \"%SCRIPTS_HOME%\\myke-frontend.el\" \"%HOME%\\.emacs.d\\solutions\\myke-frontend.el\" /Y");
    Process.Start(@"%EMACS_HOME%\emacs.exe".Expand());
    return 0;
  }
}