@echo off
deploy-copy "%SOFTWARE%\Far Manager\Far Manager - 1366.lnk" "%USERPROFILE%\Desktop\Far Manager.lnk"
regedit /S "%~dp0\1366.reg"
