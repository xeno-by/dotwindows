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

[Connector(name = "sublimescala-scalariform", priority = 999, description =
  "Wraps the development workflow of the Scalariform subproject of the SublimeScala project.")]

public class SublimeScalaScalariform : Git {
  public override String project { get { return @"%PROJECTS%\SublimeScala\Scalariform".Expand(); } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public SublimeScalaScalariform() : base() {}
  public SublimeScalaScalariform(FileInfo file, Arguments arguments) : base(file) {}
  public SublimeScalaScalariform(DirectoryInfo dir, Arguments arguments) : base(dir) {}

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("sbt \"project scalariform\" publish".Expand(), home: root);
  }
}