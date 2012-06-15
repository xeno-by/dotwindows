// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "scratchpad", priority = 400, description = "Provides a builder for Scala scratchpad")]

public class Scratchpad : Git {
  private Arguments arguments;
  public Scratchpad() : base() {}
  public Scratchpad(FileInfo file, Arguments arguments) : base(file) { this.arguments = arguments; }
  public Scratchpad(DirectoryInfo dir, Arguments arguments) : base(dir) { this.arguments = arguments; }

  public override bool accept() {
    if (Config.verbose) println("scratchpad = {0}, dir = {1}", @"%DROPBOX%\Scratchpad\Scala".Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(@"%DROPBOX%\Scratchpad\Scala".Expand());
  }

  [Action]
  public virtual ExitCode clean() {
    dir.GetFiles("*.class").ToList().ForEach(file1 => file1.Delete());
    dir.GetFiles("*.log").ToList().ForEach(file1 => file1.Delete());
    return 0;
  }

  [Action]
  public virtual ExitCode rebuild() {
    var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
    return scala.rebuild();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
    return scala.compile();
  }

  [Action, Meaningful]
  public virtual ExitCode run() {
    var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
    return scala.run();
  }
}