// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

[Connector(name = "myke", description =
  "Self-compiler of myke.\r\n" +
  "Is actually capable of replacing itself after being compiled.")]

public class Myke : Ubi {
  public Myke(FileInfo file, Lines lines) : base(file, lines) {
  }

  [Action]
  public override ExitCode compile() {
    var mykerun = @"%TMP%\myke-self-compile.bat".Expand();
    File.WriteAllText(mykerun, "@echo off\r\n" + String.Format(@"
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
    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars", "%%MykeContinuation", mykerun);
    return 0;
  }

  [Action]
  public override ExitCode run(Arguments arguments) {
    var mykerun = @"%TMP%\myke-self-compile-and-then-run.bat".Expand();
    File.WriteAllText(mykerun, "@echo off\r\n" + String.Format(@"
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

      ""{2}"" {3}
      set status=%errorlevel%

      echo Windows Registry Editor Version 5.00 > %TMP%\mykestatus.reg
      echo. >> %TMP%\mykestatus.reg
      echo [HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars] >> %TMP%\mykestatus.reg
      echo ""%%%%MykeContinuation""="""" >> %TMP%\mykestatus.reg
      echo ""%%%%MykeStatus""=""%status%"" >> %TMP%\mykestatus.reg
      regedit /s %TMP%\mykestatus.reg
      if not %status% == 0 exit /b %status%

      exit /b %status%
    ", "%SCRIPTS_HOME%".Expand(), compiler, exe.FullName, arguments));

    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars", "%%MykeContinuation", mykerun);
    return 0;
  }
}