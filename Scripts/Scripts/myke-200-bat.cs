// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "bat", priority = 200, description =
  "Simply runs Windows batch files (*.bat, *.cmd extensions)")]

public class Bat : Git {
  public Bat(FileInfo file) : base(file) {
  }

  public override bool accept() {
    return file.Extension == ".bat" || file.Extension == ".cmd";
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    Func<String> readArguments = () => Console.readln(prompt: "Run arguments", history: String.Format("run {0}", file.FullName));
    return Console.interactive(file.FullName.GetShortPath() + " " + (arguments.Count > 0 ? arguments.ToString() : readArguments()), home: root);
  }
}