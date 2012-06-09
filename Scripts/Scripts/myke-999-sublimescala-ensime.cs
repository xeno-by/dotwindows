// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

[Connector(name = "sublimescala-ensime", priority = 999, description =
  "Wraps the development workflow of the Ensime subproject of the SublimeScala project.")]

public class SublimeScalaEnsime : Git {
  public override String project { get { return @"%PROJECTS%\SublimeScala\Ensime".Expand(); } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public SublimeScalaEnsime() : base() { init(); }
  public SublimeScalaEnsime(FileInfo file) : base(file) { init(); }
  public SublimeScalaEnsime(DirectoryInfo dir) : base(dir) { init(); }
  private void init() { env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]scala):([0-9]+)"; }

  [Action]
  public virtual ExitCode clean() {
    return Console.batch("sbt clean", home: root);
  }

  [Action]
  public virtual ExitCode rebuild() {
    return clean() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    var result = Console.batch("sbt stage", home: root);
    return result && transplantDir("dist_2.10.0-SNAPSHOT", @"%APPDATA%\Sublime Text 2\Packages\SublimeEnsime\server");
  }
}