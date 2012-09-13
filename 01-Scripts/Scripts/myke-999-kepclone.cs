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
using ZetaLongPaths;

[Connector(name = "kep-clone", priority = 998, description =
  "Wraps the development workflow of a Kepler clone")]
public class KepClone : Kep {
  public KepClone() : base() { }
  public KepClone(FileInfo file, Arguments arguments) : base(file, arguments) { this.arguments = arguments; }
  public KepClone(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) { this.arguments = arguments; }

  public override bool accept() { return dir.FullName.GetRealPath().ToUpper().StartsWith(@"%PROJECTS%\Kepler".Expand().GetRealPath().ToUpper()); }
  public override String project { get {
    var root = new DirectoryInfo(dir.FullName.GetRealPath());
    while (root.Parent != null) {
      if (root.Parent.FullName == @"%PROJECTS%".Expand()) return root.FullName;
      root = root.Parent;
    }
    return root.Parent == null ? null : root.FullName;
  } }
}