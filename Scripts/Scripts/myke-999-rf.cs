// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "rf", priority = 999, description =
  "Wraps the development workflow of project Reflection.\r\n" +
  "Uses sbt for everything, but doesn't support tests by design.")]

public class Rf : Sbt {
  public override String project { get { return @"%PROJECTS%\Reflection".Expand(); } }

  public Rf() : base() {}
  public Rf(FileInfo file) : base(file) {}
  public Rf(DirectoryInfo dir) : base(dir) {}

  [Action]
  public override ExitCode test() {
    Console.println("error: tests are not supported by design");
    Console.println("all reflection tests should go directly to Kepler's partest infrastructure");
    Console.println("this is essential to promote contribution to scalac");
    return -1;
  }
}