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

[Connector(name = "sublimescala-scalarefactoring", priority = 999, description =
  "Wraps the development workflow of the ScalaRefactoring subproject of the SublimeScala project.")]

public class SublimeScalaScalaRefactoring : Git {
  public override String project { get { return @"%PROJECTS%\SublimeScala\ScalaRefactoring".Expand(); } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public SublimeScalaScalaRefactoring() : base() {}
  public SublimeScalaScalaRefactoring(FileInfo file, Arguments arguments) : base(file) {}
  public SublimeScalaScalaRefactoring(DirectoryInfo dir, Arguments arguments) : base(dir) {}

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("mvn deploy -DrepositoryId=sublimescala.org -Durl=file://%HOME%/.m2/repository -DpomFile=pom.xml -DaltDeploymentRepository=sublimescala.org::default::file://%HOME%/.m2/repository".Expand(), home: root);
  }
}