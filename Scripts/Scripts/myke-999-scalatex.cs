// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

[Connector(name = "scalatex", priority = 999, description =
  "Wraps the development workflow of project Scalatex")]

public class Scalatex : Git {
  public override String project { get { return @"%PROJECTS%\Scalatex".Expand(); } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public Scalatex() : base() { init(); }
  public Scalatex(FileInfo file) : base(file) { init(); }
  public Scalatex(DirectoryInfo dir) : base(dir) { init(); }
  private void init() { env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]scala):([0-9]+)"; }

  [Action]
  public virtual ExitCode clean() {
    dir.GetDirectories("scalatex").ToList().ForEach(subdir => subdir.Delete(true));
    return 0;
  }

  [Action]
  public virtual ExitCode rebuild() {
    return clean() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    var status = Console.batch("scalac Expression.scala LatexElements.scala");
    if (!status) return status;

    status = Console.batch("scalac Macros.scala MacroTest.scala");
    if (!status) return status;

    status = Console.batch("scalac Main.scala Scalatex.scala");
    if (!status) return status;

    return status;
  }
}