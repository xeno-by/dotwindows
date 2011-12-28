// build this with "csc /t:exe /out:myke.exe /debug+ /r:System.Xml.Linq.dll myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

[Connector(name = "upstream", priority = 999, description =
  "Wraps the development workflow of upstream Scala.\r\n" +
  "Uses ant for building, itself for a repl, runs Reflection and doesn't support tests yet.")]

public class Upstream : Kep {
  public override String project { get { return @"%PROJECTS%\ScalaUpstream".Expand(); } }
  public override String profile { get { return "build"; } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public Upstream() : base() {}
  public Upstream(FileInfo file) : base(file) {}
  public Upstream(DirectoryInfo dir) : base(dir) {}
}