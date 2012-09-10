@echo off
set AHK2EXE=c:\Program Files (x86)\AutoHotkey\Compiler\Ahk2Exe.exe
"%AHK2EXE%" /in "%~dp0\hotkey-daemon.ahk" /out "%~dp0\hotkey-daemon.exe"