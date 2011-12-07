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
  public virtual ExitCode clean() {
    dir.GetFiles("*.class").ToList().ForEach(file1 => file1.Delete());
    dir.GetFiles("*.log").ToList().ForEach(file1 => file1.Delete());
    return 0;
  }

  [Action]
  public virtual ExitCode rebuild() {
    return clean() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch(compiler, home: root);
  }

  public virtual String inferMainclass() {
    var mains = lines.Select(line => {
      var m = Regex.Match(line, @"object\s+(?<name>.*?)\s+extends\s+App");
      return m.Success ? m.Result("${name}") : null;
    }).Where(main => main != null).ToList();

    return mains.Count() == 1 ? mains.Single() : null;
  }

  public virtual String inferArguments() {
    return lines.Any(line => line.Contains("args")) ? null : "";
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    Func<String> readMainclass = () => inferMainclass() ?? Console.readln(prompt: "Main class", history: String.Format("mainclass {0}", file.FullName));
    Func<String> readArguments = () => inferArguments() ?? Console.readln(prompt: "Run arguments", history: String.Format("run {0}", file.FullName));
    return compile() && Console.interactive("scala " + " " + readMainclass() + " " + (arguments.Count > 0 ? arguments.ToString() : readArguments()), home: root);
  }
}