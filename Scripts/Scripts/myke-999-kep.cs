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

[Connector(name = "kep", priority = 999, description =
  "Wraps the development workflow of project Kepler.\r\n" +
  "Uses ant for building, itself for a repl, runs Reflection and doesn't support tests yet.")]

public class Kep : Git {
  public override String project { get { return @"%PROJECTS%\Kepler".Expand(); } }

  public Kep() : base() {}
  public Kep(FileInfo file) : base(file) {}
  public Kep(DirectoryInfo dir) : base(dir) {}

  [Action]
  public virtual ExitCode rebuild() {
    var status = Console.batch("ant clean -buildfile build.xml", home: root);
    return status && println() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    if (file != null && file.FullName.Replace("\\", "/").Contains("/test/")) {
      var lines = new Lines(file, File.ReadAllLines(file.FullName).ToList());
      var scala = new Scala(file, lines);
      return scala.compile();
    } else {
      return Console.batch("ant build -buildfile build.xml", home: root);
    }
  }

  [Action]
  public virtual ExitCode repl() {
    var status = compile();
    return status && println() && Console.interactive(@"build\pack\bin\scala.bat -deprecation", home: root);
  }

  public virtual FileInfo current { get {
    var dotcurrent = new FileInfo(root + "\\.current");
    if (!dotcurrent.Exists) {
      println("error: .current file not found");
      return null;
    }

    var f_current = File.ReadAllLines(dotcurrent.FullName).FirstOrDefault();
    if (f_current != null) f_current = project + "\\" + f_current;
    if (f_current == null || !File.Exists(f_current)) {
      println("error: file referenced by .current does not exist");
      return null;
    }

    return new FileInfo(f_current);
  } }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    if (file != null && file.FullName.Replace("\\", "/").Contains("/test/")) {
      var lines = new Lines(file, File.ReadAllLines(file.FullName).ToList());
      var scala = new Scala(file, lines);
      return scala.run("Test", "");
    } else {
      //var options = new List<String>();
      //options.Add("-deprecation");
      //options.Add("-Yreify-copypaste");
      //Func<String> readArguments = () => Console.readln(prompt: "Lift", history: String.Format("lift {0}", root.FullName));
      //options.Add("-e \"scala.reflect.Code.lift{" + (arguments.Count > 0 ? arguments.ToString() : readArguments()) + "}\"");
      //return Console.batch("scala " + String.Join(" ", options.ToArray()));

      if (current == null) return -1;
      var lines = new Lines(current, File.ReadAllLines(current.FullName).ToList());
      var scala = new Scala(current, lines);
      return scala.run("Test", "");
    }
  }

  [Action]
  public virtual ExitCode compileTest() {
    if (current == null) return -1;
    var lines = new Lines(current, File.ReadAllLines(current.FullName).ToList());
    var scala = new Scala(current, lines);
    return scala.compile();
  }

  [Action]
  public virtual ExitCode runTest() {
    if (current == null) return -1;
    var lines = new Lines(current, File.ReadAllLines(current.FullName).ToList());
    var scala = new Scala(current, lines);
    return scala.compile() && scala.run("Test", "");
  }
}