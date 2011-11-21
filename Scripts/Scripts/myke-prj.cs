// build this with "csc /t:exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public abstract class Prj {
  public FileInfo file;
  public DirectoryInfo dir;

  public Prj(FileInfo file) {
    this.file = file;
  }

  public Prj(DirectoryInfo dir) {
    this.dir = dir ?? (project == null ? null : new DirectoryInfo(project));
  }

  public virtual String project { get { return null; } }

  public virtual DirectoryInfo root { get {
    if (project != null) {
      return new DirectoryInfo(project);
    }

    if (repo != null) {
      return repo;
    }

    return new DirectoryInfo(".");
  } }

  public virtual bool accept() {
    if (project != null) {
      return dir.EquivalentTo(project);
    }

    return true;
  }

  protected static ExitCode print(String format, params Object[] objs) {
    Console.print(format, objs);
    return 0;
  }

  protected static ExitCode print(String obj) {
    Console.print(obj);
    return 0;
  }

  protected static ExitCode print(Object obj) {
    Console.print(obj);
    return 0;
  }

  protected static ExitCode println(String format, params Object[] objs) {
    Console.println(format, objs);
    return 0;
  }

  protected static ExitCode println(String obj) {
    Console.println(obj);
    return 0;
  }

  protected static ExitCode println(Object obj) {
    Console.println(obj);
    return 0;
  }

  protected static ExitCode println() {
    Console.println();
    return 0;
  }

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
      Console.println("error: {0} is not under Git repository", file.FullName);
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