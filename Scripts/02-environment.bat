@echo off

set HOME=%HOMEDRIVE%%HOMEPATH%
setenv -au HOME "%HOME%"
set XENODRIVE=d:
setenv -a XENODRIVE "%XENODRIVE%"
set MOBIDRIVE=e:
setenv -a MOBIDRIVE "%MOBIDRIVE%"
set DROPBOX=%XENODRIVE%\Dropbox
setenv -a DROPBOX "%DROPBOX%"
set SOFTWARE=%DROPBOX%\Software\Windows
setenv -a SOFTWARE "%SOFTWARE%"

regedit /s "%~dp0\02-environment.reg"