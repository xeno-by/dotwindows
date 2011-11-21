// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "git", priority = -1, description =
  "Provides common interface for Git version-control system.\r\n" +
  "Despite of lacking other operations, it is still useful to be bound to a shortcut in shell.")]

public class GitRaw : Git {
  public GitRaw(DirectoryInfo dir) : base(dir) {
  }

  // if not this line, GitRaw would be merged with Git
  public override String project { get { return repo == null ? null : repo.FullName; } }
}