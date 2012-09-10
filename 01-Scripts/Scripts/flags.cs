// build this with "csc /t:winexe flags.cs"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new MainForm());
  }
}

public class MainForm: Form {
  TextBox tb;
  TextBox lb;

  public MainForm() {
    this.Height = 200;
    this.Width = 300;
    this.Location = new Point(500, 500);
    this.StartPosition = FormStartPosition.Manual;

    this.MinimizeBox = false;
    this.MaximizeBox = false;
    this.KeyPreview = true;
    this.KeyDown += (o, e) => {
      if (e.KeyCode == Keys.Escape) Application.Exit();
    };

    var panel = new Panel();
    panel.Dock = DockStyle.Fill;
    this.Controls.Add(panel);
    tb = new TextBox();
    tb.Dock = DockStyle.Top;
    lb = new TextBox();
    lb.Dock = DockStyle.Fill;
    lb.Multiline = true;
    lb.ReadOnly = true;
    panel.Controls.Add(lb);
    panel.Controls.Add(tb);
    this.ActiveControl = tb;

    var fname = @"C:\Projects\Kepler\src\reflect\scala\reflect\internal\Flags.scala";
    if (!File.Exists(fname)) Application.Exit();
    var lines = File.ReadAllLines(fname).Select(line => line.Trim()).ToList();
    lines = lines.SkipWhile(line => !line.StartsWith("//  0:")).ToList();
    lines = lines.TakeWhile(line => line != "").ToList();
    lines = lines.Select(line => line.Substring(6).Trim()).ToList();
    lines = lines.Select(line => line.Replace("/M", "")).ToList();
    lines = lines.Select(line => String.Join("/", line.Split(new []{" "}, StringSplitOptions.RemoveEmptyEntries))).ToList();

    tb.TextChanged += (o, e) => {
      ulong flags;
      if (ulong.TryParse(tb.Text, out flags)) {
        var buffer = new List<String>();
        for (var i = 0; i < 64; ++i) {
          if ((flags & (1UL << i)) == (1UL << i)) buffer.Add(lines[i]);
        }
        lb.Text = String.Join(" | ", buffer.ToArray());
      } else {
        lb.Text = "Invalid format";
      }
    };

    ulong initFlags;
    if (ulong.TryParse(Clipboard.GetText(), out initFlags)) tb.Text = initFlags.ToString();
  }
}