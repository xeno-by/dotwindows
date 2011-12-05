// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "sbt", priority = 300, description =
  "Supports projects that can be built under sbt.\r\n" +
  "Runner and repl are overloaded because of glitches with vanilla implementation.")]

public class Sbt : Git {
  public virtual String sbtproject { get { return null; } }

  public Sbt() : base() {}
  public Sbt(FileInfo file) : base(file) {}
  public Sbt(DirectoryInfo dir) : base(dir) {}

  public override String project { get { return sbtroot == null ? null : sbtroot.FullName; } }
  public DirectoryInfo sbtroot { get {
    // todo. do we need to cache this?
    return detectsbtroot();
  } }

  public virtual DirectoryInfo detectsbtroot() {
    var wannabe = file != null ? file.Directory : dir;
    while (wannabe != null) {
      var buildSbt = wannabe.GetFiles().FirstOrDefault(child => child.Name == "build.sbt");
      var project = wannabe.GetDirectories().FirstOrDefault(child => child.Name == "project");
      if (buildSbt != null || project != null) return wannabe;
      wannabe = wannabe.Parent;
    }

    return null;
  }

  public override bool accept() {
    return base.accept() && dir.IsChildOrEquivalentTo(sbtroot);
  }

  [Action]
  public virtual ExitCode rebuild() {
    var preamble = String.Format("sbt {0}", sbtproject == null ? null : "\"project " + sbtproject + "\"");
    return Console.batch(String.Format("{0} clean compile", preamble), home: root);
  }

  [Default, Action]
  public virtual ExitCode compile() {
    var preamble = String.Format("sbt {0}", sbtproject == null ? null : "\"project " + sbtproject + "\"");
    return Console.batch(String.Format("{0} compile", preamble), home: root);
  }

  private class ProjectInfo {
    public String scalahome;
    public List<String> classpath;
    public List<String> mainclasses;
  }

  private ProjectInfo compileAndInfer() {
    var log = new FileInfo(Path.GetTempFileName());
    if (log.Exists) log.Delete();

    var preamble = String.Format("sbt {0}", sbtproject == null ? null : "\"project " + sbtproject + "\"");
    Console.batch(String.Format("{0} compile \"show runtime:scala-home\" \"show runtime:dependency-classpath\" \"show discovered-main-classes\" | tee {1}", preamble, log.FullName), home: root);

    if (!log.Exists) return null;
    var lines = File.ReadAllLines(log.FullName);
    if (lines.Any(line => line.StartsWith("[error]"))) return null;
    Console.println();

    var s_scalahome = lines[lines.Count() - 5];
    var r_scalahome = new Regex(@"^\[info\] Some\((?<path>.*)\)$");
    var m_scalahome = r_scalahome.Match(s_scalahome);
    if (!m_scalahome.Success) return null;
    var scalahome = m_scalahome.Result("${path}");
    if (Config.verbose) { Console.println("scalahome = {0}", scalahome); }

    var s_classpath = lines[lines.Count() - 4];
    var r_classpath = new Regex(@"^\[info\] ArrayBuffer\((?<paths>.*)\)$");
    var m_classpath = r_classpath.Match(s_classpath);
    if (!m_classpath.Success) return null;
    s_classpath = m_classpath.Result("${paths}");
    r_classpath = new Regex(@"Attributed\((?<path>.*?)\)");
    m_classpath = r_classpath.Match(s_classpath);
    if (!m_classpath.Success) return null;
    var classpath = new List<String>();
    for (; m_classpath.Success; m_classpath = m_classpath.NextMatch())
      classpath.Add(m_classpath.Result("${path}"));
    if (Config.verbose) { Console.println("classpath = {0}", String.Join("; ", classpath)); }

    var s_mainclasses = lines[lines.Count() - 2];
    var r_mainclasses = new Regex(@"^\[info\] List\((?<classes>.*)\)$");
    var m_mainclasses = r_mainclasses.Match(s_mainclasses);
    if (!m_mainclasses.Success) return null;
    s_mainclasses = m_mainclasses.Result("${classes}");
    var mainclasses = s_mainclasses.Split(',').Select(mainclass => mainclass.Trim()).ToList();
    if (Config.verbose) { Console.println("mainclasses = {0}", String.Join(", ", mainclasses)); }

    if (Config.verbose) { Console.println(); }
    return new ProjectInfo{scalahome = scalahome, classpath = classpath, mainclasses = mainclasses};
  }

  [Action]
  public virtual ExitCode repl() {
    var info = compileAndInfer();
    if (info == null) return -1;

    var scala = (info.scalahome + "\\bin\\scala.bat").GetShortPath();
    var options = new List<String>();
    options.Add("-deprecation");
    options.Add("-classpath " + String.Join(";", info.classpath.Select(path => path.GetShortPath())));
    return Console.interactive(scala + " " + String.Join(" ", options.ToArray()));
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    var info = compileAndInfer();
    if (info == null) return -1;

    String mainclass = null;
    if (info.mainclasses.Count() == 0) {
      Console.println("error: no main classes have been detected");
      return -1;
    } else if (info.mainclasses.Count() == 1) {
      mainclass = info.mainclasses[0];
    } else {
      Console.println("Please, select one of the detected mainclasses: {0} or {1}", String.Join(", ", info.mainclasses.Take(info.mainclasses.Count() - 1)), info.mainclasses[info.mainclasses.Count() - 1]);
      Func<String> readMainclass = () => Console.readln(prompt: "Mainclass", history: String.Format("mainclass {0}", root.FullName));
      mainclass = readMainclass();
      if (mainclass == String.Empty) {
        Console.println("error: mainclass cannot be empty");
        return -1;
      }
    }

    var scala = (info.scalahome + "\\bin\\scala.bat").GetShortPath();
    var options = new List<String>();
    options.Add("-deprecation");
    options.Add("-classpath " + String.Join(";", info.classpath.Select(path => path.GetShortPath())));
    options.Add(mainclass);
    Func<String> readArguments = () => Console.readln(prompt: "Run arguments", history: String.Format("run {0}", root.FullName));
    options.Add(arguments.Count > 0 ? arguments.ToString() : readArguments());
    return Console.interactive(scala + " " + String.Join(" ", options.ToArray()));
  }

  [Action]
  public virtual ExitCode compileTest() {
    println("error: compilation of tests without running them is not supported");
    return -1;
  }

  [Action]
  public virtual ExitCode runTest() {
    var preamble = String.Format("sbt {0}", sbtproject == null ? null : "\"project " + sbtproject + "\"");
    return Console.batch(String.Format("{0} test", preamble), home: root);
  }
}