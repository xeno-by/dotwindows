// build this with "csc /t:exe /debug+ myke*.cs"

using System;
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
    var status = Console.batch(String.Format("sbt {0}clean", sbtproject == null ? null : "project " + sbtproject + " "), home: sbtRoot);
    return status && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch(String.Format("sbt {0}compile", sbtproject == null ? null : "project " + sbtproject + " "), home: sbtRoot);
  }

  [Action]
  public virtual ExitCode repl() {
    Console.println("error: not yet implemented");
    Console.println("this should extract classpath and scalahome information from an sbt project");
    Console.println("and run a standalone repl, since sbt isn't good at being integrated");
    return -1;
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
    return Console.batch(String.Format("sbt {0}test", sbtproject == null ? null : "project " + sbtproject + " "), home: sbtRoot);
  }
}