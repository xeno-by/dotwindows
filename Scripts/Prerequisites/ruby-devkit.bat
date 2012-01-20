@echo off

unzip "%~dp0\ruby-devkit.zip" -d "%TMP%\ruby-devkit"
deploy-copy "%TMP%\ruby-devkit" "%HOMEDRIVE%\RubyDevKit451"
rmdir "%TMP%\ruby-devkit" /S /Q

cd /D "%HOMEDRIVE%\RubyDevKit451"
ruby dk.rb init
ruby dk.rb install