// build this with "csc /t:exe regread.cs"

using System;
using System.Linq;
using Microsoft.Win32;

public class App {
  public static int Main(String[] args) {
    if (args.Length != 1 || args[0] == "/?" || args[0] == "--help") {
      Console.WriteLine(@"usage: regread PATH\TO\VALUE");
      return -1;
    }

    var full = args[0].Replace("/", "\\");
    var iof = full.LastIndexOf("\\");
    if (iof == -1) return -1;

    var key = full.Substring(0, iof);
    var value = full.Substring(iof + 1);

    iof = key.IndexOf("\\");
    if (iof == -1) return -1;
    var hive = key.Substring(0, iof);
    key = key.Substring(iof + 1);

    RegistryKey rhive = null;
    switch (hive) {
      case "HKCU":
      case "HKEY_CURRENT_USER":
        rhive = Registry.CurrentUser;
        break;

      case "HKLM":
      case "HKEY_LOCAL_MACHINE":
        rhive = Registry.LocalMachine;
        break;

      case "HKCR":
      case "HKEY_CLASSES_ROOT":
        rhive = Registry.ClassesRoot;
        break;

      case "HKU":
      case "HKEY_USERS":
        rhive = Registry.Users;
        break;

      case "HKPD":
      case "HKEY_PERFORMANCE_DATA":
        rhive = Registry.PerformanceData;
        break;

      case "HKCC":
      case "HKEY_CURRENT_CONFIG":
        rhive = Registry.CurrentConfig;
        break;

      default:
          return -1;
    }

    var rkey = rhive.OpenSubKey(key);
    if (rkey == null) return -1;

    if (!rkey.GetValueNames().Contains(value)) return -1;
    Console.WriteLine(rkey.GetValue(value));
    return 0;
  }
}