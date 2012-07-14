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

[Connector(name = "icfpc", priority = 999)]

public class Icfpc : Sbt {
  public override String project { get { return @"%PROJECTS%\ICFP".Expand(); } }

  public Icfpc() : base() {}
  public Icfpc(FileInfo file) : base(file) {}
  public Icfpc(DirectoryInfo dir) : base(dir) {}

  public override ExitCode compile() {
    return Console.batch("sbt compile", home: project + "\\scala");
  }

  public override ExitCode run(Arguments arguments) {
    var url = readln("Map url");
    return Console.batch("sbt \"game " + url + "\"", home: project + "\\scala");
  }

  public override ExitCode runTest() {
    return Console.batch("sbt our-test", home: project + "\\scala");
  }
}