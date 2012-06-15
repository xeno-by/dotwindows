// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

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
  protected override String transplantTo { get { return "%FAR_HOME%"; } }

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
    return Console.batch("nmake makefile_vc", home: project);
  }

  [Action]
  public virtual ExitCode run() {
    env["meaningful"] = "0";
    return Console.ui("Far.exe", home: project + @"\Release.32.vc");
  }

  [Action, MenuItem(description = "Deploy to Program Files (x86)", priority = 999.2)]
  public virtual ExitCode deploy() {
    var status = println("Deploying Far Manager 2.1 bis...");
    status = status && transplantFile("Release.32.vc/Far.exe", "Far.exe");
//    status = status && transplantFile("Release.32.vc/Far.map", "Far.map");
    return status;
  }
}