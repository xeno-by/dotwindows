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

public class SublimeScalaEnsime : Sbt {
  public override String project { get { return @"%PROJECTS%\SublimeScala\Ensime".Expand(); } }

  public SublimeScalaEnsime() : base() {}
  public SublimeScalaEnsime(FileInfo file) : base(file) {}
  public SublimeScalaEnsime(DirectoryInfo dir) : base(dir) {}

  [Action]
  public override ExitCode compile() {
    // return base.compile();
    return deploy();
  }

  [Action, MenuItem(description = "Deploy to Sublime", priority = 999.2)]
  public virtual ExitCode deploy() {
    var result = Console.batch("sbt stage", home: project);
    return result && transplantDir("dist_2.10.0-SNAPSHOT", @"%APPDATA%\Sublime Text 2\Packages\SublimeEnsime\server");
  }
}