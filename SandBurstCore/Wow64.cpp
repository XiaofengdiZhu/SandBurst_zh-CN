#include "SandBurstCore.h"

// NtUserCallTwoParamの番号
DWORD g_GetCursorPosNo;
DWORD g_GetPhysicalCursorPosNo;
DWORD g_SetCursorPosNo;
DWORD g_SetPhysicalCursorPosNo;

// user32 APIのシステム番号
DWORD g_MoveWindowNo;
DWORD g_SetWindowPosNo;
DWORD g_SetWindowPlacementNo;
DWORD g_ClipCursorNo;
DWORD g_NtUserCallTwoParamNo;
DWORD g_TrackPopupMenuExNo;

// Wow64に切り替えるジャンプコードの場所とそのコード
LPVOID g_OriginalWow64Entry = NULL;
BYTE g_Wow64EntryCode[7];

// Wow64のフックエントリとイグジット、7と10でWow64の呼び出し規約が異なるため、関数ポインタで使い分ける
FARPROC g_Wow64Entry = NULL;
FARPROC g_Wow64Exit = NULL;

BOOL WINAPI Wow64NtUserCallTwoParam(LPVOID arg1, LPVOID arg2, INT id, DWORD regEAX, DWORD regECX);
BOOL WINAPI Wow64SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int x, int y, int _cx, int _cy, UINT uFlags, DWORD regEAX, DWORD regECX);
BOOL WINAPI Wow64MoveWindow(HWND hWnd, int x, int y, int nWidth, int nHeight, BOOL bRepaint, DWORD regEAX, DWORD regECX);
BOOL WINAPI Wow64SetWindowPlacement(HWND hWnd, WINDOWPLACEMENT *lpwndpl, DWORD regEAX, DWORD regECX);
BOOL WINAPI Wow64ClipCursor(RECT *lpRect, DWORD regEAX, DWORD regECX);
BOOL WINAPI Wow64TrackPopupMenuEx(HMENU hmenu, UINT fuFlags, int x, int y, HWND hwnd, LPTPMPARAMS lptpm, DWORD regEAX, DWORD regECX);

__declspec(naked)
void Wow64Entry()
{
	_asm
	{
		// モジュール初期化時にここにWow64へのジャンプコードを書き込む
		NOP
		INT 3
		INT 3
		INT 3
		INT 3
		INT 3
		NOP
	}
}

__declspec(naked)
void Wow64EntryOnWin10()
{
	_asm
	{
		CALL Wow64Entry
		RET
	}
}

__declspec(naked)
void WINAPI Wow64Syscall()
{
#define LOCAL_VAR_SIZE 0x10
	DWORD regEAX, regECX, regEDX;
	DWORD* args;

	_asm
	{
		PUSH EBP
		MOV EBP, ESP
		SUB ESP, LOCAL_VAR_SIZE
		MOV regEAX, EAX
		MOV regECX, ECX
		MOV regEDX, EDX
		LEA EAX, [EBP + 0x0C]
		MOV args, EAX
	}


	if (regEAX == g_NtUserCallTwoParamNo)
	{
		// NtUserTwoParam
		Wow64NtUserCallTwoParam((LPVOID)args[0], (LPVOID)args[1], args[2], regEAX, regECX);
	}
	else if (regEAX == g_SetWindowPosNo)
	{
		// SetWindowPos
		Wow64SetWindowPos((HWND)args[0], (HWND)args[1], (INT)args[2], (INT)args[3], (INT)args[4], (INT)args[5], args[6], regEAX, regECX);
	}
	else if (regEAX == g_MoveWindowNo)
	{
		// MoveWindow
		Wow64MoveWindow((HWND)args[0], (INT)args[1], (INT)args[2], (INT)args[3], (INT)args[4], (BOOL)args[5], regEAX, regECX);
	}
	else if (regEAX == g_SetWindowPlacementNo)
	{
		// SetWindowPlacement
		Wow64SetWindowPlacement((HWND)args[0], (WINDOWPLACEMENT*)args[1], regEAX, regECX);
	}
	else if (regEAX == g_ClipCursorNo)
	{
		// ClipCursor
		Wow64ClipCursor((RECT*)args[0], regEAX, regECX);
	}
	else if (regEAX == g_TrackPopupMenuExNo)
	{
		Wow64TrackPopupMenuEx((HMENU)args[0], (UINT)args[1], (INT)args[2], (INT)args[3], (HWND)args[4], (LPTPMPARAMS)args[5], regEAX, regECX);
	}
	else
	{
		_asm
		{
			MOV EAX, regEAX
			MOV ECX, regECX
			MOV EDX, regEDX
			ADD ESP, LOCAL_VAR_SIZE
			POP EBP
			JMP Wow64Entry
			ADD ESP, 0x04
		}

	}

	_asm
	{
		ADD ESP, LOCAL_VAR_SIZE
		POP EBP
		JMP g_Wow64Exit
	}
#undef LOCAL_VAR_SIZE
}

