// build this with "csc /t:exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "kep", description = "Wraps the development workflow of project Kepler.\r\n" +
                                       "Uses ant for building, itself for a repl, runs Reflection and doesn't support tests yet.")]
public class Kep : Git {
  private DirectoryInfo dir;

  public Kep(DirectoryInfo dir) : base(dir) {
    this.dir = dir;
  }

  public override DirectoryInfo repo { get {
    var repo = base.repo;
    if (repo == null) return null;

    var buildXml = repo.GetFiles().FirstOrDefault(child => child.Name == "build.xml");
    if (buildXml == null) return null;

    return repo;
  } }

  public virtual DirectoryInfo reflection { get {
    var s_prj = Environment.GetEnvironmentVariable("PROJECTS");
    if (s_prj == null) return null;

    var prj = new DirectoryInfo(s_prj);
    return prj.GetDirectories().FirstOrDefault(child => child.Name == "Reflection");
  } }

  public virtual bool accept() {
    return repo != null;
  }

  [Action]
  public virtual ExitCode rebuild() {
    var status = Console.batch("ant all.clean --buildfile build.xml", home: repo);
    return status && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    var status = Console.batch("ant build --buildfile build.xml", home: repo);
    return status && Console.batch("ant pack --buildfile pack.xml", home: repo);
  }

  [Action]
  public virtual ExitCode repl() {
    var status = compile();
    return status && Console.interactive(@"build\pack\bin\scala.bat", home: repo);
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    var status = compile();
    status = status && new Rf(reflection).compile();
    return status && new Rf(reflection).run(arguments);
  }

  [Action]
  public virtual ExitCode test() {
    Console.println("error: not yet implemented");
    Console.println("as far as I can see, it should invoke Kepler-related tests via partest");
    return -1;
  }
}