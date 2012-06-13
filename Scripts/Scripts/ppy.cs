// build this with "csc /t:winexe ppy.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"C:\Users\xeno.by\1430227575Python.pdf");
  }
}