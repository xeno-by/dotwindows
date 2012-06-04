// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "vcs", priority = 100, description =
  "Version control services: only git is supported at the moment")]

public class Vcs : Git {
  public Vcs() : base((DirectoryInfo)null) {}
  public Vcs(FileInfo file) : base(file) {}
  public Vcs(DirectoryInfo dir) : base(dir) {}

  public override bool accept() {
    if (Config.verbose) println("repo = {0}, dir = {1}", repo, dir.FullName);
    if (dir.IsChildOrEquivalentTo("%SCRIPTS_HOME%".Expand())) return true;
    return dir.IsChildOrEquivalentTo(repo);
  }
}