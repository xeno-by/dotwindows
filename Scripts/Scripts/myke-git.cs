// build this with "csc /t:exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public abstract class Git {
  private FileInfo file;
  private DirectoryInfo dir;

  public Git(FileInfo file) {
    this.file = file;
  }

  public Git(DirectoryInfo dir) {
    this.dir = dir;
  }

  public virtual DirectoryInfo repo { get {
    var wannabe = file != null ? file.Directory : dir;
    while (wannabe != null) {
      var gitIndex = wannabe.GetDirectories().FirstOrDefault(child => child.Name == ".git");
      if (gitIndex != null) return wannabe;
      wannabe = wannabe.Parent;
    }

    return null;
  } }

  public virtual bool verifyRepo() {
    if (repo == null) {
      Console.println("error: {0} is not under Git repository", file.FullName);
      Console.print("Create the repo with the root next to the target (y/n, default is no)? ");
      var answer = Console.readln();
      if (answer.ToLower() == "y" || answer.ToLower() == "yes") {
        Console.batch("git init");
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
    return Console.ui(String.Format("tgit commit \"{0}\"", repo.FullName));
  }

  [Action]
  public virtual ExitCode logall() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tgit log \"{0}\"", repo.FullName));
  }

  [Action]
  public virtual ExitCode logthis() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tgit log \"{0}\"", file.FullName));
  }

  [Action]
  public virtual ExitCode log() {
    return logall();
  }

  [Action]
  public virtual ExitCode push() {
    if (!verifyRepo()) return -1;
    return Console.interactive("git push", home: repo);
  }

  [Action]
  public virtual ExitCode pull() {
    if (!verifyRepo()) return -1;
    return Console.interactive("git pull", home: repo);
  }
}