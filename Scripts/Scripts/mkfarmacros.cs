using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class App {
  public static void Main(String[] args) {
    var lines = new List<String>();

//    var from = (int)'a';
//    var to = (int)'z';
    var from = (int)'0';
    var to = (int)'9';
    for (var i = from; i <= to; ++i) {
      var c = (Char)i;
      lines.Add(@"[HKEY_CURRENT_USER\Software\Far2\KeyMacros\Shell\Alt" + Char.ToUpper(c) + "]");
      lines.Add(@"""Sequence""=""Alt" + Char.ToUpper(c) + @" BS * " + c + @"""");
      lines.Add(@"""DisableOutput""=dword:00000001");
      lines.Add("");
    }

    File.AppendAllLines(@"d:\foo", lines);
  }
}