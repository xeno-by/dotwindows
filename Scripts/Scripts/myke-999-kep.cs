// build this with "csc /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

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
  public virtual String profile { get { return "fastlocker"; } }
  //public virtual String profile { get { return "build"; } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public Kep() : base() { init(); }
  public Kep(FileInfo file) : base(file) { init(); }
  public Kep(DirectoryInfo dir) : base(dir) { init(); }
  private void init() { env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]scala):([0-9]+)"; }

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
      return Console.batch("ant clean " + profile + " -buildfile build.xml", home: root);
    }
  }

  [Default, Action]
  public virtual ExitCode compile() {
    if (inPlayground && file != null) {
      var lines = new Lines(file, File.ReadAllLines(file.FullName).ToList());
      var scala = new Scala(file, lines);
      return scala.compile();
    } else {
      var status = Console.batch("ant " + profile + " -buildfile build.xml", home: root);
      if (!status) return status;

      var partest = project + @"\build\locker\classes\partest";
      if (!Directory.Exists(partest)) {
        var donor = new Donor(new DirectoryInfo(new Donor().project));
        return donor.compile();
      } else {
        return 0;
      }
    }
  }

  [Action]
  public virtual ExitCode repl() {
//    return compile() && println() && Console.interactive(Config.sublime ? "scala /sublime" : "scala", home: root);
    return Console.interactive(Config.sublime ? "scala /sublime" : "scala", home: root);
  }

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

      var root = new DirectoryInfo(@"%PROJECTS%\Kepler\sandbox\".Expand());
      var files = root.GetFiles("*.scala", SearchOption.AllDirectories).ToList();
      if (files.Count == 0) {
        println("error: nothing to run");
        return -1;
      } else if (files.Count == 1) {
        var toRun = files[0];
        println("running {0}", toRun.FullName);
        var lines = new Lines(toRun, File.ReadAllLines(toRun.FullName).ToList());
        var scala = new Scala(toRun, lines);
        return scala.run(arguments);
      } else {
        println("error: command is ambiguous");
        files.Take(5).ToList().ForEach(file1 => println("    " + file1));
        if (files.Count > 5) println("    ... " + (files.Count - 5) + " more");
        return -1;
      }
    }
  }

  public virtual List<String> toTest { get {
    var dotTest = new FileInfo(root + "\\.test");
    if (!dotTest.Exists) {
      println("error: .test file not found");
      return null;
    }

    var fs_toTest = File.ReadAllLines(dotTest.FullName).ToList();
    return fs_toTest.Where(f_toTest => !f_toTest.StartsWith("#")).Select(f_toTest => project + "\\" + f_toTest).ToList();
  } }

  [Action]
  public virtual ExitCode runTest() {
    if (toTest == null || toTest.Count() == 0) return -1;
    var prefix = project;
    prefix = prefix.Replace("/", "\\");
    if (!prefix.EndsWith("\\")) prefix += "\\";
    prefix += "test\\";
    var tests = toTest.Select(f => f.Substring(prefix.Length)).ToList();
    var partest = @"%SCRIPTS_HOME%\partest.exe";
    return Console.batch("\"" + partest + "\" " + String.Join(" ", tests.ToArray()), home: root + "\\test");
  }
}