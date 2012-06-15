@echo off
set AHK2EXE=c:\Program Files (x86)\AutoHotkey\Compiler\Ahk2Exe.exe
"%AHK2EXE%" /in "%~dp0\replace-conflicting-windows-hotkeys.ahk" /out "%~dp0\replace-conflicting-windows-hotkeys.exe"