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

public class SublimeScalaScalariform : Sbt {
  public override String project { get { return @"%PROJECTS%\SublimeScala\Scalariform".Expand(); } }

  public SublimeScalaScalariform() : base() {}
  public SublimeScalaScalariform(FileInfo file, Arguments arguments) : base(file) {}
  public SublimeScalaScalariform(DirectoryInfo dir, Arguments arguments) : base(dir) {}

  [Default, MenuItem(description = "Deploy to Sublime", priority = 999.2)]
  public virtual ExitCode deploy() {
    return Console.batch("sbt \"project scalariform\" publish".Expand(), home: project);
  }
}