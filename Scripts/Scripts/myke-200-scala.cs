// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "scala", priority = 200, description =
  "Builds a scala program using the command-line provided in the first line of the target file.\r\n" +
  "This no-hassle approach can do the trick for simple programs, but for more complex scenarios consider using sbt.")]

public class Scala : Git {
  private Lines lines;

  public Scala(FileInfo file, Lines lines) : base(file) {
    this.lines = lines;
  }

  public virtual String compiler { get {
    var shebang = lines.ElementAtOrDefault(0) ?? "";
    var r = new Regex("^\\s*//\\s*build\\s+this\\s+with\\s+\"(?<commandline>.*)\"\\s*$");
    var m = r.Match(shebang);
    return m.Success ? m.Result("${commandline}") : ("scalac -deprecation -Yreify-copypaste " + file.FullName);
  } }

  public override bool accept() {
    return file.Extension == ".scala" && compiler != null;
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
    //var status = compile();
    //if (status != 0) return status;

    Func<String> readMainclass = () => Console.readln(prompt: "Main class", history: String.Format("mainclass {0}", file.FullName));
    Func<String> readArguments = () => Console.readln(prompt: "Run arguments", history: String.Format("run {0}", file.FullName));
    return Console.interactive("scala " + " " + readMainclass() + " " + (arguments.Count > 0 ? arguments.ToString() : readArguments()));
  }
}