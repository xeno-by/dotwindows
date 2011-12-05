@echo off

regedit /s "%TMP%\ViewerLastPositions.reg"
del "%TMP%\ViewerLastPositions.reg"

regedit /s "%TMP%\EditorLastPositions.reg"
del "%TMP%\EditorLastPositions.reg"

regedit /s "%TMP%\SavedHistory.reg"
del "%TMP%\SavedHistory.reg"

regedit /s "%TMP%\SavedViewHistory.reg"
del "%TMP%\SavedViewHistory.reg"

regedit /s "%TMP%\SavedDialogHistory.reg"
del "%TMP%\SavedDialogHistory.reg"

regedit /s "%TMP%\SavedFolderHistory.reg"
del "%TMP%\SavedFolderHistory.reg"

regedit /s "%TMP%\PanelLeft.reg"
del "%TMP%\PanelLeft.reg"

regedit /s "%TMP%\PanelRight.reg"
del "%TMP%\PanelRight.reg"

regedit /s "%TMP%\RegEditor.reg"
del "%TMP%\RegEditor.reg"
