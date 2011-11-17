@echo off

set SCRIPTS_HOME=%ProgramFiles(x86)%\scripts
"%~dp0\Scripts\deploy-copy" "%~dp0\Scripts" "%ProgramFiles(x86)%\scripts"
"%~dp0\Scripts\deploy-copy" "%~dp0\Macros\macros.doskey" "%ProgramFiles(x86)%\scripts\macros.doskey"
"%~dp0\Scripts\deploy-copy" "%~dp0\Macros\macros.installer" "%ProgramFiles(x86)%\scripts\macros.installer"
"%~dp0\Scripts\setenv" -a SCRIPTS_HOME "%SCRIPTS_HOME%
"%~dp0\Scripts\setenv" -ap PATH %%%%SCRIPTS_HOME%%
"%~dp0\Scripts\setenv" -ap PATH "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\"
regedit /S "%~dp0\macros.installer"

set CYGROOT=c:\cygwin
"%~dp0\Scripts\setenv" -a CYGROOT %CYGROOT%
"%~dp0\Scripts\setenv" -a SHELL %CYGROOT%\bin\bash.exe
"%~dp0\Scripts\setenv" -ap PATH %CYGROOT%\bin
"%~dp0\Scripts\setenv" -a LANG C.UTF-8
"%~dp0\Scripts\setenv" -a LC_ALL C.UTF-8
"%~dp0\Scripts\setenv" -a CYGWIN nodosfilewarning

set CYGHOME=%CYGROOT%\home\xeno_by
"%~dp0\Scripts\setenv" -a CYGHOME %CYGHOME%
if not exist %CYGHOME% mkdir %CYGHOME%
if not exist %CYGROOT%\home\xeno_by mklink %CYGROOT%\home\xeno_by %CYGHOME% /D
if not exist %CYGROOT%\home\administrator mklink %CYGROOT%\home\administrator %CYGHOME% /D

"%~dp0\cygwin.exe"