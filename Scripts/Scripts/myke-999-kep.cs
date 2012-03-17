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

    return names.Any(name => name != null && (name.Contains("/test") || name.Contains("/sandbox")));
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

  public virtual List<String> toTest(String profile) {
    var dotTestName = ".test" + (String.IsNullOrEmpty(profile) ? "" : "." + profile);
    var dotTest = new FileInfo(root + "\\" + dotTestName);
    if (!dotTest.Exists) {
      println("error: " + dotTestName + " file not found");
      return null;
    }

    var fs_toTest = File.ReadAllLines(dotTest.FullName).ToList();
    return fs_toTest.Select(f_toTest => {
      var iof = f_toTest.IndexOf("#");
      if (iof != -1) f_toTest = f_toTest.Substring(0, iof);
      return f_toTest.Trim();
    }).Where(f_toTest => f_toTest != String.Empty).Select(f_toTest => {
      return project + "\\" + f_toTest;
    }).ToList();
  }

  [Action]
  public virtual ExitCode runTest() {
    var dotProfile = new FileInfo(root + "\\.profile");
    var profile = dotProfile.Exists ? File.ReadAllText(dotProfile.FullName) : null;
    var script = toTest(profile);
    if (script == null || script.Count() == 0) return -1;

    var prefix = project;
    prefix = prefix.Replace("/", "\\");
    if (!prefix.EndsWith("\\")) prefix += "\\";
    prefix += "test\\";
    var tests = script.Select(f => f.Substring(prefix.Length)).ToList();
    var partest = @"%SCRIPTS_HOME%\partest.exe";
    return Console.batch("\"" + partest + "\" " + String.Join(" ", tests.ToArray()), home: root + "\\test");
  }

  protected virtual ExitCode indexTests(String profile, Func<String, String, String, bool> filter) {
    var dotTest = new FileInfo(root + "\\.test." + profile);
    var existingTests = dotTest.Exists ? File.ReadAllLines(dotTest.FullName).ToList() : new List<String>();

    var ktr = new DirectoryInfo(root + "\\test\\files\\run");
    var ktp = new DirectoryInfo(root + "\\test\\files\\pos");
    var ktn = new DirectoryInfo(root + "\\test\\files\\neg");
    var ktdirs = new []{ktr, ktp, ktn}.ToList();
    var tests = ktdirs.SelectMany(ktdir => ktdir.GetFiles("*.scala", SearchOption.AllDirectories).Select(f => {
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

    println("found " + tests.Count + " " + profile + " tests");
    var newTests = tests.Where(test => existingTests.Where(existingTest => existingTest.Contains(test)).Count() == 0).ToList();
    if (newTests.Count() == tests.Count()) println("all of them are new");
    else if (newTests.Count() == 0) println("none of them are new");
    else println(newTests.Count() + " of them are new: " + String.Join(", ", newTests.ToArray()));
    var obsoleteTests = existingTests.Where(existingTest => tests.Where(test => existingTest.Contains(test)).Count() == 0).ToList();
    if (obsoleteTests.Count() == tests.Count()) println("all of them are obsolete");
    else if (obsoleteTests.Count() == 0) println("none of them are obsolete");
    else println(obsoleteTests.Count() + " of them are obsolete: " + String.Join(", ", obsoleteTests.ToArray()));

    if (newTests.Count() != 0 || obsoleteTests.Count() != 0) {
      existingTests.AddRange(newTests);
      existingTests.RemoveAll(existingTest => obsoleteTests.Contains(existingTest));
      File.WriteAllLines(dotTest.FullName, existingTests);
      println("wrote " + dotTest.FullName.Substring(root.FullName.Length + 1));
    }

    return 0;
  }

  [Action]
  public virtual ExitCode indexMacroTests() {
    return indexTests("macro", (fullName, shortName, text) => {
      var pos = fullName.Contains("macro") || text.Contains("macro");
      var neg = shortName == "test\\files\\run\\reify_printf.scala";
      return pos && !neg;
    });
  }

  [Action]
  public virtual ExitCode indexReifyTests() {
    return indexTests("reify", (fullName, shortName, text) => {
      var pos = fullName.Contains("reify") || text.Contains("reify") || text.Contains("TypeTag") || text.Contains("GroundTypeTag");
      var neg = shortName.StartsWith("test\\files\\run\\macro-def-path-dependent");
      return pos && !neg;
    });
  }
}