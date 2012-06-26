@echo off
set /P CYGPACKAGES=<"%~dp0\cygwin.packages"
"%~dp0/cygwin.exe" -L -q -l "%~dp0\http%%3a%%2f%%2ftweedo.com%%2fmirror%%2fcygwin%%2f" -R "%CYGROOT%" -P "%CYGPACKAGES%"