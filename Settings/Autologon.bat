@echo off

set /P username= Username: 
set /P password= Password: 

set template=%~dp0\Autologon.reg
set regfile=%TMP%\Autologon.reg
copy "%template%" "%regfile%" >nul

perl -i.bak -npE "s/%%USERNAME%%/%USERNAME%/g" "%regfile%"
perl -i.bak -npE "s/%%PASSWORD%%/%PASSWORD%/g" "%regfile%"
regedit /S "%regfile%"
rm "%regfile%"