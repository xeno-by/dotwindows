// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

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

  [Action]
  public virtual ExitCode clean() {
    dir.GetFiles("*.aux").ToList().ForEach(file => file.Delete());
    dir.GetFiles("*.log").ToList().ForEach(file => file.Delete());
    dir.GetFiles("*.nav").ToList().ForEach(file => file.Delete());
    dir.GetFiles("*.out").ToList().ForEach(file => file.Delete());
    dir.GetFiles("*.snm").ToList().ForEach(file => file.Delete());
    dir.GetFiles("*.toc").ToList().ForEach(file => file.Delete());
    return 0;
  }

  [Action]
  public virtual ExitCode rebuild() {
    return clean() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("pdflatex " + input, home: root);
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    return compile() && Console.ui("\"" + output.FullName + "\"", home: root);
  }
}