__declspec(naked)
void WINAPI Wow64Exit()
{
	_asm
	{
		MOV ECX, [ESP]
		JMP ECX
	}
}

__declspec(naked)
void WINAPI Wow64ExitOnWin10()
{
	_asm
	{
		RET
	}
}

BOOL WINAPI Wow64NtUserCallTwoParam(LPVOID arg1, LPVOID arg2, INT id, DWORD regEAX, DWORD regECX)
{
	BOOL result;

	if ((id == g_GetCursorPosNo) || (id == g_GetPhysicalCursorPosNo))
	{
		// GetCursorPos
		// GetPhysicalCursorPos

		if (g_Info.ApiHooks.GetCursorPosHook)
		{


			_asm
			{
				MOV EAX, regEAX
				MOV ECX, regECX
				PUSH id
				PUSH arg2
				PUSH arg1
				LEA EDX, [ESP]
				CALL g_Wow64Entry
				ADD ESP, 0x10
				MOV result, EAX
			}

			if (arg1 && result)
			{
				POINT Pos;
				LPPOINT lpPoint = (LPPOINT)arg1;

				Pos.x = lpPoint->x;
				Pos.y = lpPoint->y;

				ScreenToClient(g_hWnd, &Pos);

				Pos.x = Pos.x * 100 / g_Info.Scale;
				Pos.y = Pos.y * 100 / g_Info.Scale;

				ClientToScreen(g_hWnd, &Pos);

				lpPoint->x = Pos.x;
				lpPoint->y = Pos.y;
			}

			return result;
		}
	}
	else if ((id == g_SetCursorPosNo) || (id == g_SetPhysicalCursorPosNo))
	{
		// SetCursorPos
		// SetPhysicalCursorPos
		if (g_Info.ApiHooks.SetCursorPosHook)
		{
			
			POINT Pos;
			LPINT x = (LPINT)&arg1;
			LPINT y = (LPINT)&arg2;
			Pos.x = *x;
			Pos.y = *y;

			ScreenToClient(g_hWnd, &Pos);

			Pos.x = Pos.x * g_Info.Scale / 100;
			Pos.y = Pos.y * g_Info.Scale / 100;

			ClientToScreen(g_hWnd, &Pos);

			*x = Pos.x;
			*y = Pos.y;
		}
	}

	_asm
	{
		MOV EAX, regEAX
		MOV ECX, regECX
		PUSH id
		PUSH arg2
		PUSH arg1
		LEA EDX, [ESP]
		CALL g_Wow64Entry
		ADD ESP, 0x10
	}
}

BOOL WINAPI Wow64SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int x, int y, int _cx, int _cy, UINT uFlags, DWORD regEAX, DWORD regECX)
{
	if (g_Info.ApiHooks.SetWindowPosHook)
	{
		if (uFlags & SWP_SANDBURST_CONTROL)
		{
			uFlags ^= SWP_SANDBURST_CONTROL;
		}
		else
		{
			std::map<HWND, PWINDOW_SIZE_INFO>::iterator it = g_ModifiedWindows.find(hWnd);
			if (it != g_ModifiedWindows.end())
			{
				uFlags |= SWP_NOSIZE;
			}
		}
	}

	_asm
	{
		MOV EAX, regEAX
		MOV ECX, regECX
		PUSH uFlags
		PUSH _cy
		PUSH _cx
		PUSH y
		PUSH x
		PUSH hWndInsertAfter
		PUSH hWnd
		LEA EDX, [ESP]
		CALL g_Wow64Entry
		ADD ESP, 0x20
	}
}

