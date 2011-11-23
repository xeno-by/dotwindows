// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "ubidir", description =
  "Version-control abstractions for ubi.\r\n" +
  "Effectively maps ubi onto %SOFTWARE% and performs copy/paste transfers before commits.")]

public class Ubidir : Git {
  public Ubidir(DirectoryInfo dir) : base(dir) {
  }

  public override bool accept() {
    return base.accept() && dir.EquivalentTo("%SCRIPTS_HOME%".Expand());
  }

  public override DirectoryInfo detectRepo() {
    var sw = new DirectoryInfo(@"%SOFTWARE%".Expand());

    var gitIndex = sw.GetDirectories().FirstOrDefault(child => child.Name == ".git");
    if (gitIndex == null) return null;

    return sw;
  }

  public override bool verifyRepo() {
    if (repo == null) {
      Console.println("%SOFTWARE% is not found or is not a Git repo");
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