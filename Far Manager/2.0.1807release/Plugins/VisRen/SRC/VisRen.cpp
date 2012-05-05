/****************************************************************************
 * VisRen.cpp
 *
 * Plugin module for FAR Manager 2.0
 *
 * Copyright (c) 2007-2010 Alexey Samlyukov
 ****************************************************************************/
/*
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:
1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the authors may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

/* $ Revision: 14.0 $ */

#include "VisRen.hpp"
#include "VisRenDlg.hpp"

/****************************************************************************
 * Копии стандартных структур FAR
 ****************************************************************************/
struct PluginStartupInfo Info;
struct FarStandardFunctions FSF;

/****************************************************************************
 * Набор переменных
 ****************************************************************************/
struct Options Opt;         //Текущие настройки плагина
struct DlgSize DlgSize;     //Размер диалога
struct UndoFileName Undo;   //Undo переименования

void * __cdecl malloc(size_t size)
{
	return HeapAlloc(GetProcessHeap(),HEAP_ZERO_MEMORY,size);
}

void __cdecl free(void *block)
{
	if (block) HeapFree(GetProcessHeap(),0,block);
}

void * __cdecl realloc(void *block, size_t size)
{
	if (!size)
	{
		if (block) HeapFree(GetProcessHeap(),0,block);
		return NULL;
	}
	if (block) return HeapReAlloc(GetProcessHeap(),HEAP_ZERO_MEMORY,block,size);
	else return HeapAlloc(GetProcessHeap(),HEAP_ZERO_MEMORY,size);
}
#ifdef __cplusplus
void * __cdecl operator new(size_t size)
{
	return malloc(size);
}
 void __cdecl operator delete(void *block)
{
	free(block);
}
#endif


/****************************************************************************
 * Очистка элементов Undo переименования
 ****************************************************************************/
void FreeUndo()
{
	for (int i=Undo.iCount-1; i>=0; i--)
	{
		free(Undo.CurFileName[i]);
		free(Undo.OldFileName[i]);
	}
	free(Undo.CurFileName); Undo.CurFileName=0;
	free(Undo.OldFileName); Undo.OldFileName=0;
	free(Undo.Dir); Undo.Dir=0;
	Undo.iCount=0;
}

/****************************************************************************
 * Обёртка сервисной функции FAR: получение строки из .lng-файла
 ****************************************************************************/
const wchar_t *GetMsg(int MsgId)
{
	return Info.GetMsg(Info.ModuleNumber, MsgId);
}

/****************************************************************************
 * Показ предупреждения-ошибки с заголовком и одной строчкой
 ****************************************************************************/
void ErrorMsg(DWORD Title, DWORD Body)
{
	const wchar_t *MsgItems[]={ GetMsg(Title), GetMsg(Body), GetMsg(MOK) };
	Info.Message(Info.ModuleNumber, FMSG_WARNING, 0, MsgItems, 3, 1);
}

/****************************************************************************
 * Показ предупреждения "Yes-No" с заголовком и одной строчкой
 ****************************************************************************/
bool YesNoMsg(DWORD Title, DWORD Body)
{
	const wchar_t *MsgItems[]={ GetMsg(Title), GetMsg(Body) };
	return (!Info.Message(Info.ModuleNumber, FMSG_WARNING|FMSG_MB_YESNO, 0, MsgItems, 2, 0));
}

// Сообщение для отладки
int DebugMsg(wchar_t *msg, wchar_t *msg2, unsigned int i)
{
  wchar_t *MsgItems[] = {L"DebugMsg", L"", L"", L""};
  wchar_t buf[80]; FSF.itoa(i, buf,10);
  MsgItems[1] = msg2;
  MsgItems[2] = msg;
  MsgItems[3] = buf;
  return (!Info.Message(Info.ModuleNumber, FMSG_WARNING|FMSG_MB_OKCANCEL, 0,
                        MsgItems, sizeof(MsgItems) / sizeof(MsgItems[0]),2));
}


/****************************************************************************
 ***************************** Exported functions ***************************
 ****************************************************************************/

/****************************************************************************
 * Эти функции плагина FAR вызывает в первую очередь
 ****************************************************************************/
// установим минимально поддерживаемую версию FARа...
int WINAPI _export GetMinFarVersionW() { return MAKEFARVERSION(2,0,1572); }

// заполним структуру PluginStartupInfo и сделаем ряд полезных действий...
void WINAPI _export SetStartupInfoW(const struct PluginStartupInfo *Info)
{
	::Info = *Info;
	if (Info->StructSize >= (int)sizeof(struct PluginStartupInfo))
	{
		FSF = *Info->FSF;
		::Info.FSF = &FSF;
		// обнулим структуру под элементы отката
		memset(&Undo, 0, sizeof(Undo));
	}
}

/****************************************************************************
 * Эту функцию плагина FAR вызывает во вторую очередь - заполним PluginInfo, т.е.
 * скажем FARу какие пункты добавить в "Plugin commands" и "Plugins configuration".
 ****************************************************************************/
void WINAPI _export GetPluginInfoW(struct PluginInfo *Info)
{
	static const wchar_t *PluginMenuStrings[1];
	PluginMenuStrings[0]            = GetMsg(MVRenTitle);

	Info->StructSize                = (int)sizeof(*Info);
	Info->PluginMenuStrings         = PluginMenuStrings;
	Info->PluginMenuStringsNumber   = sizeof(PluginMenuStrings) / sizeof(PluginMenuStrings[0]);
}

/****************************************************************************
 * Основная функция плагина. FAR её вызывает, когда пользователь зовёт плагин
 ****************************************************************************/
HANDLE WINAPI _export OpenPluginW(int OpenFrom, INT_PTR Item)
{
	HANDLE hPlugin = INVALID_HANDLE_VALUE;
	struct PanelInfo PInfo;

	// Если не удалось запросить информацию о панели...
	if (!Info.Control(PANEL_ACTIVE,FCTL_GETPANELINFO,0,(LONG_PTR)&PInfo))
		return hPlugin;

	if (PInfo.PanelType!=PTYPE_FILEPANEL || PInfo.Plugin || !PInfo.ItemsNumber || !PInfo.SelectedItemsNumber)
	{
		ErrorMsg(MVRenTitle, MFilePanelsRequired);
		return hPlugin;
	}

	class VisRenDlg RenDlg;
	RenDlg.InitFileList(PInfo.SelectedItemsNumber);

	switch (RenDlg.ShowDialog())
	{
		case 0:
			RenDlg.RenameFile(PInfo.SelectedItemsNumber, PInfo.ItemsNumber); break;
		case 2:
			RenDlg.RenameInEditor(PInfo.SelectedItemsNumber, PInfo.ItemsNumber); break;
	}

	return hPlugin;
}

/****************************************************************************
 * Эту функцию FAR вызывает перед выгрузкой плагина
 ****************************************************************************/
void WINAPI _export ExitFARW()
{
	//Освободим память в случае выгрузки плагина
	FreeUndo();
}
