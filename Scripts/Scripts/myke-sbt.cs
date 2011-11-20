// build this with "csc /t:exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "sbt", description = "Supports projects that can be built under sbt.\r\n" +
                                       "Runner and repl are overloaded because of glitches with vanilla implementation.")]
public class Sbt : Git {
  private DirectoryInfo dir;

  public Sbt(DirectoryInfo dir) : base(dir) {
    this.dir = dir;
  }

  public override DirectoryInfo repo { get {
    var buildSbt = repo.GetFiles().FirstOrDefault(child => child.Name == "build.sbt");
    var project = repo.GetDirectories().FirstOrDefault(child => child.Name == "project");
    if (!buildSbt.Exists && !project.Exists) return null;

    return dir;
  } }

  [Action]
  public virtual ExitCode rebuild() {
    var status = Console.batch("sbt clean", home: repo);
    return status && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("sbt compile", home: repo);
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
    return Console.batch("sbt test", home: repo);
  }
}