@echo off

regedit /e "%DROPBOX%\Software\Windows\Far Manager\console.reg" "HKEY_CURRENT_USER\Console"
call "%~dp0\save-settings-privacy-enable.bat"
regedit /e "%DROPBOX%\Software\Windows\Far Manager\settings-public.reg" "HKEY_CURRENT_USER\Software\Far2"
call "%~dp0\save-settings-privacy-disable.bat"
regedit /e "%DROPBOX%\Software\Windows\Far Manager\settings-private.reg" "HKEY_CURRENT_USER\Software\Far2"

rd "%DROPBOX%\Software\Windows\Far Manager\2.0.b1807" /S /Q
xcopy "%FAR_HOME%" "%DROPBOX%\Software\Windows\Far Manager\2.0.b1807" /H /I /E
