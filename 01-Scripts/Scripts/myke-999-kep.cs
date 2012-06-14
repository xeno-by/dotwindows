// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using ZetaLongPaths;

[Connector(name = "kep", priority = 999, description =
  "Wraps the development workflow of project Kepler.")]

public class Kep : Git {
  public override String project { get { return @"%PROJECTS%\Kepler".Expand(); } }
  public virtual String metadata { get { return @"%PROJECTS%\Metadata\Kepler".Expand(); } }
  public override bool accept() { return base.accept() || (this.GetType() == typeof(Kep) && dir.IsChildOrEquivalentTo(metadata)); }

  public virtual String profile { get { return "fastlocker"; } }
  public virtual String profileAlt { get { return "build"; } }
  public virtual String profileClean { get { return "locker.clean"; } }
  public virtual String profileAltClean { get { return "clean"; } }
  public virtual String profileLibrary { get { return "fastlocker.lib"; } }
  public virtual String profileReflect { get { return "fastlocker.reflect"; } }
  public virtual String profileCompiler { get { return "fastlocker.comp"; } }
  public virtual String profileAltLibrary { get { return "quick.lib"; } }
  public virtual String profileAltReflect { get { return "quick.reflect"; } }
  public virtual String profileAltCompiler { get { return "quick.comp"; } }

  public virtual String antExecutable() { return "ant" + " -Dscalac.args=\"\"\"" + String.Join(" ", arguments.ToArray()) + "\"\"\""; }
  public virtual ExitCode runAnt(String commandLine) {
    if (arguments.Contains("-Xprompt")) return Console.interactive(antExecutable() + " " + commandLine, home: project);
    return Console.batch(antExecutable() + " " + commandLine + " -buildfile build.xml", home: project);
  }
//  public virtual String antExecutable() { return "ant -Djavac.args=-Dmyke.comments=" + mykeComments().Replace(" ", "_"); }
//  public virtual String mykeComments() {
//    var dotCommentsName = ".comments";
//    var dotTest = new FileInfo(root + "\\" + dotCommentsName);
//    if (dotTest.Exists) {
//      return File.ReadAllText(dotTest.FullName);
//    } else {
//      var implicits = File.ReadAllLines(root + "\\" + @"src\compiler\scala\tools\nsc\typechecker\Implicits.scala");
//      if (implicits.Any(line => line.Contains("if (false)") && !line.Contains("//"))) return "macro materializers disabled";
//      else return "macro materializers enabled";
//    }
//  }

