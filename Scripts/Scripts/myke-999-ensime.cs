// build this with "csc /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

[Connector(name = "Ensime", priority = 999, description =
  "Wraps the development workflow of project Ensime.")]

public class Ensime : Git {
  public override String project { get { return @"%PROJECTS%\Ensime".Expand(); } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public Ensime() : base() { init(); }
  public Ensime(FileInfo file) : base(file) { init(); }
  public Ensime(DirectoryInfo dir) : base(dir) { init(); }
  private void init() { env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]scala):([0-9]+)"; }

  [Action]
  public virtual ExitCode clean() {
    return Console.batch("sbt clean", home: root);
  }

  [Action]
  public virtual ExitCode rebuild() {
    return Console.batch("sbt clean stage", home: root);
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("sbt stage", home: root);
  }
}