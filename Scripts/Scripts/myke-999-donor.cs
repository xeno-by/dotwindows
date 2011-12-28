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

[Connector(name = "donor", priority = 999, description =
  "Wraps the development workflow of donor scalacheck for Kepler")]

public class Donor : Kep {
  public override String project { get { return @"%PROJECTS%\Donor".Expand(); } }
  public override String profile { get { return "build"; } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public Donor() : base() {}
  public Donor(FileInfo file) : base(file) {}
  public Donor(DirectoryInfo dir) : base(dir) {}

  public static ExitCode CopyDirectory(string sourceFolder, string destFolder) {
    try {
      int iof = -1;
      while ((iof = sourceFolder.IndexOf(".jar")) != -1) {
        var archive = sourceFolder.Substring(0, iof + 4);
        var insideArchive = sourceFolder.Substring(iof + 4);
        var temp = Path.GetTempFileName() + ".unpack";
        var status = Console.batch(String.Format("unzip -qq \"{0}\" -d \"{1}\"", archive, temp));
        if (!status) { println("[FAILED]"); return status; }
        sourceFolder = temp + "\\" + insideArchive;
      }

      if (!Directory.Exists(destFolder))
        Directory.CreateDirectory(destFolder);

      var files = Directory.GetFiles(sourceFolder);
      foreach (var file in files) {
        var name = Path.GetFileName(file);
        var dest = Path.Combine(destFolder, name);
        File.Copy(file, dest);
      }

      var folders = Directory.GetDirectories(sourceFolder);
      foreach (var folder in folders) {
        var name = Path.GetFileName(folder);
        var dest = Path.Combine(destFolder, name);
        var status = CopyDirectory(folder, dest);
        if (!status) { println("[FAILED]"); return status; }
      }

      return 0;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  [Default, Action]
  public override ExitCode compile() {
    if (inPlayground && file != null) {
      return base.compile();
    } else {
      var status = Console.batch("ant " + profile + " -buildfile build.xml", home: root);
      if (!status) return -1;

      println();
      println("Transplanting partest to Kepler...");
      status = status && transplant("build/quick/classes/partest", "build/locker/classes/partest");
      status = status && transplant("build/quick/classes/library/scala/actors", "build/locker/classes/partest/scala/actors");
      status = status && transplant("build/quick/classes/scalap/scala/tools/scalap", "build/locker/classes/partest/scala/tools/scalap");
      status = status && transplant("lib/forkjoin.jar/scala/concurrent/forkjoin", "build/locker/classes/partest/scala/concurrent/forkjoin");
      status = status && transplant("lib/fjbg.jar/ch/epfl/lamp/fjbg", "build/locker/classes/partest/ch/epfl/lamp/fjbg");
      status = status && transplant("lib/fjbg.jar/ch/epfl/lamp/util", "build/locker/classes/partest/ch/epfl/lamp/util");
      status = status && transplant("lib/msil.jar/ch/epfl/lamp/compiler/msil", "build/locker/classes/partest/ch/epfl/lamp/compiler/msil");
      status = status && transplant("lib/jline.jar/org/fusesource", "build/locker/classes/partest/org/fusesource");
      status = status && transplant("lib/jline.jar/scala/tools/jline", "build/locker/classes/partest/scala/tools/jline");
      return status;
    }
  }

  public ExitCode transplant(String from, String to) {
    from = project + "\\" + from.Replace("/", "\\");
    to = new Kep().project + "\\" + to.Replace("/", "\\");
    print("  * Copying {0} to {1}... ", from, to);

    try {
      if (Directory.Exists(to)) Directory.Delete(to, true);
      var status = CopyDirectory(from, to);
      if (status) println("[  OK  ]");
      return status;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }
}