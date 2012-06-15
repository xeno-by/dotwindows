@echo off

deploy-copy "%~dp0\2.0.1807bis29" "%ProgramFiles(x86)%\Far Manager"

setenv -a FAR_HOME "%ProgramFiles(x86)%\Far Manager
setenv -ap PATH %%%%FAR_HOME%%

rem Don't freak out when these changes aren't applied
rem Regedit cannot deal with exports it generates (the problem is with newlines)
rem Just fire up far and press enter on those files in far
regedit /S "%~dp0\Settings\console.reg"
regedit /S "%~dp0\Settings\settings-public.reg"
regedit /S "%~dp0\Settings\settings-private.reg"