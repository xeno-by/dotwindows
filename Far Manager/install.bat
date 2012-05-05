@echo off

deploy-copy "%~dp0\2.0.1807bis29" "%ProgramFiles(x86)%\Far Manager"

setenv -a FAR_HOME "%ProgramFiles(x86)%\Far Manager
setenv -ap PATH %%%%FAR_HOME%%

regedit /S "%~dp0\Settings\console.reg"
regedit /S "%~dp0\Settings\settings.reg"