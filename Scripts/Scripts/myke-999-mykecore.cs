// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

[Connector(name = "myke-core", priority = 999, description =
  "Self-compiler of myke.\r\n" +
  "Is actually capable of replacing itself after being compiled.")]

public class MykeCore : Csc {
  public MykeCore(FileInfo file, Lines lines) : base(file, lines) {
  }

  public override bool accept() {
    return base.accept() && dir.IsChildOrEquivalentTo("%SCRIPTS_HOME%".Expand()) && file.Name.StartsWith("myke");
  }

  [Action]
  public override ExitCode compile() {
    var mykecompile = @"%TMP%\myke-self-compile.bat".Expand();
    File.WriteAllText(mykecompile, "@echo off\r\n" + String.Format(@"
      rem Wait for self to exit
      sleep 100

      cd /D ""{0}""
      {1}
      set status=%errorlevel%

      echo Windows Registry Editor Version 5.00 > %TMP%\mykestatus.reg
      echo. >> %TMP%\mykestatus.reg
      echo [HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars] >> %TMP%\mykestatus.reg
      echo ""%%%%MykeContinuation""="""" >> %TMP%\mykestatus.reg
      echo ""%%%%MykeStatus""=""%status%"" >> %TMP%\mykestatus.reg
      regedit /s %TMP%\mykestatus.reg
      if not %status% == 0 exit /b %status%

      exit /b %status%
    ", "%SCRIPTS_HOME%".Expand(), compiler));

    if (Config.dryrun) println("cont: " + mykecompile);
    else Registry.SetValue(@"HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars", "%%MykeContinuation", mykecompile);

    return 0;
  }

  [Action]
  public override ExitCode run(Arguments arguments) {
    return runWithCompile(arguments);
  }

  [Action]
  public virtual ExitCode runWithCompile(Arguments arguments) {
    var mykerun = @"%TMP%\myke-self-compile-and-then-run.bat".Expand();
    File.WriteAllText(mykerun, "@echo off\r\n" + String.Format(@"
      rem Wait for self to exit
      sleep 100

      cd /D ""{0}""
      {1}
      set status=%errorlevel%

      echo Windows Registry Editor Version 5.00 > %TMP%\mykestatus.reg
      echo. >> %TMP%\mykestatus.reg
      echo [HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars] >> %TMP%\mykestatus.reg
      echo ""%%%%MykeContinuation""="""" >> %TMP%\mykestatus.reg
      echo ""%%%%MykeStatus""=""%status%"" >> %TMP%\mykestatus.reg
      regedit /s %TMP%\mykestatus.reg
      if not %status% == 0 exit /b %status%

      myke runWithoutCompile ""{2}"" {3}
      set status=%errorlevel%

      echo Windows Registry Editor Version 5.00 > %TMP%\mykestatus.reg
      echo. >> %TMP%\mykestatus.reg
      echo [HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars] >> %TMP%\mykestatus.reg
      echo ""%%%%MykeContinuation""="""" >> %TMP%\mykestatus.reg
      echo ""%%%%MykeStatus""=""%status%"" >> %TMP%\mykestatus.reg
      regedit /s %TMP%\mykestatus.reg
      if not %status% == 0 exit /b %status%

      exit /b %status%
    ", "%SCRIPTS_HOME%".Expand(), compiler, Config.target, arguments));

    if (Config.dryrun) println("cont: " + mykerun);
    else Registry.SetValue(@"HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars", "%%MykeContinuation", mykerun);

    return 0;
  }

  [Action]
  public virtual ExitCode runWithoutCompile(Arguments arguments) {
    Func<String> readArguments = () => Console.readln(prompt: "Run arguments", history: String.Format("run {0}", exe.FullName));
    return Console.interactive(exe.FullName.GetShortPath() + " " + (arguments.Count > 0 ? arguments.ToString() : readArguments()), home: root);
  }
}