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

[Connector(name = "kep", priority = 999, description =
  "Wraps the development workflow of project Kepler.")]

public class Kep : Git {
  public override String project { get { return @"%PROJECTS%\Kepler".Expand(); } }
  public virtual String profile { get { return "fastlocker"; } }
  //public virtual String profile { get { return "build"; } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  private Arguments arguments;
  public Kep() : base() { init(); }
  public Kep(FileInfo file, Arguments arguments) : base(file) { init(); this.arguments = arguments; }
  public Kep(DirectoryInfo dir, Arguments arguments) : base(dir) { init(); this.arguments = arguments; }
  private void init() { env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]scala):([0-9]+)"; }

  public virtual bool inPlayground { get {
    var names = new [] {
      file == null ? null : file.FullName.Replace("\\", "/"),
      dir == null ? null : dir.FullName.Replace("\\", "/"),
    };

    return names.Any(name => name != null && (name.Contains("/test/") || name.Contains("/sandbox/")));
  } }

  public virtual bool inTest { get {
    if (file != null && file.FullName.Replace("\\", "/").Contains("/test/")) return true;

    var s = Path.GetFullPath(dir.FullName).Replace("\\", "/");
    var iof = s.IndexOf("/test/");
    if (iof == -1) return false;
    s = s.Substring(iof + "/test/".Length);
    // test/files/run will fail, but test/files/run/blah will pass
    return s.Where(c => c == '/').Count() >= 2;
  } }

  [Action]
  public virtual ExitCode clean() {
    if (inPlayground) {
      dir.GetDirectories("*.obj").ToList().ForEach(dir1 => dir1.Delete());
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
      var scala = new Scala(file, lines, arguments);
      return scala.rebuild();
    } else {
      return Console.batch("ant clean " + profile + " -buildfile build.xml", home: root);
    }
  }

  [Default, Action]
  public virtual ExitCode compile() {
    if (inPlayground && file != null) {
      var lines = new Lines(file, File.ReadAllLines(file.FullName).ToList());
      var scala = new Scala(file, lines, arguments);
      return scala.compile();
    } else {
      var status = Console.batch("ant " + profile + " -buildfile build.xml", home: root);
      if (!status) return status;

      var partest = project + @"\build\locker\classes\partest";
      if (!Directory.Exists(partest)) {
        var donor = new Donor(new DirectoryInfo(new Donor().project), arguments);
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
      var scala = new Scala(file, lines, arguments);
      return scala.run(arguments);
    } else {
      //var options = new List<String>();
      //options.Add("-deprecation");
      //options.Add("-Yreify-copypaste");
      //Func<String> readArguments = () => Console.readln(prompt: "Lift", history: String.Format("lift {0}", root.FullName));
      //options.Add("-e \"scala.reflect.mirror.reify{" + (arguments.Count > 0 ? arguments.ToString() : readArguments()) + "}\"");
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
        var scala = new Scala(toRun, lines, arguments);
        return scala.run(arguments);
      } else {
        println("error: command is ambiguous");
        files.Take(5).ToList().ForEach(file1 => println("    " + file1));
        if (files.Count > 5) println("    ... " + (files.Count - 5) + " more");
        return -1;
      }
    }
  }

  [Action]
  public override ExitCode runTest() {
    List<String> tests;
    if (inTest) {
      var prefix = project;
      prefix = prefix.Replace("/", "\\");
      if (!prefix.EndsWith("\\")) prefix += "\\";
      prefix += "test\\";
      var files = new List<FileSystemInfo>{(FileSystemInfo)file ?? dir}.Concat(arguments.Select(argument => new DirectoryInfo(argument).Exists ? (FileSystemInfo)new DirectoryInfo(argument) : new FileInfo(argument))).ToList();
      files = files.Where(file1 => file1.Exists).ToList();
      tests = files.Select(f => f.FullName.Substring(prefix.Length)).ToList();
      if (tests.Count == 0) { println("nothing to test!"); return -1; }
    } else {
      var suite = getCurrentTestSuite();
      if (suite == null) { println("there is no test suite associated with this project"); return -1; }
      tests = getTestSuiteTests(suite);
      if (tests == null || tests.Count() == 0) { println(suite + " does not have any tests"); return -1; }
      tests.ForEach(test => { if (!test.StartsWith("test\\")) throw new Exception("bad test: " + test); });
      tests = tests.Select(test => test.Substring("test\\".Length)).ToList();
    }

    var partest = @"%SCRIPTS_HOME%\partest.exe";
    return Console.batch("\"" + partest + "\" " + String.Join(" ", tests.ToArray()), home: root + "\\test");
  }

  public override List<String> calculateTestSuiteTests(String profile) {
    Func<String, String, String, bool> filter = null;
    if (profile == "macro") {
      filter = (fullName, shortName, text) => {
        var pos = fullName.Contains("macro") || text.Contains("macro");
        var neg = shortName == "test\\files\\run\\reify_printf.scala";
        return pos && !neg;
      };
    } else if (profile == "reify") {
      filter = (fullName, shortName, text) => {
        var pos = fullName.Contains("reify") || text.Contains("reify") || text.Contains("TypeTag") || text.Contains("GroundTypeTag");
        var neg = shortName.StartsWith("test\\files\\run\\macro-def-path-dependent");
        return pos && !neg;
      };
    }

    if (filter == null)
      return null;

    var ktr = new DirectoryInfo(root + "\\test\\files\\run");
    var ktp = new DirectoryInfo(root + "\\test\\files\\pos");
    var ktn = new DirectoryInfo(root + "\\test\\files\\neg");
    var ktdirs = new []{ktr, ktp, ktn}.ToList();
    return ktdirs.SelectMany(ktdir => ktdir.GetFiles("*.scala", SearchOption.AllDirectories).Select(f => {
      var fullName = f.FullName;
      var prefix = ktdir.FullName;
      if (!prefix.EndsWith("\\")) prefix += "\\";
      var stripped = f.FullName.Substring(prefix.Length);
      var iof = stripped.IndexOf("\\");
      stripped = iof == -1 ? stripped : stripped.Substring(0, iof);
      var category = ktdir.FullName.Substring(root.FullName.Length);
      if (category.StartsWith("\\")) category = category.Substring(1);
      if (category.EndsWith("\\")) category = category.Substring(0, category.Length - 1);
      var shortName = category + "\\" + stripped;
      var text = File.ReadAllText(f.FullName);
      return filter(fullName, shortName, text) ? shortName : null;
    })).Where(test => test != null).Distinct().ToList();
  }
}