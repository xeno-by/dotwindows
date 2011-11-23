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

[Connector(name = "kep", priority = 999, description =
  "Wraps the development workflow of project Kepler.\r\n" +
  "Uses ant for building, itself for a repl, runs Reflection and doesn't support tests yet.")]

public class Kep : Git {
  public override String project { get { return @"%PROJECTS%\Kepler".Expand(); } }

  public Kep() : base() {}
  public Kep(FileInfo file) : base(file) {}
  public Kep(DirectoryInfo dir) : base(dir) {}

  [Action]
  public virtual ExitCode rebuild() {
    var status = Console.batch("ant all.clean -buildfile build.xml", home: root);
    return status && println() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("ant build -buildfile build.xml", home: root);
  }

  [Action]
  public virtual ExitCode repl() {
    var status = compile();
    return status && println() && Console.interactive(@"build\pack\bin\scala.bat -deprecation", home: root);
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    var status = compile();
    return status && println() && new Rf().run(arguments);
  }

  [Action]
  public virtual ExitCode test() {
    var project = new FileInfo(root + "\\.project");
    if (!project.Exists) {
      println("error: project file not found");
      return -1;
    }

    var doc = XDocument.Load(@"C:\Projects\Kepler\.project");
    if (doc == null) {
      println("error: project file has unexpected format");
      return -1;
    }

    var tests = ((IEnumerable<Object>)doc.XPathEvaluate("//linkedResources//locationURI[starts-with(text(), 'PROJECT_LOC/test/files') and substring(text(), string-length(text()) - 5) = '.scala']/text()")).Cast<XText>().Select(text => text.Value).ToList();
    var home = new DirectoryInfo(root.FullName + "\\test");
    return Console.batch("partest " + String.Join(" ", tests.Select(test => test.Substring(17))), home: home);
  }
}