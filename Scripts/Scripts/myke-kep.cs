// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "kep", description =
  "Wraps the development workflow of project Kepler.\r\n" +
  "Uses ant for building, itself for a repl, runs Reflection and doesn't support tests yet.")]

public class Kep : Git {
  public override String project { get { return @"%PROJECTS%\Kepler".Expand(); } }

  public Kep(DirectoryInfo dir = null) : base(dir) {
  }

  [Action]
  public virtual ExitCode rebuild() {
    var status = Console.batch("ant all.clean -buildfile build.xml", home: root);
    return status && println() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("ant build -buildfile build.xml", home: root);
  }

  [Action]
  public virtual ExitCode repl() {
    var status = compile();
    return status && println() && Console.interactive(@"build\pack\bin\scala.bat -deprecation", home: root);
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    var status = compile();
    return status && println() && new Rf().run(arguments);
  }

  [Action]
  public virtual ExitCode test() {
    Console.println("error: not yet implemented");
    Console.println("as far as I can see, it should invoke Kepler-related tests via partest");
    return -1;
  }
}