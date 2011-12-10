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

  public virtual bool inPlayground { get {
    var names = new [] {
      file == null ? null : file.FullName.Replace("\\", "/"),
      dir == null ? null : dir.FullName.Replace("\\", "/"),
    };

    return names.Any(name => name != null && (name.Contains("/test") || name.Contains("/sandbox")));
  } }

  [Action]
  public virtual ExitCode clean() {
    if (inPlayground) {
      dir.GetFiles("*.class").ToList().ForEach(file1 => file1.Delete());
      dir.GetFiles("*.log").ToList().ForEach(file1 => file1.Delete());
      return 0;
    } else {
      //return Console.batch("ant clean -buildfile build.xml", home: root);
      println("error: clean for kepler is disabled to prevent occasional loss of work");
      return -1;
    }
  }

  [Action]
  public virtual ExitCode rebuild() {
    if (inPlayground && file != null) {
      var lines = new Lines(file, File.ReadAllLines(file.FullName).ToList());
      var scala = new Scala(file, lines);
      return scala.rebuild();
    } else {
      return Console.batch("ant clean build -buildfile build.xml", home: root);
    }
  }

  [Default, Action]
  public virtual ExitCode compile() {
    if (inPlayground && file != null) {
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

  public virtual FileInfo toRun { get {
    var dotRun = new FileInfo(root + "\\.run");
    if (!dotRun.Exists) {
      println("error: .run file not found");
      return null;
    }

    var f_toRun = File.ReadAllLines(dotRun.FullName).FirstOrDefault();
    if (f_toRun != null) f_toRun = project + "\\" + f_toRun;
    if (f_toRun == null || !File.Exists(f_toRun)) {
      println("error: {0}, file referenced by .run does not exist", f_toRun);
      return null;
    }

    return new FileInfo(f_toRun);
  } }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    if (inPlayground && file != null) {
      var lines = new Lines(file, File.ReadAllLines(file.FullName).ToList());
      var scala = new Scala(file, lines);
      return scala.run(arguments);
    } else {
      //var options = new List<String>();
      //options.Add("-deprecation");
      //options.Add("-Yreify-copypaste");
      //Func<String> readArguments = () => Console.readln(prompt: "Lift", history: String.Format("lift {0}", root.FullName));
      //options.Add("-e \"scala.reflect.Code.lift{" + (arguments.Count > 0 ? arguments.ToString() : readArguments()) + "}\"");
      //return Console.batch("scala " + String.Join(" ", options.ToArray()));

      if (toRun == null) return -1;
      println("running {0}", toRun.FullName);
      var lines = new Lines(toRun, File.ReadAllLines(toRun.FullName).ToList());
      var scala = new Scala(toRun, lines);
      return scala.run(arguments);
    }
  }

  public virtual List<String> toTest { get {
    var dotTest = new FileInfo(root + "\\.test");
    if (!dotTest.Exists) {
      println("error: .test file not found");
      return null;
    }

    var fs_toTest = File.ReadAllLines(dotTest.FullName).ToList();
    return fs_toTest.Select(f_toTest => project + "\\" + f_toTest).ToList();
  } }

  [Action]
  public virtual ExitCode runTest() {
    var prefix = project;
    prefix = prefix.Replace("/", "\\");
    if (!prefix.EndsWith("\\")) prefix += "\\";
    prefix += "test\\";
    var tests = toTest.Select(f => f.Substring(prefix.Length)).ToList();
    return Console.batch("partest " + String.Join(" ", tests.ToArray()));
  }
}