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
using ZetaLongPaths;

[Connector(name = "scala-trunk", priority = 999, description =
  "Wraps the development workflow of a dedicated trunk project for Kepler")]
public class ScalaTrunk : Kep {
  public ScalaTrunk() : base() { }
  public ScalaTrunk(FileInfo file, Arguments arguments) : base(file, arguments) { this.arguments = arguments; }
  public ScalaTrunk(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) { this.arguments = arguments; }

  public override String project { get { return @"%PROJECTS%\Scala".Expand(); } }
  public override String profile { get { return profileAlt; } }
  public override String profileClean { get { return profileAltClean; } }
  public override String profileLibrary { get { return profileAltLibrary; } }
  public override String profileReflect { get { return profileAltReflect; } }
  public override String profileCompiler { get { return profileAltCompiler; } }
}