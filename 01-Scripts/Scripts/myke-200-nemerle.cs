// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "nemerle", priority = 200, description =
  "Builds a nemerle program using the command-line provided in the first line of the target file.\r\n" +
  "This no-hassle approach can do the trick for simple programs, but for more complex scenarios consider using msbuild.")]

public class Nemerle : Git {
  private Lines lines;

  public Nemerle(DirectoryInfo dir) : base(dir) {}
  public Nemerle(FileInfo file) : base(file) {}
  public override void init() { env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]n):([0-9]+):([0-9]+)"; }

  public override bool accept() {
    return dir.GetFiles("*.n").Count() > 0;
  }

  [Action]
  public virtual ExitCode clean() {
    dir.GetFiles("*.dll").ToList().ForEach(file1 => file1.Delete());
    dir.GetFiles("*.exe").ToList().ForEach(file1 => file1.Delete());
    return -1;
  }

  [Action]
  public virtual ExitCode rebuild() {
    return clean() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    var macrofiles = dir.GetFiles("macro*.n").ToList();
    var macrocmd = "ncc -no-color -nowarn:10003 -nowarn:10005 -nowarn:168 ";
    macrocmd += "-debug+ ";
    macrocmd += "-r Nemerle.Compiler.dll -r System.Core.dll ";
    macrocmd += ("-t:dll " + String.Join(" ", macrofiles.Select(f => f.Name)) + " ");
    macrocmd += "-out macros.dll";
    var result = println("Compiling macros...") && Console.batch(macrocmd, home: dir) && println("Produced macros.dll") && println();

    var appfiles = dir.GetFiles("*.n").Where(f => !macrofiles.Contains(f)).ToList();
    var appcmd = "ncc -no-color -nowarn:10003 -nowarn:10005 -nowarn:168 ";
    appcmd += "-debug+ ";
    appcmd += "-r System.Core.dll -r Nemerle.Linq.dll ";
    appcmd += "-m macros.dll";
    appcmd += String.Join(" ", appfiles.Select(f => f.Name));
    appcmd += "-out app.exe";
    return result && println("Compiling app...") && Console.batch(appcmd, home: dir) && println("Produced app.exe");
  }

  [Action, Meaningful]
  public virtual ExitCode run(Arguments arguments) {
    return Console.interactive("app.exe", home: dir);
  }
}