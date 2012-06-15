@echo off

rd "%DROPBOX%\Software\Windows\01-Scripts\Macros" /S /Q > NUL
mkdir "%DROPBOX%\Software\Windows\01-Scripts\Macros"
copy "%SCRIPTS_HOME%\macros.doskey" "%DROPBOX%\Software\Windows\01-Scripts\Macros\macros.doskey"
copy "%SCRIPTS_HOME%\macros.installer" "%DROPBOX%\Software\Windows\01-Scripts\Macros\macros.installer"
rd "%DROPBOX%\Software\Windows\01-Scripts\Scripts" /S /Q > NUL
xcopy "%SCRIPTS_HOME%" "%DROPBOX%\Software\Windows\01-Scripts\Scripts" /H /I /E

regedit /s "%SCRIPTS_HOME%\myke-far.reg"
copy "%SCRIPTS_HOME%\myke-backend.el" "%HOME%\.emacs.d\solutions\myke-backend.el" /Y
copy "%SCRIPTS_HOME%\myke-frontend.el" "%HOME%\.emacs.d\solutions\myke-frontend.el" /Y

regedit /e "%DROPBOX%\Software\Windows\Far Manager\Settings\console.reg" "HKEY_CURRENT_USER\Console"
call "%~dp0\save-settings-privacy-enable.bat"
regedit /e "%DROPBOX%\Software\Windows\Far Manager\Settings\settings-public.reg" "HKEY_CURRENT_USER\Software\Far2"
call "%~dp0\save-settings-privacy-disable.bat"
regedit /e "%DROPBOX%\Software\Windows\Far Manager\Settings\settings-private.reg" "HKEY_CURRENT_USER\Software\Far2"
rd "%DROPBOX%\Software\Windows\Far Manager\2.0.1807bis29" /S /Q > NUL
xcopy "%FAR_HOME%" "%DROPBOX%\Software\Windows\Far Manager\2.0.1807bis29" /H /I /E

regedit /e "%TMP%\User.reg" "HKEY_CURRENT_USER\Environment"
more +0 "%TMP%\User.reg" > "%TMP%\User-trimmed.reg"
regedit /e "%TMP%\Machine.reg" "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment"
more +2 "%TMP%\Machine.reg" > "%TMP%\Machine-trimmed.reg"
copy "%TMP%\User-trimmed.reg"+"%TMP%\Machine-trimmed.reg" "%DROPBOX%\Software\Windows\01-Scripts\02-environment.reg"

regedit /e "%DROPBOX%\Software\Windows\01-Scripts\02-myke-private.reg" "HKEY_CURRENT_USER\Software\Myke"

copy "%USERPROFILE%\.gitconfig" "%DROPBOX%\Software\Windows\Development\Git\Client\.gitconfig"
copy "%USERPROFILE%\.gitignore" "%DROPBOX%\Software\Windows\Development\Git\Client\.gitignore"
copy "%USERPROFILE%\.gitconfig" "%DROPBOX%\Software\Windows\02-Settings\Dotfiles\.gitconfig"
copy "%USERPROFILE%\.gitignore" "%DROPBOX%\Software\Windows\02-Settings\Dotfiles\.gitignore"
copy "%USERPROFILE%\.scala_autorun" "%DROPBOX%\Software\Windows\02-Settings\Dotfiles\.scala_autorun"
copy "%USERPROFILE%\.scala_history" "%DROPBOX%\Software\Windows\02-Settings\Dotfiles\.scala_history"

cygcheck -c -d|sed -e "1,2d" -e 's/ .*$//'|gawk 'BEGIN{OFS="\n";RS="";OFS=","} {$1=$1}1' > "%DROPBOX%\Software\Windows\01-Scripts\Prerequisites\cygwin.packages"