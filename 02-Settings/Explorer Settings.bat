@echo off

rem also read up http://stackoverflow.com/questions/4491999/configure-windows-explorer-folder-options-through-powershell
regedit /S "%~dp0\Explorer Settings.reg"
