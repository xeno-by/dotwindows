// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

[Connector(name = "bis", priority = 999.1, description = "Compiles Far Manager 2.1 bis")]

public class Bis : Git {
  public override String project { get { return @"%DROPBOX%\Software\Windows\Far Manager\Sources".Expand(); } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  private Arguments arguments;
  public Bis() : base() {}
  public Bis(FileInfo file, Arguments arguments) : base(file) { this.arguments = arguments; }
  public Bis(DirectoryInfo dir, Arguments arguments) : base(dir) { this.arguments = arguments; }

  [Action, Default]
  public virtual ExitCode compile() {
    return Console.batch("nmake makefile_vc", home: root);
  }

  [Action]
  public virtual ExitCode run() {
    env["meaningful"] = "0";
    return Console.ui("Far.exe", home: root + @"\Release.32.vc");
  }
}