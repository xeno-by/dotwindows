// build this with "csc /t:exe /out:myke.exe /debug+ /r:System.Xml.Linq.dll myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

[Connector(name = "trunk", priority = 999, description =
  "Wraps the development workflow of Scala trunk.")]

public class Trunk : Kep {
  public override String project { get { return @"%PROJECTS%\ScalaSVN".Expand(); } }

  public Trunk() : base() {}
  public Trunk(FileInfo file) : base(file) {}
  public Trunk(DirectoryInfo dir) : base(dir) {}

  public override DirectoryInfo detectRepo() {
    var wannabe = file != null ? file.Directory : dir;

    while (wannabe != null) {
      var svnIndex = wannabe.GetDirectories().FirstOrDefault(child => child.Name == ".svn");
      if (svnIndex != null) return wannabe;
      wannabe = wannabe.Parent;
    }

    return null;
  }

  public override bool verifyRepo() {
    if (repo == null) {
      Console.println("error: {0} is not under SVN repository", file != null ? file.FullName : dir.FullName);
      return false;
    } else {
      return true;
    }
  }

  [Action]
  public override ExitCode commit() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tsvn commit \"{0}\"", repo.GetRealPath().FullName));
  }

  [Action]
  public override ExitCode logall() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tsvn log \"{0}\"", repo.GetRealPath().FullName));
  }

  [Action]
  public override ExitCode logthis() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tsvn log \"{0}\"", (file != null ? (FileSystemInfo)file : dir).GetRealPath().FullName));
  }

  [Action]
  public override ExitCode log() {
    return logall();
  }

  [Action]
  public override ExitCode push() {
    println("error: you can only do commits, not pushes in SVN");
    return -1;
  }

  [Action]
  public override ExitCode pull() {
    if (!verifyRepo()) return -1;
    return Console.interactive("svn update", home: repo.GetRealPath());
  }
}