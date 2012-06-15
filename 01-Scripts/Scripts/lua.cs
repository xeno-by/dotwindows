// build this with "csc /t:winexe lua.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"C:\Program Files (x86)\Far Manager\Plugins\luafar4editor\doc\lf4ed_manual.chm");
  }
}