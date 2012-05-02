// build this with "csc /t:winexe prof.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"C:\Program Files (x86)\YourKit Java Profiler 11.0.0\bin\profiler.exe");
  }
}