BOOL WINAPI Wow64MoveWindow(HWND hWnd, int x, int y, int nWidth, int nHeight, BOOL bRepaint, DWORD regEAX, DWORD regECX)
{
	if (g_Info.ApiHooks.MoveWindowHook)
	{
		return SetWindowPos(hWnd, 0, x, y, nWidth, nHeight, SWP_NOZORDER);
	}

	_asm
	{
		PUSH bRepaint
		PUSH nHeight
		PUSH nWidth
		PUSH y
		PUSH x
		PUSH hWnd
		LEA EDX, [ESP]
		CALL g_Wow64Entry
		ADD ESP, 0x1C
	}
}

BOOL WINAPI Wow64SetWindowPlacement(HWND hWnd, WINDOWPLACEMENT *lpwndpl, DWORD regEAX, DWORD regECX)
{
	if (g_Info.ApiHooks.SetWindowPlacementHook)
	{
		std::map<HWND, PWINDOW_SIZE_INFO>::iterator it = g_ModifiedWindows.find(hWnd);
		if (it != g_ModifiedWindows.end())
		{
			return TRUE;
		}
	}

	_asm
	{
		MOV EAX, regEAX
		MOV ECX, regECX
		PUSH lpwndpl
		PUSH hWnd
		LEA EDX, [ESP]
		CALL g_Wow64Entry
		ADD ESP, 0x0C
	}
}

BOOL WINAPI Wow64ClipCursor(RECT *lpRect, DWORD regEAX, DWORD regECX)
{
	BOOL Result;

	POINT Pos;

	if (g_Info.ApiHooks.ClipCursolHook)
	{
		if (lpRect)
		{
			Pos.x = lpRect->right;
			Pos.y = lpRect->bottom;

			ScreenToClient(g_hWnd, &Pos);

			Pos.x = Pos.x * g_Info.Scale / 100;
			Pos.y = Pos.y * g_Info.Scale / 100;

			ClientToScreen(g_hWnd, &Pos);

			lpRect->right = Pos.x;
			lpRect->bottom = Pos.y;
		}
	}
	
	__asm
	{
		MOV EAX, regEAX
		MOV ECX, regECX
		PUSH lpRect
		LEA EDX, [ESP]
		CALL g_Wow64Entry
		MOV Result, EAX
	}

	return Result;
}

BOOL WINAPI Wow64TrackPopupMenuEx(HMENU hmenu, UINT fuFlags, int x, int y, HWND hwnd, LPTPMPARAMS lptpm, DWORD regEAX, DWORD regECX)
{
	BOOL result;
	HDC hBuffer;
	HBITMAP hBitmap;
	HGDIOBJ hOld;
	BOOL visible;

//	CreateCapturedDC(&hBuffer, &hBitmap, &hOld);
//	UpdateThumbnailDC();
	
	visible = IsWindowVisible(g_Info.hShadow);
	if (visible)
	{
		ShowWindow(g_Info.hShadow, SW_HIDE);
	}

	_asm
	{
		MOV EAX, regEAX
		MOV ECX, regECX
		PUSH lptpm
		PUSH hwnd
		PUSH y
		PUSH x
		PUSH fuFlags
		PUSH hmenu
		LEA EDX, [ESP]
		CALL g_Wow64Entry
		ADD ESP, 0x1C
		MOV result, EAX
	}
		
//	RestoreCapturedDC(hBuffer, hBitmap, hOld);
	if (visible)
	{
		ShowWindow(g_Info.hShadow, SW_SHOW);
	}

	return result;
}

BYTE GetNtUserCallTwoParamNo(LPVOID apiAddress, DWORD offset)
{
	LPBYTE b = (LPBYTE)(apiAddress)+offset;

	return *b;
}

DWORD GetUser32ApiSystemNo(FARPROC user32Api)
{
	LPBYTE b = ((LPBYTE)user32Api) + 1;

	return *((LPDWORD)b);
}

