// build this with "csc /t:exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "csc", description = "Builds a csharp program using the command-line provided in the first line of the target file.\r\n" +
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
    var result = compile();
    if (result != 0) return result;

    var exe = new FileInfo(Path.ChangeExtension(file.FullName, ".exe"));
    return exe.Exists ? Console.interactive(exe.FullName, arguments) : -1;
  }
}