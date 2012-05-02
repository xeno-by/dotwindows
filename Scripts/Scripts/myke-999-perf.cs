// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

[Connector(name = "perf_good", priority = 999, description =
  "Wraps the development workflow of the Perf_Good revision of Kepler.")]

public class KepPerfGood : Kep {
  public override String project { get { return @"%PROJECTS%\Perf_Good".Expand(); } }

  public KepPerfGood() : base() {}
  public KepPerfGood(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public KepPerfGood(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}

  [Action]
  public override ExitCode runTest() {
    return rebuildAltCompiler();
  }
}

[Connector(name = "perf_bad", priority = 999, description =
  "Wraps the development workflow of the Perf_Bad revision of Kepler.")]

public class KepPerfBad : Kep {
  public override String project { get { return @"%PROJECTS%\Perf_Bad".Expand(); } }

  public KepPerfBad() : base() {}
  public KepPerfBad(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public KepPerfBad(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}

  [Action]
  public override ExitCode runTest() {
    return rebuildAltCompiler();
  }
}

[Connector(name = "perf_quickcomp", priority = 999, description =
  "Wraps the development workflow of the Perf_QuickComp revision of Kepler.")]

public class KepPerfQuickComp : Kep {
  public override String project { get { return @"%PROJECTS%\Perf_QuickComp".Expand(); } }

  public KepPerfQuickComp() : base() {}
  public KepPerfQuickComp(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public KepPerfQuickComp(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}

  [Action]
  public override ExitCode runTest() {
    return rebuildAltCompiler();
  }
}

[Connector(name = "perf_quicklib", priority = 999, description =
  "Wraps the development workflow of the Perf_QuickLib revision of Kepler.")]

public class KepPerfQuickLib : Kep {
  public override String project { get { return @"%PROJECTS%\Perf_QuickLib".Expand(); } }

  public KepPerfQuickLib() : base() {}
  public KepPerfQuickLib(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public KepPerfQuickLib(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}

  [Action]
  public override ExitCode runTest() {
    return rebuildAltCompiler();
  }
}
