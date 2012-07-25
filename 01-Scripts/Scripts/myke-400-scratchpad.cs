// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "scratchpad", priority = 400, description = "Provides a builder for Scala scratchpad")]

public class Scratchpad : Scala {
  public Scratchpad(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public Scratchpad(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}

  public override bool accept() {
    if (Config.verbose) println("scratchpad = {0}, dir = {1}", @"%DROPBOX%\Scratchpad\Scala".Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(@"%DROPBOX%\Scratchpad\Scala".Expand());
  }
}