  protected Arguments arguments;
  public Kep() : base() { init(); }
  public Kep(FileInfo file, Arguments arguments) : base(file) { init(); this.arguments = arguments; }
  public Kep(DirectoryInfo dir, Arguments arguments) : base(dir) { init(); this.arguments = arguments; }
  private void init() { env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]scala):([0-9]+)"; }

  public virtual bool inPlayground { get {
    return dir.FullName.Replace("\\", "/").Contains("/sandbox");
  } }

  public virtual bool inTest { get {
    if (file != null && file.FullName.Replace("\\", "/").Contains("/test/")) return true;

    var s = Path.GetFullPath(dir.FullName).Replace("\\", "/");
    var iof = s.IndexOf("/test/");
    if (iof == -1) return false;
    s = s.Substring(iof + "/test/".Length);
    // test/files/run will fail, but test/files/run/blah will pass
    return s.Where(c => c == '/').Count() > 1;
  } }

  public virtual bool inTestRoot { get {
    if (file != null && file.FullName.Replace("\\", "/").Contains("/test/")) return true;

    var s = Path.GetFullPath(dir.FullName).Replace("\\", "/");
    var iof = s.IndexOf("/test/");
    if (iof == -1) return false;
    s = s.Substring(iof + "/test/".Length);
    return s.Where(c => c == '/').Count() == 1;
  } }

  [Action]
  public virtual ExitCode clean() {
    if (inPlayground || inTest || inTestRoot) {
      dir.GetDirectories("*.obj").ToList().ForEach(dir1 => dir1.Delete(true));
      dir.GetFiles("*.class").ToList().ForEach(file1 => file1.Delete());
      dir.GetFiles("*.log").ToList().ForEach(file1 => file1.Delete());
      return 0;
    } else {
      //return runAnt("all.clean");
      println("error: clean for kepler is disabled to prevent occasional loss of work");
      return -1;
    }
  }

  [Action]
  public virtual ExitCode rebuild() {
    if (inPlayground || inTest) {
      var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
      return scala.rebuild();
    } else {
      var mods = arguments;
      var library = mods.Count() == 0 || mods.Contains("library");
      var reflect = mods.Contains("reflect");
      var compiler = mods.Contains("compiler");
      var reflectAndCompiler = mods.Contains("reflect-and-compiler");
      var altLibrary = mods.Contains("alt") || mods.Contains("alt-library");
      var altReflect = mods.Contains("alt-reflect");
      var altCompiler = mods.Contains("alt-compiler");
      var altReflectAndCompiler = mods.Contains("alt-reflect-and-compiler");
      arguments = new Arguments(arguments.Where(arg => arg != "library" && arg != "reflect" && arg != "compiler" && arg != "reflect-and-compiler" && arg != "alt" && arg != "alt-library" && arg != "alt-reflect" && arg != "alt-compiler" && arg != "alt-reflect-and-compiler").ToList());

      if (library) return runAnt(profileClean + " " + profile);
      else if (reflect) return rebuildReflect();
      else if (compiler) return rebuildCompiler();
      else if (reflectAndCompiler) return rebuildReflectAndCompiler();
      else if (altLibrary) return rebuildAltLibrary();
      else if (altReflect) return rebuildAltReflect();
      else if (altCompiler) return rebuildAltCompiler();
      else if (altReflectAndCompiler) return rebuildAltReflectAndCompiler();
      else throw new Exception("don't know how to rebuild the stuff you asked: " + mods);
    }
  }

  [Action]
  public virtual ExitCode rebuildWithYourkit() {
    if (inPlayground || inTest) {
      var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
      return scala.rebuild();
    } else {
      return runAnt("/yourkit " + profileClean + " " + profile);
    }
  }

  [Action]
  public virtual ExitCode rebuildLibrary() {
    return rebuildLibraryInternal(false);
  }

  [Action]
  public virtual ExitCode rebuildLibraryWithYourkit() {
    return rebuildLibraryInternal(true);
  }

  private ExitCode rebuildLibraryInternal(bool yourkit) {
    var libraryClasses = new DirectoryInfo(project + "\\build\\locker\\classes\\library");
    if (libraryClasses.Exists) ZlpIOHelper.DeleteDirectory(libraryClasses.FullName, true);
    var libraryToken = new FileInfo(project + "\\build\\locker\\library.complete");
    if (libraryToken.Exists) libraryToken.Delete();
    var flags = yourkit ? "/yourkit " : "";
    return runAnt(flags + profileLibrary);
  }

  [Action]
  public virtual ExitCode rebuildReflect() {
    return rebuildReflectInternal(false);
  }

  [Action]
  public virtual ExitCode rebuildReflectWithYourkit() {
    return rebuildReflectInternal(true);
  }

  private ExitCode rebuildReflectInternal(bool yourkit) {
    var reflectClasses = new DirectoryInfo(project + "\\build\\locker\\classes\\reflect");
    if (reflectClasses.Exists) ZlpIOHelper.DeleteDirectory(reflectClasses.FullName, true);
    var reflectToken = new FileInfo(project + "\\build\\locker\\reflect.complete");
    if (reflectToken.Exists) reflectToken.Delete();
    var flags = yourkit ? "/yourkit " : "";
    return runAnt(flags + profileReflect);
  }

  [Action]
  public virtual ExitCode rebuildCompiler() {
    return rebuildCompilerInternal(false);
  }

  [Action]
  public virtual ExitCode rebuildCompilerWithYourkit() {
    return rebuildCompilerInternal(true);
  }

  private ExitCode rebuildCompilerInternal(bool yourkit) {
    var compilerClasses = new DirectoryInfo(project + "\\build\\locker\\classes\\compiler");
    if (compilerClasses.Exists) ZlpIOHelper.DeleteDirectory(compilerClasses.FullName, true);
    var compilerToken = new FileInfo(project + "\\build\\locker\\compiler.complete");
    if (compilerToken.Exists) compilerToken.Delete();
    var flags = yourkit ? "/yourkit " : "";
    return runAnt(flags + profileCompiler);
  }

  [Action]
  public virtual ExitCode rebuildReflectAndCompiler() {
    return rebuildReflect() && rebuildCompiler();
  }

  [Action]
  public virtual ExitCode rebuildReflectAndCompilerWithYourkit() {
    return rebuildReflectWithYourkit() && rebuildCompilerWithYourkit();
  }

  [Action]
  public virtual ExitCode rebuildCompilerAndReflect() {
    return rebuildReflectAndCompiler();
  }

  [Action]
  public virtual ExitCode rebuildCompilerAndReflectWithYourkit() {
    return rebuildCompilerAndReflectWithYourkit();
  }

  [Action]
  public virtual ExitCode rebuildAlt() {
    return runAnt(profileAltClean + " " + profileAlt);
  }

  [Action]
  public virtual ExitCode rebuildAltWithYourkit() {
    return runAnt("/yourkit " + profileAltClean + " " + profileAlt);
  }

  [Action]
  public virtual ExitCode rebuildAltLibrary() {
    return rebuildAltLibraryInternal(false);
  }

  [Action]
  public virtual ExitCode rebuildAltLibraryWithYourkit() {
    return rebuildAltLibraryInternal(true);
  }

  private ExitCode rebuildAltLibraryInternal(bool yourkit) {
    var libraryClasses = new DirectoryInfo(project + "\\build\\quick\\classes\\library");
    if (libraryClasses.Exists) ZlpIOHelper.DeleteDirectory(libraryClasses.FullName, true);
    var libraryToken = new FileInfo(project + "\\build\\quick\\library.complete");
    if (libraryToken.Exists) libraryToken.Delete();
    var flags = yourkit ? "/yourkit " : "";
    return runAnt(flags + profileAltLibrary);
  }

  [Action]
  public virtual ExitCode rebuildAltReflect() {
    return rebuildAltReflectInternal(false);
  }

  [Action]
  public virtual ExitCode rebuildAltReflectWithYourkit() {
    return rebuildAltReflectInternal(true);
  }

  private ExitCode rebuildAltReflectInternal(bool yourkit) {
    var reflectClasses = new DirectoryInfo(project + "\\build\\quick\\classes\\reflect");
    if (reflectClasses.Exists) ZlpIOHelper.DeleteDirectory(reflectClasses.FullName, true);
    var reflectToken = new FileInfo(project + "\\build\\quick\\reflect.complete");
    if (reflectToken.Exists) reflectToken.Delete();
    var flags = yourkit ? "/yourkit " : "";
    return runAnt(flags + profileAltReflect);
  }

  [Action]
  public virtual ExitCode rebuildAltCompiler() {
    return rebuildAltCompilerInternal(false);
  }

  [Action]
  public virtual ExitCode rebuildAltCompilerWithYourkit() {
    return rebuildAltCompilerInternal(true);
  }

  private ExitCode rebuildAltCompilerInternal(bool yourkit) {
    var compilerClasses = new DirectoryInfo(project + "\\build\\quick\\classes\\compiler");
    if (compilerClasses.Exists) ZlpIOHelper.DeleteDirectory(compilerClasses.FullName, true);
    var compilerToken = new FileInfo(project + "\\build\\quick\\compiler.complete");
    if (compilerToken.Exists) compilerToken.Delete();
    var flags = yourkit ? "/yourkit " : "";
    return runAnt(flags + profileAltCompiler);
  }

  [Action]
  public virtual ExitCode rebuildAltReflectAndCompiler() {
    return rebuildAltReflect() && rebuildAltCompiler();
  }

  [Action]
  public virtual ExitCode rebuildAltReflectAndCompilerWithYourkit() {
    return rebuildAltReflectWithYourkit() && rebuildAltCompilerWithYourkit();
  }

  [Action]
  public virtual ExitCode rebuildAltCompilerAndReflect() {
    return rebuildAltReflectAndCompiler();
  }

  [Action]
  public virtual ExitCode rebuildAltCompilerAndReflectWithYourkit() {
    return rebuildAltCompilerAndReflectWithYourkit();
  }

  [Action]
  public virtual ExitCode rebuildAll() {
    return runAnt("all.clean build");
  }

  [Action]
  public virtual ExitCode rebuildAllWithYourkit() {
    return runAnt("all.clean build");
  }

  [Default, Action]
  public virtual ExitCode compile() {
    if (inPlayground || inTest) {
      var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
      return scala.compile();
    } else {
      return runAnt(profile);
    }
  }

  [Action]
  public virtual ExitCode compileAlt() {
    if (inPlayground || inTest) {
      var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
      return scala.compile();
    } else {
      var status = runAnt("build");
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

  [Action, DontTrace]
  public virtual ExitCode repl() {
    var incantation = Config.sublime ? "scala /S" : "scala";
    if (arguments.Count() > 0) incantation += " ";
    incantation = incantation + String.Join(" ", arguments.ToArray());
    return Console.interactive(incantation, home: project);
  }

  [Action, Meaningful]
  public virtual ExitCode run() {
    if (inPlayground || inTest) {
      var scala = file != null ? new Scala(file, arguments): new Scala(dir, arguments);
      return scala.run();
    } else {
      //var options = new List<String>();
      //options.Add("-deprecation");
      //options.Add("-Yreify-copypaste");
      //Func<String> readArguments = () => Console.readln(prompt: "Lift", history: String.Format("lift {0}", root.FullName));
      //options.Add("-e \"scala.reflect.mirror.reify{" + (arguments.Count > 0 ? arguments.ToString() : readArguments()) + "}\"");
      //return Console.batch("scala " + String.Join(" ", options.ToArray()));

      // var root = new DirectoryInfo(project + @"\sandbox\");
      // var files = root.GetFiles("*.scala", SearchOption.AllDirectories).ToList();
      // if (files.Count == 0) {
      //   println("error: nothing to run");
      //   return -1;
      // } else if (files.Count == 1) {
      //   var toRun = files[0];
      //   println("running {0}", toRun.FullName);
      //   var scala = new Scala(toRun, arguments);
      //   return scala.run();
      // } else {
      //   println("error: command is ambiguous");
      //   files.Take(5).ToList().ForEach(file1 => println("    " + file1));
      //   if (files.Count > 5) println("    ... " + (files.Count - 5) + " more");
      //   return -1;
      // }
      var dir = new DirectoryInfo(project + @"\sandbox");
      var scala = new Scala(dir, arguments);
      return scala.run();

      //return Console.batch("partest files\\pos\\t1693.scala", home: project);
    }
  }

  public virtual Donor mkDonor() {
    return new Donor(new DirectoryInfo(project), arguments);
  }

  [Action]
  public override ExitCode runTest() {
    var partest1 = project + @"\build\locker\classes\partest";
    if (!Directory.Exists(partest1)) {
      var donor = mkDonor();
      var status1 = donor.compile();
      if (!status1) return status1;
    }

    var prefix = project;
    prefix = prefix.Replace("/", "\\");
    if (!prefix.EndsWith("\\")) prefix += "\\";
    prefix += "test\\";

    List<String> tests;
    if (inTest) {
      var files = new List<FileSystemInfo>{(FileSystemInfo)file ?? dir}.Concat(arguments.Select(argument => new DirectoryInfo(argument).Exists ? (FileSystemInfo)new DirectoryInfo(argument) : new FileInfo(argument))).ToList();
      files = files.Where(file1 => file1.Exists).ToList();
      tests = files.Select(f => f.FullName.Substring(prefix.Length)).ToList();
    } else {
      var suite = getCurrentTestSuite();
      if (suite == null) { println("there is no test suite associated with this project"); return -1; }
      tests = getTestSuiteAllTests(suite) ?? new List<String>();
      tests.ForEach(test => { if (!test.StartsWith(prefix, true, CultureInfo.CurrentCulture)) throw new Exception("bad test: " + test); });
      tests = tests.Select(test => test.Substring(prefix.Length)).ToList();
    }

    tests = tests.Select(test => {
      var full = prefix + test;

      if (full.Contains("nyi") || (Directory.Exists(full) && Directory.GetFiles(full, "*nyi*").Count() > 0))
        return null;

      var suffixes = new []{ ".check", ".flags", "-run.log", "-neg.log", "-pos.log" };
      foreach (var suffix in suffixes) {
        if (full.EndsWith(suffix)) {
          var trimmed = full.Substring(0, full.Length - suffix.Length);
          if (Directory.Exists(trimmed)) {
            return trimmed.Substring(prefix.Length);
          }
          trimmed += ".scala";
          if (File.Exists(trimmed)) {
            return trimmed.Substring(prefix.Length);
          }
        }
      }

      if (full.EndsWith(".scala")) {
        return test;
      } else if (Directory.Exists(full)) {
        return test;
      } else {
        return null;
      }
    }).Where(test => test != null).Distinct().ToList();

    if (tests.Count == 0) { println("nothing to test!"); return -1; }
    traceln("[myke] testing: {0}", String.Join(" ", tests.ToArray()));
    var partest = @"%SCRIPTS_HOME%\partest.exe";
    return Console.batch("\"\"" + partest + "\"\" " + String.Join(" ", tests.ToArray()), home: project + "\\test");
  }

  [Action]
  public override ExitCode open() {
    if (inTest) { Config.action = "run-test"; return runTest(); }
    else if (inPlayground && file.Extension == "scala") { Config.action = "run"; return run(); }
    else return base.open();
  }

  [Action]
  public ExitCode runAllTests() {
    var status = runAnt("build");
    if (status) {
      var tests = calculateTestSuiteTests("all").Select(test => test.Substring((project + "\\test\\").Length)).ToList();
      traceln("[myke] testing: {0}", String.Join(" ", tests.ToArray()));
      status = runAnt("test");
    }
    return status;
  }

  public override List<String> calculateTestSuiteTests(String profile) {
    Func<String, String, Func<String>, bool> filter = null;
    if (profile == "macro") {
      filter = (fullName, shortName, text0) => {
        var text = text0();
        var pos = fullName.Contains("macro") || text.Contains("macro");
        var neg = shortName == "test\\files\\run\\reify_printf.scala";
        return pos && !neg;
      };
    } else if (profile == "reify") {
      filter = (fullName, shortName, text0) => {
        var text = text0();
        var pos = fullName.Contains("reify") || text.Contains("reify") || text.Contains("Manifest") || text.Contains("ClassTag") || text.Contains("AbsTypeTag") || text.Contains("TypeTag");
        var neg = shortName.StartsWith("test\\files\\run\\macro-def-path-dependent");
        return pos && !neg;
      };
    } else if (profile == "array") {
      filter = (fullName, shortName, text0) => {
        var text = text0();
        var pos = text.Contains("Array") || text.Contains("ClassTag") || text.Contains("ClassManifest");
        var neg = false;
        return pos && !neg;
      };
    } else if (profile == "all") {
      filter = (fullName, shortName, text0) => true;
    }

    if (filter == null)
      return null;

    var ktr = new DirectoryInfo(root + "\\test\\files\\run");
    var ktp = new DirectoryInfo(root + "\\test\\files\\pos");
    var ktn = new DirectoryInfo(root + "\\test\\files\\neg");
    var ktj = new DirectoryInfo(root + "\\test\\files\\jvm");
    var ktdirs = new []{ktr, ktp, ktn, ktj}.ToList();
    return ktdirs.SelectMany(ktdir => ktdir.GetFiles("*.scala", SearchOption.AllDirectories).Concat(ktdir.GetFiles("*.java", SearchOption.AllDirectories)).Select(f => {
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
      Func<String> text = () => File.ReadAllText(f.FullName);
      return filter(fullName, shortName, text) ? (project + "\\" + shortName) : null;
    })).Where(test => test != null).Distinct().ToList();
  }

  public override List<String> getTestSuiteFailedTests(String profile) {
    return getTestSuiteTestsInternal(profile, "failed");
  }
  public override List<String> getTestSuiteSucceededTests(String profile) {
    return getTestSuiteTestsInternal(profile, "succeeded");
  }

  public List<String> getTestSuiteTestsInternal(String profile, String kind) {
    var result = new List<String>();
    var traceDir = new DirectoryInfo(@"%HOME%\.myke_important".Expand());
    if (traceDir.Exists) {
      var logs = traceDir.GetFiles("*.log").OrderByDescending(fi => fi.LastWriteTime).ToList();
      var tests = null as List<String>;
      var relevant = logs.Where(log => {
        var lines = File.ReadAllLines(log.FullName);
        var shebang = lines.FirstOrDefault();
        if (shebang != null) {
          var psiPrefix = "[myke] testing: ";
          var psi = lines.FirstOrDefault(line => line.StartsWith(psiPrefix));
          if (psi != null) {
            tests = psi.Substring(psiPrefix.Length).Split(new[]{" "}, StringSplitOptions.None).Select(shortName => project + "\\test\\" + shortName).ToList();
            return true;
          }
        }
        return false;
      }).FirstOrDefault();
      if (relevant != null) {
        println(relevant.FullName);
        var fragments = File.ReadAllLines(relevant.FullName).Select(line => {
          var rxSucceeded = @"testing: \[\.\.\.\](?<filename>.*?)\s*\[  OK  \]$";
          var rxFailed = @"(testing: \[\.\.\.\](?<filename>.*?)\s*\[FAILED\]$)|(Possible compiler crash during test of: (?<filename>.*?)$)";
          var rx = kind == "failed" ? rxFailed : rxSucceeded;
          var m = Regex.Match(line, rx);
          return m.Success ? m.Result("${filename}").Replace("/", "\\") : null;
        }).Where(fragment => fragment != null).Distinct().ToList();
        fragments.ForEach(fragment => {
          var neg = fragment.Contains("\\scalap\\");
          neg = neg || fragment.Contains("\\scalacheck\\");
          neg = neg || fragment.Contains("\\buildmanager\\");
          neg = neg || fragment.Contains("\\res\\");
          if (!neg) {
            var matches = tests.Where(test => test.EndsWith(fragment)).ToList();
            if (matches.Count() != 1) result.Add(fragment);
            else {
              var match = matches[0];
              var flavor = match.Substring((project + "\\test\\files\\").Length);
              var iof = flavor.IndexOf("\\");
              flavor = flavor.Substring(0, iof);
              result.Add(match);
              var filename = Path.GetDirectoryName(match) + "\\" + Path.GetFileNameWithoutExtension(match);
              result.Add(filename + "-" + flavor + ".log");
              result.Add(filename + ".check");
            }
          }
        });
      }
    }
    result = result.OrderBy(name => name).ToList();
    return result;
  }

  [Action, MenuItem(hotkey = "s", description = "Build in Jenkins", priority = 180)]
  public virtual ExitCode smartJenkins() {
    if (!verifyRepo()) return -1;
    var gitStatus = getCurrentStatus();
    if (gitStatus != null && gitStatus.Contains("nothing to commit")) {
      println("Nothing to commit");
      var branch = Config.rawTarget;
      if (branch == "") branch = getCurrentBranch();
      var url = getBranchJenkinsUrl(branch);
      if (url == null) return -1;
      return smartPush() && Console.ui(url);
    } else {
      return smartCommit();
    }
  }

  public virtual String getJenkinsUrl(String remote) {
    var lines = Console.eval("git remote -v", home: repo.GetRealPath());
    var line = lines.Where(line2 => line2.StartsWith(remote)).FirstOrDefault();
    if (line == null) return null;
    line = line.Substring(remote.Length).Trim();
    line = line.Substring(0, line.LastIndexOf("(") - 1).Trim();
    // https://github.com/scalamacros/kepler/pull/new/topic/reflection
    var url1 = line;
    var re1 = "^git://github.com/(?<user>.*?)/(?<repo>.*).git$";
    var m1 = Regex.Match(url1, re1);
    if (m1.Success) {
      return m1.Result("https://scala-webapps.epfl.ch/jenkins/job/scala-checkin-manual/buildWithParameters?githubUsername=${user}&repository=${repo}");
    } else {
      var re2 = "^git@github.com:(?<user>.*?)/(?<repo>.*).git$";
      var m2 = Regex.Match(url1, re2);
      if (m2.Success) {
        return m2.Result("https://scala-webapps.epfl.ch/jenkins/job/scala-checkin-manual/buildWithParameters?githubUsername=${user}&repository=${repo}");
      } else {
        return null;
      }
    }
  }

  public virtual String getBranchJenkinsUrl(String branch) {
    String remote = null;
    if (branch.StartsWith("remotes/")) {
      branch = branch.Substring("remotes/".Length);
      remote = branch.Substring(0, branch.IndexOf("/") - 1);
      branch = branch.Substring(branch.IndexOf("/") + 1);
    } else {
      remote = "origin";
    }

    var url = getJenkinsUrl(remote);
    if (url == null) return null;
    else return url + "&branch=" + branch;
  }

  public String mostUptodateLibs() {
    var loloLibraryJar = new FileInfo(root + @"\build\locker\lib\scala-library.jar");
    var loloReflectJar = new FileInfo(root + @"\build\locker\lib\scala-reflect.jar");
    var loloCompilerJar = new FileInfo(root + @"\build\locker\lib\scala-compiler.jar");
    var paloLibraryJar = new FileInfo(root + @"\build\palo\lib\scala-library.jar");
    var paloReflectJar = new FileInfo(root + @"\build\palo\lib\scala-reflect.jar");
    var paloCompilerJar = new FileInfo(root + @"\build\palo\lib\scala-compiler.jar");
    var packLibraryJar = new FileInfo(root + @"\build\pack\lib\scala-library.jar");
    var packReflectJar = new FileInfo(root + @"\build\pack\lib\scala-reflect.jar");
    var packCompilerJar = new FileInfo(root + @"\build\pack\lib\scala-compiler.jar");
    // todo. rewrite this to include ***reflectJar files into consideration
    var loloModTime = loloLibraryJar.Exists && loloCompilerJar.Exists ? (loloLibraryJar.LastWriteTime > loloCompilerJar.LastWriteTime ? loloLibraryJar.LastWriteTime : loloCompilerJar.LastWriteTime) : DateTime.MinValue;
    var paloModTime = paloLibraryJar.Exists && paloCompilerJar.Exists ? (paloLibraryJar.LastWriteTime > paloCompilerJar.LastWriteTime ? paloLibraryJar.LastWriteTime : paloCompilerJar.LastWriteTime) : DateTime.MinValue;
    var packModTime = packLibraryJar.Exists && packCompilerJar.Exists ? (packLibraryJar.LastWriteTime > packCompilerJar.LastWriteTime ? packLibraryJar.LastWriteTime : packCompilerJar.LastWriteTime) : DateTime.MinValue;
    if (loloModTime == DateTime.MinValue && paloModTime == DateTime.MinValue && packModTime == DateTime.MinValue) return null;
    return (loloModTime > paloModTime && loloModTime > packModTime) ? "build/locker/" : (paloModTime > loloModTime && paloModTime > packModTime) ? "build/palo/" : "build/pack/";
  }

  public ExitCode packLockerIntoJars() {
    var classesDir = project + @"\build\locker\classes";
    var jarDir = project + @"\build\locker\lib";
    if (!Directory.Exists(jarDir)) Directory.CreateDirectory(jarDir);
    var status = print("* scala-compiler... ") && Console.batch("jar cf ../lib/scala-compiler.jar -C compiler .", home: classesDir) && println("[  OK  ]");
    if (Directory.Exists(classesDir + @"\\" + "reflect")) {
      status = status && print("* scala-reflect... ") && Console.batch("jar cf ../lib/scala-reflect.jar -C reflect .", home: classesDir) && println("[  OK  ]");
    }
    status = status && print("* scala-library... ") && Console.batch("jar cf ../lib/scala-library.jar -C library .", home: classesDir) && println("[  OK  ]");
    status = status && print("* scala-partest... ") && Console.batch("jar cf ../lib/scala-partest.jar -C partest .", home: classesDir) && println("[  OK  ]");
    if (!status) println("[FAILED]");
    return status;
  }

  [Action, MenuItem(description = "Deploy to Starr", priority = 999.2)]
  public virtual ExitCode deployStarr() {
    var source = mostUptodateLibs();
    if (source == null) { println("Couldn't find neither pack nor palo nor lolo to deploy"); return -1; }

    println("Deploying starr upon ourselves...");
    var status = transplantFile(source + "lib/scala-library.jar", project + "/lib/scala-library.jar");
    if (File.Exists(source + "lib/scala-reflect.jar")) {
      status = status && transplantFile(source + "lib/scala-reflect.jar", project + "/lib/scala-reflect.jar");
    }
    status = status && transplantFile(source + "lib/scala-compiler.jar", project + "/lib/scala-compiler.jar");
    return status;
  }

  [Action, MenuItem(description = "Deploy to Ensime", priority = 999.1)]
  public virtual ExitCode deployEnsime() {
    var status = packLockerIntoJars();
    if (!status) return status;

    var source = mostUptodateLibs();
    if (source == null) { println("Couldn't find neither pack nor palo nor lolo to deploy"); return -1; }

    println("Deploying ourselves as a snapshot for Ensime...");
    status = status && transplantFile(source + "lib/scala-library.jar", "%APPDATA%/Sublime Text 2/Packages/SublimeEnsime/scala/scala-library.jar");
    if (File.Exists(project + "/" + source + "lib/scala-reflect.jar")) {
      status = status && transplantFile(source + "lib/scala-reflect.jar", "%APPDATA%/Sublime Text 2/Packages/SublimeEnsime/scala/scala-reflect.jar");
    }
    status = status && transplantFile(source + "lib/scala-compiler.jar", "%APPDATA%/Sublime Text 2/Packages/SublimeEnsime/scala/scala-compiler.jar");
    return status;
  }
}