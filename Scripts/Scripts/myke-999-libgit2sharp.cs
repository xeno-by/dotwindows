// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

[Connector(name = "libgit2sharp", priority = 999, description =
  "Wraps the development workflow of LibGit2Sharp.")]

public class LibGit2SharpProject : Git {
  public override String project { get { return @"%PROJECTS%\LibGit2Sharp".Expand(); } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return dir.IsChildOrEquivalentTo(project);
  }

  public LibGit2SharpProject() : base() { init(); }
  public LibGit2SharpProject(FileInfo file) : base(file) { init(); }
  public LibGit2SharpProject(DirectoryInfo dir) : base(dir) { init(); }
  private void init() {
    env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]cs)\\(([0-9]+),[0-9]+\\)";
    env["ResultBaseDir"] = project + "\\" + "LibGit2Sharp";
  }

  [Default, Action]
  public virtual ExitCode compile() {
    return Console.batch("msbuild LibGit2Sharp.sln", home: root) && deployDebug();
  }

  [Action]
  public override ExitCode runTest() {
    return Console.batch("msbuild CI-build.msbuild /target:Test", home: root);
  }

  [Action, MenuItem(description = "Deploy Debug to Ubi", priority = 999.2)]
  public virtual ExitCode deployDebug() {
    var mykedeploy = @"%TMP%\myke-self-deploy-debug.bat".Expand();
    File.WriteAllText(mykedeploy, "@echo off\r\n" + String.Format(@"
      rem Wait for self to exit
      sleep 100

      copy ""{0}\LibGit2Sharp\bin\Debug\LibGit2Sharp.dll"" ""%SCRIPTS_HOME%\LibGit2Sharp.dll""
      copy ""{0}\LibGit2Sharp\bin\Debug\LibGit2Sharp.pdb"" ""%SCRIPTS_HOME%\LibGit2Sharp.pdb""
      set status=%errorlevel%

      echo Windows Registry Editor Version 5.00 > %TMP%\mykestatus.reg
      echo. >> %TMP%\mykestatus.reg
      echo [HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars] >> %TMP%\mykestatus.reg
      echo ""%%%%MykeContinuation""="""" >> %TMP%\mykestatus.reg
      echo ""%%%%MykeStatus""=""%status%"" >> %TMP%\mykestatus.reg
      if not %status% == 0 echo ""%%%%MykeMeaningful""=""1"" >> %TMP%\mykestatus.reg
      echo ""%%%%MykeWorkingDir""=""{1}"" >> %TMP%\mykestatus.reg
      regedit /s %TMP%\mykestatus.reg
      if not %status% == 0 exit /b %status%

      exit /b %status%
    ", project, project.Replace(@"\", @"\\")));

    if (Config.dryrun) println("cont: " + mykedeploy);
    else Registry.SetValue(@"HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars", "%%MykeContinuation", mykedeploy);

    return 0;
  }

  [Action, MenuItem(description = "Deploy Release to Ubi", priority = 999.2)]
  public virtual ExitCode deployRelease() {
    var status = Console.batch("msbuild CI-build.msbuild", home: root);
    if (!status) return status;

    var mykedeploy = @"%TMP%\myke-self-deploy-release.bat".Expand();
    File.WriteAllText(mykedeploy, "@echo off\r\n" + String.Format(@"
      rem Wait for self to exit
      sleep 100

      copy ""{0}\Build\LibGit2Sharp.dll"" ""%SCRIPTS_HOME%\LibGit2Sharp.dll""
      copy ""{0}\Build\LibGit2Sharp.pdb"" ""%SCRIPTS_HOME%\LibGit2Sharp.pdb""
      set status=%errorlevel%

      echo Windows Registry Editor Version 5.00 > %TMP%\mykestatus.reg
      echo. >> %TMP%\mykestatus.reg
      echo [HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars] >> %TMP%\mykestatus.reg
      echo ""%%%%MykeContinuation""="""" >> %TMP%\mykestatus.reg
      echo ""%%%%MykeStatus""=""%status%"" >> %TMP%\mykestatus.reg
      if not %status% == 0 echo ""%%%%MykeMeaningful""=""1"" >> %TMP%\mykestatus.reg
      echo ""%%%%MykeWorkingDir""=""{1}"" >> %TMP%\mykestatus.reg
      regedit /s %TMP%\mykestatus.reg
      if not %status% == 0 exit /b %status%

      exit /b %status%
    ", project, project.Replace(@"\", @"\\")));

    if (Config.dryrun) println("cont: " + mykedeploy);
    else Registry.SetValue(@"HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars", "%%MykeContinuation", mykedeploy);

    return 0;
  }
}