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

[Connector(name = "starr", priority = 999, description =
  "Wraps the development workflow of the stable mirror or project Kepler.")]

public class Starr : Kep {
  public override String project { get { return @"%PROJECTS%\Starr".Expand(); } }
  public override String profile { get { return "locker.unlock palo.bin"; } }

  public Starr() : base() {}
  public Starr(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public Starr(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}

  [Default, Action]
  public override ExitCode compile() {
    if (inPlayground || inTest) {
      var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
      return scala.compile();
    } else {
      var status = Console.batch("ant " + profile + " -buildfile build.xml", home: root);
      if (!status) return -1;

      println();
      println("Transplanting starr to Kepler...");
      status = status && transplantFile("build/palo/lib/scala-library.jar", "lib/scala-library.jar");
      status = status && transplantFile("build/palo/lib/scala-compiler.jar", "lib/scala-compiler.jar");
      return status;
    }
  }

  [Action]
  public override ExitCode run() {
    if (inPlayground || inTest) {
      var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
      return scala.run();
    } else {
      return new Kep().compile();
    }
  }

  private ExitCode transplantFile(String from, String to) {
    from = project + "\\" + from.Replace("/", "\\");
    to = new Kep().project + "\\" + to.Replace("/", "\\");
    print("  * Copying {0} to {1}... ", from, to);

    try {
      ExitCode status = -1;
      if (File.Exists(from)) status = CopyFile(from, to);
      if (status) println("[  OK  ]");
      return status;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  private static ExitCode CopyFile(string sourceFile, string destFile) {
    try {
      File.Copy(sourceFile, destFile, true);
      return 0;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  [Action]
  public override ExitCode deploy() {
    var status = Console.batch("git add *", home: root);
    status = status && Console.batch("git commit -m wip", home: root);
    status = status && Console.batch("git push", home: root);
    status = status && Console.batch("git pull origin topic/typetags/v2", home: new Kep().root);
    return status;
  }
}