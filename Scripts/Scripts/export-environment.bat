@echo off

regedit /e "%TMP%\User.reg" "HKEY_CURRENT_USER\Environment"  
more +0 "%TMP%\User.reg" > "%TMP%\User-trimmed.reg"
regedit /e "%TMP%\Machine.reg" "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" 
more +2 "%TMP%\Machine.reg" > "%TMP%\Machine-trimmed.reg"
copy "%TMP%\User-trimmed.reg"+"%TMP%\Machine-trimmed.reg" "%DROPBOX%\Software\Windows\Scripts\02-environment.reg"
