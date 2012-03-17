// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

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
    if (file != null) {
      var lines = new Lines(file, File.ReadAllLines(file.FullName).ToList());
      var scala = new Scala(file, lines, arguments);
      return scala.rebuild();
    } else {
      println("rebuild: not implemented when invoked upon the entire scratchpad");
      return -1;
    }
  }

  [Default, Action]
  public virtual ExitCode compile() {
    if (file != null) {
      var lines = new Lines(file, File.ReadAllLines(file.FullName).ToList());
      var scala = new Scala(file, lines, arguments);
      return scala.compile();
    } else {
      println("compile: not implemented when invoked upon the entire scratchpad");
      return -1;
    }
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    if (file != null) {
      var lines = new Lines(file, File.ReadAllLines(file.FullName).ToList());
      var scala = new Scala(file, lines, arguments);
      return scala.run(arguments);
    } else {
      println("run: not implemented when invoked upon the entire scratchpad");
      return -1;
    }
  }
}