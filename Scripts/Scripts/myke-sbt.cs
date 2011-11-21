// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "sbt", priority = -1, description =
  "Supports projects that can be built under sbt.\r\n" +
  "Runner and repl are overloaded because of glitches with vanilla implementation.")]

public class Sbt : Prj {
  public Sbt(DirectoryInfo dir = null) : base(dir) {
  }

  public virtual String sbtproject { get { return null; } }

  public DirectoryInfo sbtRoot { get {
    // todo. do we need to cache this?
    return detectSbtRoot();
  } }

  public virtual DirectoryInfo detectSbtRoot() {
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
    return base.accept() && dir.EquivalentTo(sbtRoot);
  }

  [Action]
  public virtual ExitCode rebuild() {
    var preamble = String.Format("sbt {0}", sbtproject == null ? null : "\"project " + sbtproject + "\"");
    return Console.batch(String.Format("{0} clean compile", preamble), home: sbtRoot);
  }

  [Default, Action]
  public virtual ExitCode compile() {
    var preamble = String.Format("sbt {0}", sbtproject == null ? null : "\"project " + sbtproject + "\"");
    return Console.batch(String.Format("{0} compile", preamble), home: sbtRoot);
  }

  [Action]
  public virtual ExitCode repl() {
    var log = new FileInfo("%TMP%\\sbt.log".Expand());
    if (log.Exists) log.Delete();

    var preamble = String.Format("sbt {0}", sbtproject == null ? null : "\"project " + sbtproject + "\"");
    Console.batch(String.Format("{0} compile \"show runtime:scala-home\" \"show runtime:dependency-classpath\" | tee {1}", preamble, log.FullName), home: sbtRoot);

    if (!log.Exists) return -1;
    var lines = File.ReadAllLines(log.FullName);
    if (lines.Any(line => line.StartsWith("[error]"))) return -1;
    Console.println();

    var s_scalahome = lines[lines.Count() - 3];
    var r_scalahome = new Regex(@"^\[info\] Some\((?<path>.*)\)$");
    var m_scalahome = r_scalahome.Match(s_scalahome);
    if (!m_scalahome.Success) return -1;
    var scalahome = m_scalahome.Result("${path}");
    if (Config.verbose) { Console.println("scalahome = {0}", scalahome); }

    var s_classpath = lines[lines.Count() - 2];
    var r_classpath = new Regex(@"^\[info\] ArrayBuffer\((?<paths>.*)\)$");
    var m_classpath = r_classpath.Match(s_classpath);
    if (!m_classpath.Success) return -1;
    s_classpath = m_classpath.Result("${paths}");
    r_classpath = new Regex(@"Attributed\((?<path>.*?)\)");
    m_classpath = r_classpath.Match(s_classpath);
    if (!m_classpath.Success) return -1;
    var classpath = new List<String>();
    for (; m_classpath.Success; m_classpath = m_classpath.NextMatch())
      classpath.Add(m_classpath.Result("${path}"));
    if (Config.verbose) { Console.println("classpath = {0}", String.Join("; ", classpath)); }

    var scala = (scalahome + "\\bin\\scala.bat").GetShortPath();
    var options = new List<String>();
    options.Add("-deprecation");
    options.Add("-classpath " + String.Join(";", classpath.Select(path => path.GetShortPath())));
    return Console.interactive(scala + " " + String.Join(" ", options.ToArray()));
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    Console.println("error: not yet implemented");
    Console.println("this should extract classpath and scalahome information from an sbt project");
    Console.println("and run a standalone app, since sbt isn't good at being integrated");
    return -1;
  }

  [Action]
  public virtual ExitCode test() {
    var preamble = String.Format("sbt {0}", sbtproject == null ? null : "\"project " + sbtproject + "\"");
    return Console.batch(String.Format("{0} test", preamble), home: sbtRoot);
  }
}