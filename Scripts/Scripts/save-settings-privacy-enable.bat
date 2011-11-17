@echo off

regedit /e "%TMP%\ViewerLastPositions.reg" "HKEY_CURRENT_USER\Software\Far2\Viewer\LastPositions"
regedit /e "%TMP%\EditorLastPositions.reg" "HKEY_CURRENT_USER\Software\Far2\Editor\LastPositions"
regedit /e "%TMP%\SavedHistory.reg" "HKEY_CURRENT_USER\Software\Far2\SavedHistory"
regedit /e "%TMP%\SavedViewHistory.reg" "HKEY_CURRENT_USER\Software\Far2\SavedViewHistory"
regedit /e "%TMP%\SavedDialogHistory.reg" "HKEY_CURRENT_USER\Software\Far2\SavedDialogHistory"
regedit /e "%TMP%\SavedFolderHistory.reg" "HKEY_CURRENT_USER\Software\Far2\SavedFolderHistory"
regedit /e "%TMP%\PanelLeft.reg" "HKEY_CURRENT_USER\Software\Far2\Panel\Left"
regedit /e "%TMP%\PanelRight.reg" "HKEY_CURRENT_USER\Software\Far2\Panel\Right"
regedit /s "%~dp0\save-settings-privacy.reg"