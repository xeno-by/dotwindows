@echo off
deploy-copy "%SOFTWARE%\Far Manager\Far Manager - 1920.lnk" "%USERPROFILE%\Desktop\Far Manager.lnk"
regedit /S "%~dp0\1920.reg"