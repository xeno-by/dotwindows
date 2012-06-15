// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

[Connector(name = "ruby", priority = 210, description =
  "Handles ruby programs.")]

public class Ruby : Git {
  public Ruby(FileInfo file) : base(file) { init(); }
  private void init() { env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]rb):([0-9]+)"; }

  public override bool accept() {
    return file.Extension == ".rb";
  }

  [Action, Meaningful]
  public virtual ExitCode run() {
    return Console.batch("ruby " + file.Name, home: dir);
  }

  [Action, DontTrace]
  public virtual ExitCode repl() {
    return Console.interactive("ruby " + file.Name, home: dir);
  }
}