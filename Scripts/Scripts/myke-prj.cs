// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

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
}