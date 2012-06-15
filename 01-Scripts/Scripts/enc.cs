// build this with "csc /t:winexe enc.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"C:\Program Files (x86)\Far Manager\Encyclopedia\FarEncyclopedia.ru.chm");
  }
}