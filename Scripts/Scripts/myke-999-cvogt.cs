// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

[Connector(name = "cvogt", priority = 999, description =
  "Wraps the development workflow of upstream Scala.")]

public class Cvogt : Kep {
  public override String project { get { return @"%PROJECTS%\Cvogt".Expand(); } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public Cvogt() : base() {}
  public Cvogt(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public Cvogt(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}
}