DWORD GetNtUserCallTwoParam()
{
	// OSのバージョンを取得
	procRtlGetVersion RtlGetVersion = (procRtlGetVersion)GetProcAddress(GetModuleHandle("ntdll.dll"), "RtlGetVersion");
	OSVERSIONINFOEX osVersion;
	osVersion.dwOSVersionInfoSize = sizeof(osVersion);
	RtlGetVersion(&osVersion);

	if ((osVersion.dwMajorVersion == 6) && (osVersion.dwMinorVersion >= 1))
	{
		// Windows 7 - 8.1

		FARPROC pGetCursorPos = GetProcAddress(GetModuleHandle("user32.dll"), "GetCursorPos");

		LPBYTE callCodeAddress = (LPBYTE)(((DWORD)pGetCursorPos) + 12);

		if (*callCodeAddress == 0xE8)
		{
			LPVOID raa;
			memcpy(&raa, callCodeAddress + 1, 4);

			return ((DWORD)callCodeAddress + (DWORD)raa + 5);
		}
	}
	else if (osVersion.dwMajorVersion == 10)
	{
		// Windows 10

		HMODULE win32u = GetModuleHandle("win32u.dll");
		return (DWORD)GetProcAddress(win32u, "NtUserCallTwoParam");
	}

	return 0;
}

void Wow64Hook()
{
	if (g_OriginalWow64Entry != NULL)
	{
		return;
	}

	_asm
	{
		MOV EAX, FS:[0xC0]
		MOV g_OriginalWow64Entry, EAX
	}

	WriteProcessMemory(GetCurrentProcess(), Wow64Entry, g_OriginalWow64Entry, 7, NULL);
	
	BYTE buf[5];
	DWORD jmpAddress = (DWORD)Wow64Syscall - (DWORD)g_OriginalWow64Entry - 5;
	
	buf[0] = 0xE9;
	memcpy(&buf[1], &jmpAddress, 4);

	memcpy(g_Wow64EntryCode, g_OriginalWow64Entry, 7);

	WriteProcessMemory(GetCurrentProcess(), g_OriginalWow64Entry, buf, 5, NULL);
	
}

void Wow64Unhook()
{
	if (g_OriginalWow64Entry == NULL)
	{
		return;
	}

	WriteProcessMemory(GetCurrentProcess(), g_OriginalWow64Entry, g_Wow64EntryCode, 7, NULL);

	g_OriginalWow64Entry = NULL;
}

void Wow64Init()
{
	// NtUserCallTwoParamの番号を取得
	HMODULE user32 = GetModuleHandle("user32.dll");
	g_GetCursorPosNo = GetNtUserCallTwoParamNo(GetProcAddress(user32, "GetCursorPos"), 6);
	g_GetPhysicalCursorPosNo = GetNtUserCallTwoParamNo(GetProcAddress(user32, "GetPhysicalCursorPos"), 6);
	g_SetCursorPosNo = GetNtUserCallTwoParamNo(GetProcAddress(user32, "SetCursorPos"), 6);
	g_SetPhysicalCursorPosNo = GetNtUserCallTwoParamNo(GetProcAddress(user32, "SetPhysicalCursorPos"), 6);

	// user32 APIのシステム番号を取得
	g_SetWindowPosNo = GetUser32ApiSystemNo(NtGetProcAddress(user32, "SetWindowPos"));
	g_MoveWindowNo = GetUser32ApiSystemNo(NtGetProcAddress(user32, "MoveWindow"));
	g_SetWindowPlacementNo = GetUser32ApiSystemNo(NtGetProcAddress(user32, "SetWindowPlacement"));
	g_ClipCursorNo = GetUser32ApiSystemNo(NtGetProcAddress(user32, "ClipCursor"));
	g_NtUserCallTwoParamNo = GetUser32ApiSystemNo((FARPROC)GetNtUserCallTwoParam());
	g_TrackPopupMenuExNo = GetUser32ApiSystemNo(NtGetProcAddress(user32, "TrackPopupMenuEx"));

	// OSのバージョンを取得
	DWORD osver = GetOsVersion();

	switch (osver)
	{
	case 7:
		g_Wow64Entry = (FARPROC)Wow64Entry;
		g_Wow64Exit = (FARPROC)Wow64Exit;
		break;
	
	case 8:
	case 10:
		g_Wow64Entry = (FARPROC)Wow64EntryOnWin10;
		g_Wow64Exit = (FARPROC)Wow64ExitOnWin10;
	default:
		break;
	}
}

BOOL Wow64IsHooked()
{
	return g_OriginalWow64Entry != NULL;
}