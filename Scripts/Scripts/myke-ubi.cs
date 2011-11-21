// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "ubi", description =
  "A refinement of the csc connector adapted for my custom CS scripts.\r\n" +
  "Adjusts git-related actions to work with %SOFTWARE% instead of current dir.")]

public class Ubi : Csc {
  public Ubi(FileInfo file, Lines lines) : base(file, lines) {
  }

  public override bool accept() {
    return base.accept() && file.Directory.EquivalentTo("%SCRIPTS_HOME%".Expand());
  }

  public override DirectoryInfo detectRepo() {
    var sw = new DirectoryInfo(@"%SOFTWARE%".Expand());

    var gitIndex = sw.GetDirectories().FirstOrDefault(child => child.Name == ".git");
    if (gitIndex == null) return null;

    return sw;
  }

  public override bool verifyRepo() {
    if (repo == null) {
      Console.println("%DROPBOX%\\Software\\Windows not found or is not a Git repo");
      return false;
    } else {
      return true;
    }
  }

  [Action]
  public override ExitCode commit() {
    return Console.batch("save-settings.bat") && base.commit();
  }
}