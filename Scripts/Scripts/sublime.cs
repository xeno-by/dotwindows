// build this with "csc /t:winexe sublime.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    var path = @"C:\Program Files\Sublime Text 2\sublime_text.exe";
    Process.Start(path, String.Join(" ", args));
  }
}