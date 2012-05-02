// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "maven", priority = 300, description =
  "Supports projects that can be built under maven")]

public class Maven : Git {
  public Maven() : base() {}
  public Maven(FileInfo file) : base(file) {}
  public Maven(DirectoryInfo dir) : base(dir) {}

  public override String project { get { return mvnroot == null ? null : mvnroot.FullName; } }
  public DirectoryInfo mvnroot { get {
    // todo. do we need to cache this?
    return detectmvnroot();
  } }

  public virtual DirectoryInfo detectmvnroot() {
    var wannabe = file != null ? file.Directory : dir;
    while (wannabe != null) {
      var pom = wannabe.GetFiles().FirstOrDefault(child => child.Name == "pom.xml");
      if (pom != null) return wannabe;
      wannabe = wannabe.Parent;
    }

    return null;
  }

  public override bool accept() {
    if (Config.verbose) println("mvnroot = {0}, dir = {1}", mvnroot, dir.FullName);
    return dir.IsChildOrEquivalentTo(mvnroot);
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("mvn compile", home: root);
  }
}