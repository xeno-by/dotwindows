@echo off

rem 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c is High Performance
rem run powercfg /list to learn more
powercfg -import "%~dp0\Power Scheme.dat"
powercfg -s 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
regedit /S "%~dp0\Power Scheme.reg"