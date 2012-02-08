@echo off
deploy-copy "%SOFTWARE%\Far Manager\Far Manager - 1680.lnk" "%USERPROFILE%\Desktop\Far Manager.lnk"
regedit /S "%~dp0\1680.reg"