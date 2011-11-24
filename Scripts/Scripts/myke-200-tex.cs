// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "tex", priority = 200, description =
  "Compiles tex files using pdflatex.\r\n" +
  "Supports the idiom of master files (i.e. ones that have to be built instead of the target one).")]

public class Tex : Git {
  public Tex() : base() {}
  public Tex(FileInfo file) : base(file) {}
  public Tex(DirectoryInfo dir) : base(dir) {}

  public override bool accept() {
    return input != null;
  }

  public virtual FileInfo input { get {
    var master = dir.GetFiles("HW*.tex").Concat(dir.GetFiles("Test*.tex")).SingleOrDefault();
    if (master != null) return master;

    return file != null && file.Extension == ".tex" ? file : null;
  } }

  public virtual FileInfo output { get {
    return input != null ? new FileInfo(Path.ChangeExtension(input.FullName, "pdf")) : null;
  } }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("pdflatex " + input);
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    return compile() && Console.ui("\"" + output.FullName + "\"");
  }
}