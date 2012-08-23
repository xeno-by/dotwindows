// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[Connector(name = "maven", priority = 300, description =
  "Supports projects that can be built under maven")]

public class Maven : Git {
  public Maven() : base() {}
  public Maven(FileInfo file) : base(file) {}
  public Maven(DirectoryInfo dir) : base(dir) {}
  public override void init() { env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]scala):([0-9]+)"; }

  public override String project { get { return mvnroot == null ? null : mvnroot.FullName; } }
  public DirectoryInfo mvnroot { get {
    // todo. do we need to cache this?
    return detectmvnroot();
  } }

  public virtual DirectoryInfo detectmvnroot() {
    var wannabe = file != null ? file.Directory : dir;
    while (wannabe != null) {
      var pom = wannabe.GetFiles().FirstOrDefault(child => child.Name == "pom.xml");
      if (pom != null) return wannabe;
      wannabe = wannabe.Parent;
    }

    return null;
  }

  public override bool accept() {
    var projectOverriden = this.GetType().GetProperty("project").DeclaringType != typeof(Maven);
    if (projectOverriden) {
      if (Config.verbose) println("project is overriden. going to base");
      return base.accept();
    } else {
      if (Config.verbose) println("project = {0}, mvnroot = {1}, dir = {2}", project, mvnroot, dir.FullName);
      return mvnroot != null && project.IsChildOrEquivalentTo(mvnroot);
    }
  }

  [Action]
  public virtual ExitCode clean() {
    return Console.batch("mvn clean", home: mvnroot);
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("mvn compile", home: mvnroot);
  }

  [Action]
  public virtual ExitCode rebuild() {
    return Console.batch("mvn clean compile", home: mvnroot);
  }

  [Default, MenuItem(description = "Deploy to .m2", priority = 999.2)]
  public virtual ExitCode deploy() {
    return Console.batch("mvn deploy -DrepositoryId=sublimescala.org -Durl=file://%HOME%/.m2/repository -DpomFile=pom.xml -DaltDeploymentRepository=sublimescala.org::default::file://%HOME%/.m2/repository".Expand(), home: mvnroot);
  }
}