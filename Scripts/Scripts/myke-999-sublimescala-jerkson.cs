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

[Connector(name = "sublimescala-jerkson", priority = 999, description =
  "Wraps the development workflow of the Jerkson subproject of the SublimeScala project.")]

public class SublimeScalaJerkson : Maven {
  public override String project { get { return @"%PROJECTS%\SublimeScala\Jerkson".Expand(); } }

  public SublimeScalaJerkson() : base() {}
  public SublimeScalaJerkson(FileInfo file, Arguments arguments) : base(file) {}
  public SublimeScalaJerkson(DirectoryInfo dir, Arguments arguments) : base(dir) {}
}