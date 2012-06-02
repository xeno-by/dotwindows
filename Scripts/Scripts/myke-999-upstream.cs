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

[Connector(name = "upstream", priority = 999, description =
  "Wraps the development workflow of upstream Scala.")]

public class Upstream : Kep {
  public override String project { get { return @"%PROJECTS%\ScalaUpstream".Expand(); } }
  public override String profile { get { return profileAlt; } }
  public override String profileClean { get { return profileAltClean; } }
  public override String profileLibrary { get { return profileAltLibrary; } }
  public override String profileReflect { get { return profileAltReflect; } }
  public override String profileCompiler { get { return profileAltCompiler; } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public Upstream() : base() {}
  public Upstream(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public Upstream(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}
}