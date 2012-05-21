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
using ZetaLongPaths;

[Connector(name = "donor", priority = 999, description =
  "Wraps the development workflow of scalatest donor for Kepler")]
public class Donor : Kep {
  public bool kur = false;
  public String donneeName() { return kur ? "Kur" : "Kepler"; }
  public Prj donnee() { return kur ? new Kur() : new Kep(); }
  public override String project { get { return kur ? @"%PROJECTS%\DonorUnderRefactoring".Expand() : @"%PROJECTS%\Donor".Expand(); } }

  public override String profile { get { return profileAlt; } }
  public override String profileClean { get { return profileAltClean; } }
  public override String profileLibrary { get { return profileAltLibrary; } }
  public override String profileCompiler { get { return profileAltCompiler; } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public Donor(bool kur = false) : base() { this.kur = kur; }
  public Donor(FileInfo file, Arguments arguments, bool kur = false) : base(file, arguments) { this.arguments = arguments; this.kur = kur; }
  public Donor(DirectoryInfo dir, Arguments arguments, bool kur = false) : base(dir, arguments) { this.arguments = arguments; this.kur = kur; }

  [Default, Action]
  public override ExitCode compile() {
    if (inPlayground || inTest) {
      return base.compile();
    } else {
      var status = Console.batch("ant " + profile + " -buildfile build.xml", home: root);
      if (!status) return -1;

      println();
      println("Transplanting partest to " + donneeName() + "...");
      status = status && transplantDir("build/quick/classes/partest", "build/locker/classes/partest");
      status = status && transplantDir("build/quick/classes/library/scala/actors", "build/locker/classes/library/scala/actors");
      status = status && transplantDir("build/quick/classes/scalap/scala/tools/scalap", "build/locker/classes/partest/scala/tools/scalap");
      status = status && transplantDir("build/quick/classes/scalacheck/org/scalacheck", "build/locker/classes/partest/org/scalacheck");
      status = status && transplantFile("build/pack/misc/scala-devel/plugins/continuations.jar", "build/locker/classes/continuations.jar");
      status = status && transplantDir("build/pack/misc/scala-devel/plugins/continuations.jar", "build/locker/classes/continuations");
      status = status && transplantDir("build/quick/classes/library/scala/util/continuations", "build/locker/classes/library/scala/util/continuations");
      status = status && transplantDir("lib/forkjoin.jar/scala/concurrent/forkjoin", "build/locker/classes/library/scala/concurrent/forkjoin");
      status = status && transplantDir("lib/fjbg.jar/ch/epfl/lamp/fjbg", "build/locker/classes/compiler/ch/epfl/lamp/fjbg");
      status = status && transplantDir("lib/fjbg.jar/ch/epfl/lamp/util", "build/locker/classes/compiler/ch/epfl/lamp/util");
      status = status && transplantDir("lib/msil.jar/ch/epfl/lamp/compiler/msil", "build/locker/classes/compiler/ch/epfl/lamp/compiler/msil");
      status = status && transplantDir("lib/jline.jar/org/fusesource", "build/locker/classes/library/org/fusesource");
      status = status && transplantDir("lib/jline.jar/scala/tools/jline", "build/locker/classes/library/scala/tools/jline");

      if (status) {
        println();
        println("Calculating longest path lengths in " + donneeName() + "...");
        var locker = new ZlpDirectoryInfo(donnee().project + "\\build\\locker" );
        var classes = locker.GetFiles("*.class", SearchOption.AllDirectories).ToList();
        classes = classes.OrderByDescending(f => f.FullName.Length).ToList();
        classes = classes.Take(5).Concat(classes.Skip(5).Where(f => f.FullName.Length > 260)).ToList();
        classes.ForEach(f => println(String.Format("  * {0} {1}", f.FullName.Length, f.FullName)));
        if (classes[0].FullName.Length > 260) {
          println("WARNING: THERE ARE FILES WITH NAMES LONGER THAN 260 CHARACTERS, JVM WON'T BE ABLE TO LOAD THEM UNLESS THEY ARE PACKED INTO A JAR");
          //return -1;
        }

        println();
        println("Packing locker into jars...");
        var classesDir = donnee().project + @"\build\locker\classes";
        var jarDir = donnee().project + @"\build\locker\lib";
        if (!Directory.Exists(jarDir)) Directory.CreateDirectory(jarDir);
        status = status && print("  * scala-compiler... ") && Console.batch("jar cf ../lib/scala-compiler.jar -C compiler .", home: classesDir) && println("[  OK  ]");
        status = status && print("  * scala-library... ") && Console.batch("jar cf ../lib/scala-library.jar -C library .", home: classesDir) && println("[  OK  ]");
        status = status && print("  * scala-partest... ") && Console.batch("jar cf ../lib/scala-partest.jar -C partest .", home: classesDir) && println("[  OK  ]");
        if (!status) println("[FAILED]");
      }

      return status;
    }
  }

  public ExitCode transplantFile(String from, String to) {
    from = project + "\\" + from.Replace("/", "\\");
    // var to5 = new Kep5().project + "\\" + to.Replace("/", "\\");
    to = donnee().project + "\\" + to.Replace("/", "\\");
    print("  * Copying {0} to {1}... ", from, to);

    try {
      ExitCode status = -1;
      // if (File.Exists(from)) status = CopyFile(from, to) && CopyFile(from, to5);
      if (File.Exists(from)) status = CopyFile(from, to);
      if (status) println("[  OK  ]");
      return status;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  public static ExitCode CopyFile(string sourceFile, string destFile) {
    try {
      File.Copy(sourceFile, destFile, true);
      return 0;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  public ExitCode transplantDir(String from, String to) {
    from = project + "\\" + from.Replace("/", "\\");
    // var to5 = new Kep5().project + "\\" + to.Replace("/", "\\");
    to = donnee().project + "\\" + to.Replace("/", "\\");
    print("  * Copying {0} to {1}... ", from, to);

    try {
      ExitCode status = -1;
      // status = CopyDirectory(from, to) && CopyDirectory(from, to5);
      status = CopyDirectory(from, to);
      if (status) println("[  OK  ]");
      return status;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

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

      if (ZlpIOHelper.DirectoryExists(destFolder)) ZlpIOHelper.DeleteDirectory(destFolder, true);
      if (!ZlpIOHelper.DirectoryExists(destFolder)) ZlpIOHelper.CreateDirectory(destFolder);

      var files = new ZlpDirectoryInfo(sourceFolder).GetFiles();
      foreach (var file in files) {
        var name = ZlpPathHelper.GetFileNameFromFilePath(file.FullName);
        var dest = ZlpPathHelper.Combine(destFolder, name);
        file.CopyTo(dest, true);
      }

      var folders = new ZlpDirectoryInfo(sourceFolder).GetDirectories();
      foreach (var folder in folders) {
        var name = ZlpPathHelper.GetFileNameFromFilePath(folder.FullName);
        var dest = ZlpPathHelper.Combine(destFolder, name);
        var status = CopyDirectory(folder.FullName, dest);
        if (!status) { println("[FAILED]"); return status; }
      }

      return 0;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }
}