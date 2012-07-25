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

  [Action]
  public override ExitCode compile() {
    return Console.batch("sbt compile", home: project + "\\scala");
  }

  [Action]
  public override ExitCode run(Arguments arguments) {
    // var url = Config.rawTarget == "" ? readln("Map url") : Config.rawTarget;
    var url = Config.rawTarget == "" ? "03" : Config.rawTarget;
    // var url = "03";
    return Console.batch("sbt \"chess " + url + " trace\"", home: project + "\\scala");
  }

  [Action]
  public ExitCode game(Arguments arguments) {
    var url = Config.rawTarget == "" ? readln("Map url") : Config.rawTarget;
    return Console.batch("sbt \"game " + url + " trace\"", home: project + "\\scala");
  }

  [Action]
  public override ExitCode runTest() {
    return runAllTests();
  }

  [Action]
  public ExitCode runAllTests() {
    // var algo = Config.rawTarget == "" ? readln("Algo") : Config.rawTarget;
    var algo = Config.rawTarget == "" ? "chess" : Config.rawTarget;
    // var algo = "chess";
    env["Meaningful"] = "1";
    return Console.batch("sbt \"tourney " + algo + "\"", home: project + "\\scala");
  }
}