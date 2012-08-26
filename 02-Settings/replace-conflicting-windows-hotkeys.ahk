#SingleInstance Force

^+`::
Run, "C:\Users\xeno.by\Far Manager.lnk"
return

#r::
Run, "C:\Program Files (x86)\Launchy\Launchy.exe" /show
return

#t::
Winset, Alwaysontop, , A
return

; http://stackoverflow.com/questions/131955/keyboard-shortcut-to-paste-clipboard-content-into-command-prompt-window-win-xp
; Redefine only when the active window is a console window
#IfWinActive ahk_class ConsoleWindowClass

; Close Command Window with Ctrl+w
$^d::
WinGetTitle sTitle
If (InStr(sTitle, "-")=0) {
  Send EXIT{Enter}
} else {
  Send ^w
}

return

; Use backslash instead of backtick (yes, I am a C++ programmer).
#EscapeChar \

; Paste in command window.
^V::
SendInput {Raw}%clipboard%
return

#IfWinActive