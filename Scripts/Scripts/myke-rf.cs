// build this with "csc /t:exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "rf", description = "Wraps the development workflow of project Reflection.\r\n" +
                                      "Uses sbt for everything, but doesn't support tests by design.")]
public class Rf : Sbt {
  private DirectoryInfo dir;

  public Rf(DirectoryInfo dir) : base(dir) {
    this.dir = dir;
  }

  public override DirectoryInfo repo { get {
    var s_prj = Environment.GetEnvironmentVariable("PROJECTS");
    if (s_prj == null) return null;

    var prj = new DirectoryInfo(s_prj);
    var reflection = prj.GetDirectories().FirstOrDefault(child => child.Name == "Reflection");

    var repo = base.repo;
    if (repo == null) return null;
    if (repo != dir) return null;

    return repo;
  } }

  public virtual bool accept() {
    return repo != null;
  }

  [Action]
  public override ExitCode test() {
    Console.println("error: tests are not supported by design");
    Console.println("all reflection tests should go directly to Kepler's partest infrastructure");
    Console.println("this is essential to promote contribution to scalac");
    return -1;
  }
}