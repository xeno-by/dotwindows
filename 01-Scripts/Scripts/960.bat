@echo off
deploy-copy "%SOFTWARE%\Far Manager\Far Manager - 960.lnk" "%USERPROFILE%\Far Manager.lnk"
regedit /S "%~dp0\960.reg"