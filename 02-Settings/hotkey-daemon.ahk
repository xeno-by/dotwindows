#SingleInstance Force

^+`::
Run, "C:\Users\xeno.by\Far Manager.lnk"
return

#r::
Run, "C:\Program Files (x86)\Launchy\Launchy.exe" /show
return

Launch_App2::
Run, "C:\Program Files (x86)\scripts\flags.exe"
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
  Send ^d
}
return

; Use backslash instead of backtick (yes, I am a C++ programmer).
#EscapeChar \

; Paste in command window.
^v::
WinGetTitle sTitle
If (InStr(sTitle, "-")=0) {
  SendInput {Raw}%clipboard%
} else {
  Send ^v
}
return

#IfWinActive

;#IfWinActive ahk_class SUMATRA_PDF_FRAME
;f5::
;FormatTime, TimeString,, HH:mm:ss
;FileAppend, F5: %TimeString%\n, D:\\Sumatra.log
;Send {F5}
;return
;
;PgUp::
;FormatTime, TimeString,, HH:mm:ss
;WinGetText, Text
;StringSplit, Lines, Text, \n
;FileAppend, PgUp: %TimeString% at%Lines3%\n, D:\\Sumatra.log
;Send {PgUp}
;return
;
;PgDn::
;FormatTime, TimeString,, HH:mm:ss
;WinGetText, Text
;StringSplit, Lines, Text, \n
;FileAppend, PgDn: %TimeString% at%Lines3%\n, D:\\Sumatra.log
;Send {PgDn}
;return
;
;#IfWinActive
