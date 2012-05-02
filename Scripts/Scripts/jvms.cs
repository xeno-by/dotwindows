// build this with "csc /t:winexe jvms.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"D:\Dropbox\Scratchpad\Stockage\jvms7.pdf");
  }
}