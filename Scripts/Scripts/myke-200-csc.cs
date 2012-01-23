// build this with "csc /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "csc", priority = 200, description =
  "Builds a csharp program using the command-line provided in the first line of the target file.\r\n" +
  "This no-hassle approach can do the trick for simple programs, but for more complex scenarios consider using msbuild.")]

public class Csc : Git {
  private Lines lines;

  public Csc(FileInfo file, Lines lines) : base(file) {
    this.lines = lines;
  }

  public virtual bool isconsole { get {
    return !lines.Any(line => line.Contains("[STAThread]"));
  } }

  public virtual String compiler { get {
    var shebang = lines.ElementAtOrDefault(0) ?? "";
    var r = new Regex("^\\s*//\\s*build\\s+this\\s+with\\s+\"(?<commandline>.*)\"\\s*$");
    var m = r.Match(shebang);
    return m.Success ? m.Result("${commandline}") : ("csc " + (isconsole ? "/t:exe " : "/t:winexe ") + file.Name);
  } }

  public virtual FileInfo exe { get {
    if (compiler == null) return null;

    var r = new Regex(@"/out:(?<out>\S+)");
    var m = r.Match(compiler);
    return new FileInfo(m.Success ? m.Result("${out}") : Path.ChangeExtension(file.FullName, ".exe"));
  } }

  public override bool accept() {
    return file.Extension == ".cs" && compiler != null;
  }

  [Action]
  public virtual ExitCode clean() {
    println("not yet implemented");
    return -1;
  }

  [Action]
  public virtual ExitCode rebuild() {
    return clean() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch(compiler, home: root);
  }

  public virtual String inferArguments() {
    return lines.Any(line => line.Contains("args") && !line.Contains("String[] args")) ? null : "";
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    var status = compile();
    if (status != 0) return -1;

    Func<String> readArguments = () => inferArguments() ?? Console.readln(prompt: "Run arguments", history: String.Format("run {0}", file.FullName));
    return Console.interactive(exe.FullName.GetShortPath() + " " + (arguments.Count > 0 ? arguments.ToString() : readArguments()), home: root);
  }
}