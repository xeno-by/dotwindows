@echo off

deploy-symlink "%DROPBOX%\Scratchpad\Documents" "%USERPROFILE%\Documents\Scratchpad"
deploy-symlink "%XENODRIVE%\Downloads" "%USERPROFILE%\Downloads"
deploy-symlink "%SOFTWARE%\02-Settings\Explorer Favorites" "%USERPROFILE%\Links"
deploy-symlink "%DROPBOX%\Public\Library" "%USERPROFILE%\Library"
deploy-symlink "%XENODRIVE%\Music" "%USERPROFILE%\Music"
deploy-symlink "%XENODRIVE%\Video" "%USERPROFILE%\Videos"
if not exist "%HOMEDRIVE%\Projects" mkdir "%HOMEDRIVE%\Projects"
deploy-symlink "%HOMEDRIVE%\Projects" "%USERPROFILE%\Projects"
deploy-symlink "%DROPBOX%\Wallpapers" "%USERPROFILE%\Pictures\Wallpapers"
deploy-symlink "%DROPBOX%\Passwords" "%USERPROFILE%\.ssh"

if defined CYGROOT (
  deploy-symlink "%XENODRIVE%" "%CYGROOT%\media\XENO"
)

if defined CYGHOME (
  deploy-symlink "%DROPBOX%\Scratchpad\Documents" "%CYGHOME%\Documents"
  deploy-symlink "%XENODRIVE%\Downloads" "%CYGHOME%\Downloads"
  deploy-symlink "%DROPBOX%\Public\Library" "%CYGHOME%\Library"
  deploy-symlink "%XENODRIVE%\Music" "%CYGHOME%\Music"
  deploy-symlink "%XENODRIVE%\Video" "%CYGHOME%\Videos"
  deploy-symlink "%HOMEDRIVE%\Projects" "%CYGHOME%\Projects"
  deploy-symlink "%USERPROFILE%\.ssh" "%CYGHOME%\.ssh"
  deploy-symlink "%USERPROFILE%\.ssh" "%CYGHOME%\..\xeno.by\.ssh"
)

deploy-symlink "%SOFTWARE%\..\Opera\sessions" "%APPDATA%\Opera\Opera\sessions"
deploy-symlink "%SOFTWARE%\..\Opera\toolbar" "%APPDATA%\Opera\Opera\toolbar"
deploy-symlink "%SOFTWARE%\..\Opera\skin" "%ProgramFiles(x86)%\Opera\skin"
deploy-symlink "%SOFTWARE%\..\Opera\styles" "%ProgramFiles(x86)%\Opera\styles"
deploy-symlink "%SOFTWARE%\..\Opera\ui" "%ProgramFiles(x86)%\Opera\ui"
deploy-symlink "%SOFTWARE%\..\Opera\bookmarks.adr" "%APPDATA%\Opera\Opera\bookmarks.adr"
deploy-symlink "%SOFTWARE%\..\Opera\bookmarks.adr.pre_sync" "%APPDATA%\Opera\Opera\bookmarks.adr.pre_sync"
deploy-symlink "%SOFTWARE%\..\Opera\global_history.dat" "%APPDATA%\Opera\Opera\global_history.dat"
deploy-symlink "%SOFTWARE%\..\Opera\operaprefs.ini" "%APPDATA%\Opera\Opera\operaprefs.ini"
deploy-symlink "%SOFTWARE%\..\Opera\search.ini" "%APPDATA%\Opera\Opera\search.ini"
deploy-symlink "%SOFTWARE%\..\Opera\search.ini.pre_sync" "%APPDATA%\Opera\Opera\search.ini.pre_sync"
deploy-symlink "%SOFTWARE%\..\Opera\search_field_history.dat" "%APPDATA%\Opera\Opera\search_field_history.dat"
deploy-symlink "%SOFTWARE%\..\Opera\speeddial.ini" "%APPDATA%\Opera\Opera\speeddial.ini"
deploy-symlink "%SOFTWARE%\..\Opera\speeddial.ini.pre_sync" "%APPDATA%\Opera\Opera\speeddial.ini.pre_sync"
deploy-symlink "%SOFTWARE%\..\Opera\typed_history.xml" "%APPDATA%\Opera\Opera\typed_history.xml"
deploy-symlink "%SOFTWARE%\..\Opera\typed_history.xml.pre_sync" "%APPDATA%\Opera\Opera\typed_history.xml.pre_sync"
deploy-symlink "%SOFTWARE%\..\Opera\urlfilter.ini" "%APPDATA%\Opera\Opera\urlfilter.ini"
deploy-symlink "%SOFTWARE%\..\Opera\urlfilter.ini.pre_sync" "%APPDATA%\Opera\Opera\urlfilter.ini.pre_sync"
deploy-symlink "%SOFTWARE%\..\Opera\wand.dat" "%APPDATA%\Opera\Opera\wand.dat"
