// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public abstract class Git : Prj {
  public Git(FileInfo file) : base(file) {
  }

  public Git(DirectoryInfo dir) : base(dir) {
  }

  public override DirectoryInfo root { get {
    if (project != null) {
      return new DirectoryInfo(project);
    }

    if (repo != null) {
      return repo;
    }

    return new DirectoryInfo(".");
  } }

  public DirectoryInfo repo { get {
    // todo. do we need to cache this?
    return detectRepo();
  } }

  public virtual DirectoryInfo detectRepo() {
    var wannabe = file != null ? file.Directory : dir;
    while (wannabe != null) {
      var gitIndex = wannabe.GetDirectories().FirstOrDefault(child => child.Name == ".git");
      if (gitIndex != null) return wannabe;
      wannabe = wannabe.Parent;
    }

    return null;
  }

  public virtual bool verifyRepo() {
    if (repo == null) {
      Console.println("error: {0} is not under Git repository", file != null ? file.FullName : dir.FullName);
      Console.print("Create the repo with the project root (y/n)? ");
      var answer = Console.readln();
      if (answer == "" || answer.ToLower() == "y" || answer.ToLower() == "yes") {
        Console.batch("git init", home: root);
        return true;
      } else {
        return false;
      }
    } else {
      return true;
    }
  }

  [Action]
  public virtual ExitCode commit() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tgit commit \"{0}\"", repo.GetRealPath().FullName));
  }

  [Action]
  public virtual ExitCode logall() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tgit log \"{0}\"", repo.GetRealPath().FullName));
  }

  [Action]
  public virtual ExitCode logthis() {
    if (!verifyRepo()) return -1;
    file = new FileInfo(Config.originalTarget); // omg hack
    return Console.ui(String.Format("tgit log \"{0}\"", file.GetRealPath().FullName));
  }

  [Action]
  public virtual ExitCode log() {
    return logall();
  }

  [Action]
  public virtual ExitCode push() {
    if (!verifyRepo()) return -1;
    return Console.interactive("git push", home: repo.GetRealPath());
  }

  [Action]
  public virtual ExitCode pull() {
    if (!verifyRepo()) return -1;
    return Console.interactive("git pull", home: repo.GetRealPath());
  }
}