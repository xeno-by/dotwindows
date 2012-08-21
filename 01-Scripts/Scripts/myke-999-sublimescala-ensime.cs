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

[Connector(name = "sublimescala-ensime", priority = 999, description =
  "Wraps the development workflow of the Ensime subproject of the SublimeScala project.")]

public class SublimeScalaEnsime : Sbt {
  public override String project { get { return @"%PROJECTS%\SublimeScala\Ensime".Expand(); } }
  public override String prelude { get { return "++ 2.10.0-SNAPSHOT"; } }

  public SublimeScalaEnsime() : base() {}
  public SublimeScalaEnsime(FileInfo file) : base(file) {}
  public SublimeScalaEnsime(DirectoryInfo dir) : base(dir) {}

  [Action]
  public override ExitCode compile() {
    // return base.compile();
    return deployToSublime();
  }

  [Action, MenuItem(description = "Deploy to Sublime", priority = 999.2)]
  public virtual ExitCode deployToSublime() {
    var result = sbt("stage");
    return result && transplantDir("dist_2.10.0-SNAPSHOT", @"%APPDATA%\Sublime Text 2\Packages\Ensime\server");
  }

  [Action, MenuItem(description = "Deploy to Downloads", priority = 999.1)]
  public virtual ExitCode deployToDownload() {
    var result = deployToSublime();

    var build = File.ReadAllLines(project + "\\project\\Build.scala");
    var line_version = build.Select(line => line.Trim()).Where(line => line.StartsWith("version := ")).Single();
    var s_version = line_version.Substring(line_version.IndexOf(":=") + 2).Trim();
    s_version = s_version.Substring(1, s_version.Length - 3);
    var version = "ensime_2.10-" + s_version;

    var src = @"%APPDATA%\Sublime Text 2\Packages\Ensime\server".Expand();
    var dest = (@"%TMP%\" + version).Expand();
    result = result && transplantDir(src, dest);

    var archive = version + ".tar.gz";
    result = result && Console.batch(String.Format("tar -pvczf {0} {1}", archive, version), home: "%TMP%".Expand());
    var destination = "D:\\" + archive;
    return result && transplantFile("%TMP%\\" + archive, destination) && println("Wrote the archive to " + destination);
    // todo. would be great to programmatically deploy to Github's downloads
  }
}