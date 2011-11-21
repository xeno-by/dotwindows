// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "csc", priority = -1, description =
  "Builds a csharp program using the command-line provided in the first line of the target file.\r\n" +
  "This no-hassle approach can do the trick for simple programs, but for more complex scenarios consider using msbuild.")]

public class Csc : Prj {
  private Lines lines;

  public Csc(FileInfo file, Lines lines) : base(file) {
    this.lines = lines;
  }

  public virtual String compiler { get {
    var shebang = lines.ElementAtOrDefault(0) ?? "";
    var r = new Regex("^\\s*//\\s*build\\s+this\\s+with\\s+\"(?<commandline>.*)\"\\s*$");
    var m = r.Match(shebang);
    return m.Success ? m.Result("${commandline}") : null;
  } }

  public virtual FileInfo exe { get {
    if (compiler == null) return null;

    var r = new Regex(@"/out:(?<out>\S+)");
    var m = r.Match(compiler);
    return new FileInfo(m.Success ? m.Result("${out}") : Path.ChangeExtension(file.FullName, ".exe"));
  } }

  public override bool accept() {
    return base.accept() && file.Extension == ".cs" && compiler != null;
  }

  [Action]
  public virtual ExitCode rebuild() {
    return compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    if (Config.verbose) Console.println(compiler);
    return Console.batch(compiler);
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    return compile() && (exe != null && exe.Exists) && Console.interactive(exe.FullName, arguments);
  }
}