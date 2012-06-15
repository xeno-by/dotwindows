// build this with "csc /t:winexe ens.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"http://aemon.com/file_dump/ensime_manual.html");
  }
}