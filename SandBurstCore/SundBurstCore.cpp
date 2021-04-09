// SandBurstHook
// version 1.7.0
// Created by Master.D
#include <string.h>
#include "SandBurstCore.h"

#pragma comment(lib, "d3d9.lib")



// API のオリジナルアドレス
FARPROC g_pGetCursorPos = NULL;
FARPROC g_pSetCursorPos = NULL;
FARPROC g_pMoveWindow = NULL;
FARPROC g_pSetWindowPos = NULL;
FARPROC g_pSetWindowPlacement = NULL;
FARPROC g_pClipCursor = NULL;
FARPROC g_pGetDC = NULL;
FARPROC g_pReleaseDC = NULL;
FARPROC g_pBitBlt = NULL;
FARPROC g_pGetWindowRect = NULL;
FARPROC g_pGetClientRect = NULL;
FARPROC g_pD3D9Present = NULL;
FARPROC g_pUser32Native = NULL;
FARPROC g_pTrackPopupMenuEx = NULL;
FARPROC g_pD3D11SetViewport = NULL;

// API のオリジナルコード
BYTE g_GetCursorPosCode[5];
BYTE g_SetCursorPosCode[5];
BYTE g_MoveWindowCode[5];
BYTE g_SetWindowPosCode[5];
BYTE g_SetWindowPlacementCode[5];
BYTE g_ClipCursorCode[5];
BYTE g_GetDCCode[5];
BYTE g_ReleaseDCCode[5];
BYTE g_BitBltCode[5];
BYTE g_GetWindowRectCode[5];
BYTE g_GetClientRectCode[5];
DWORD g_GetWindowRectArg = 0;
DWORD g_GetClientRectArg = 0;		// GetWindowRect,ClientRectはOSごとに先頭のコードが違う
BYTE g_D3D9PresentCode[5];
BYTE g_User32NativeCode[5];
BYTE g_TrackPopupMenuExCode[5];
BYTE g_D3D11SetViewportCode[5];

// 本来のウィンドウプロシージャ
WNDPROC g_OriginalProc = NULL;

// 子ウィンドウのプロシージャ
std::map<HWND, WNDPROC> g_Procs;

// デバイスコンテキストハンドル
std::vector<HDC> g_hDC;

// フックの情報
HOOK_INFO g_Info;

// ウィンドウハンドル
HWND g_hWnd = 0;

// Direct3D用のクリティカルセクション
CRITICAL_SECTION g_CriticalSection;

// サイズが修正されたウィンドウ
std::map<HWND, PWINDOW_SIZE_INFO> g_ModifiedWindows;

// プロセス終了イベント
HANDLE g_hEventExit;

// GetXXXRect
DWORD g_RectNo = 0;

DWORD GetOsVersion()
{
	// OSのバージョンを取得
	procRtlGetVersion RtlGetVersion = (procRtlGetVersion)GetProcAddress(GetModuleHandle("ntdll.dll"), "RtlGetVersion");
	OSVERSIONINFOEX osVersion;
	osVersion.dwOSVersionInfoSize = sizeof(osVersion);
	RtlGetVersion(&osVersion);

	if ((osVersion.dwMajorVersion == 6))
	{
		if (osVersion.dwMinorVersion <= 1)
		{
			return 7;
		}
		else
		{
			return 8;
		}
	}
	else if (osVersion.dwMajorVersion == 10)
	{
		return 10;
	}

	return 10;
}

BOOL Hook(FARPROC APIAddress, LPVOID HookProc)
{
	DWORD Write, Buf1, Buf2;
	Buf1 = 0xE9;
	Buf2 = (DWORD)HookProc - (DWORD)APIAddress - 5;

	if (WriteProcessMemory(GetCurrentProcess(), APIAddress, &Buf1, 1, &Write) == 0)
	{
		return FALSE;
	}

	if (WriteProcessMemory(GetCurrentProcess(), (LPVOID)((DWORD)(APIAddress)+1), &Buf2, 4, &Write) == 0)
	{
		return FALSE;
	}

	return TRUE;
}

void ModifyWindowSize(HWND hWnd, int newWidth, int newHeight)
{
	RECT rc;
	GetWindowRect(hWnd, &rc);

	int width = rc.right - rc.left;
	int height = rc.bottom - rc.top;

	UINT flag = SWP_NOMOVE | SWP_NOZORDER | SWP_ASYNCWINDOWPOS;
	PWINDOW_SIZE_INFO sizeInfo = new WINDOW_SIZE_INFO();
	sizeInfo->hWnd = hWnd;
	sizeInfo->ModifiedSize.x = newWidth;
	sizeInfo->ModifiedSize.y = newHeight;
	sizeInfo->OriginalSize.x = width;
	sizeInfo->OriginalSize.y = height;
	g_ModifiedWindows[hWnd] = sizeInfo;

	
	
	if ((g_pSetWindowPos || Wow64IsHooked()) && g_Info.ApiHooks.SetWindowPosHook)
	{
		flag |= SWP_SANDBURST_CONTROL;
	}

	SetWindowPos(hWnd, 0, 0, 0, newWidth, newHeight, flag);
}

void RestoreWindowSize(HWND hWnd)
{
	std::map<HWND, PWINDOW_SIZE_INFO>::iterator it = g_ModifiedWindows.find(hWnd);

	if (it != g_ModifiedWindows.end())
	{
		int newWidth = it->second->OriginalSize.x;
		int newHeight = it->second->OriginalSize.y;
		UINT flag = SWP_NOMOVE | SWP_NOZORDER | SWP_ASYNCWINDOWPOS;

		if ((g_pSetWindowPos || Wow64IsHooked()) && g_Info.ApiHooks.SetWindowPosHook)
		{
			flag |= SWP_SANDBURST_CONTROL;
		}
		
		SetWindowPos(hWnd, 0, 0, 0, newWidth, newHeight, flag);
	}
}

// フックプロシージャ
LRESULT CALLBACK HookProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	POINT pos;
	MINMAXINFO *pmmi;
	LRESULT result;
	BOOL visible;

	switch (msg)
	{
	case WM_SYSCOMMAND:
		switch (wParam & 0xFFF0)
		{
		case SC_MOUSEMENU:
		case SC_KEYMENU:
			visible = IsWindowVisible(g_Info.hShadow);
			if (visible)
			{
				ShowWindow(g_Info.hShadow, SW_HIDE);
			}

			result = CallWindowProc(g_OriginalProc, hWnd, msg, wParam, lParam);
			
			if (visible)
			{
				ShowWindow(g_Info.hShadow, SW_SHOW);
			}
			return result;
		}
		break;
	case WM_WINDOWPOSCHANGING:
		if (g_Info.ApiHooks.WM_WINDOWPOS_Hook)
		{
			return DefWindowProc(hWnd, msg, wParam, lParam);
		}
		break;

	case WM_GETMINMAXINFO:
		pmmi = (MINMAXINFO *)lParam;
		pmmi->ptMaxTrackSize.x = (g_Info.WindowSize.x - g_Info.ClientSize.x) + g_Info.ClientSize.x * g_Info.Scale / 100;
		pmmi->ptMaxTrackSize.y = (g_Info.WindowSize.y - g_Info.ClientSize.y) + g_Info.ClientSize.y * g_Info.Scale / 100;
		
		return 0;

	case WM_ACTIVATE:
		SendMessage(g_Info.hSandBurst, WM_USER, wParam, lParam);
		break;

	case WM_SIZE:
		if (wParam == SIZE_MINIMIZED)
		{
			SendMessage(g_Info.hSandBurst, WM_USER, 0x10000000 | WA_INACTIVE, 0);
		}

		if (g_Info.ApiHooks.WM_SIZE_Hook)
		{
			if (wParam == SIZE_RESTORED)
			{
				lParam = (LPARAM)(g_Info.ClientSize.x | (g_Info.ClientSize.y << 16));
			}
		}
		break;

	case WM_MOUSEMOVE:
	case WM_LBUTTONDBLCLK:
	case WM_LBUTTONDOWN:
	case WM_LBUTTONUP:
	case WM_MBUTTONDBLCLK:
	case WM_MBUTTONDOWN:
	case WM_MBUTTONUP:
	case WM_RBUTTONDBLCLK:
	case WM_RBUTTONDOWN:
	case WM_RBUTTONUP:
		pos.x = lParam & 0xFFFF;
		pos.y = (lParam & 0xFFFF0000) >> 16;

		pos.x = pos.x * 100 / g_Info.Scale;
		pos.y = pos.y * 100 / g_Info.Scale;

		lParam = pos.x | (pos.y << 16);
	}

	result = CallWindowProc(g_OriginalProc, hWnd, msg, wParam, lParam);

	return result;
}

// 子ウィンドウのフックプロシージャ
LRESULT CALLBACK ChildHookProc(HWND hWnd, UINT Msg, WPARAM WParam, LPARAM LParam)
{
	POINT Pos;

	switch (Msg)
	{
	case WM_MOUSEMOVE:
	case WM_LBUTTONDBLCLK:
	case WM_LBUTTONDOWN:
	case WM_LBUTTONUP:
	case WM_MBUTTONDBLCLK:
	case WM_MBUTTONDOWN:
	case WM_MBUTTONUP:
	case WM_RBUTTONDBLCLK:
	case WM_RBUTTONDOWN:
	case WM_RBUTTONUP:
		Pos.x = LParam & 0xFFFF;
		Pos.y = (LParam & 0xFFFF0000) >> 16;

		Pos.x = Pos.x * 100 / g_Info.Scale;
		Pos.y = Pos.y * 100 / g_Info.Scale;

		LParam = Pos.x | (Pos.y << 16);
	}

	LRESULT Result = CallWindowProc(g_Procs[hWnd], hWnd, Msg, WParam, LParam);

	return Result;
}

BOOL CALLBACK EnumChildProc(HWND hWnd, LPARAM Param)
{
	WNDPROC p = (WNDPROC)GetWindowLong(hWnd, GWL_WNDPROC);
	SetWindowLong(hWnd, GWL_WNDPROC, (LONG)ChildHookProc);

	g_Procs[hWnd] = p;

	return TRUE;
}

BOOL CALLBACK EnumChildProcModifyWindow(HWND hWnd, LPARAM Param)
{
	RECT rc;
	GetWindowRect(hWnd, &rc);
	
	float width = rc.right - rc.left;
	float height = rc.bottom - rc.top;
	

	if (((g_Info.ClientSize.x - width) / g_Info.ClientSize.x < 0.1f) && ((g_Info.ClientSize.y - height) / g_Info.ClientSize.y < 0.1f))
	{

		ModifyWindowSize(hWnd, width * g_Info.Scale / 100, height * g_Info.Scale / 100);
	}

	return TRUE;
}



void HookDirect3D()
{
	HMODULE hMod = GetModuleHandle("d3d9.dll");
	if (hMod)
	{
		g_pD3D9Present = (FARPROC)(g_Info.D3D9PresentRVA + (DWORD)hMod);
		memcpy(g_D3D9PresentCode, g_pD3D9Present, sizeof(g_D3D9PresentCode));

		Hook(g_pD3D9Present, HkD3D9Present);
	}

	hMod = GetModuleHandle("d3d11.dll");
	if (hMod && g_Info.D3D11SetViewportRVA)
	{
		g_pD3D11SetViewport = (FARPROC)(g_Info.D3D11SetViewportRVA + (DWORD)hMod);
		memcpy(g_D3D11SetViewportCode, g_pD3D11SetViewport, sizeof(g_D3D11SetViewportCode));

		Hook(g_pD3D11SetViewport, hkD3D11RSSetViewports);
	}
}

void UnhookDirect3D()
{
	EnterCriticalSection(&g_CriticalSection);

	DWORD Written;
	WriteProcessMemory(GetCurrentProcess(), g_pD3D9Present, g_D3D9PresentCode, sizeof(g_D3D9PresentCode), &Written);

	LeaveCriticalSection(&g_CriticalSection);
}

// WindowsAPIのアドレスを取得する関数
// Win10だと一部のAPIの名前が"NtUserXXX"に変更されているため
// 最初にNtUserXXXで取得し、無ければ元のAPI名で取得する。
FARPROC NtGetProcAddress(HMODULE hModule, LPCSTR lpProcName)
{
	if (hModule == GetModuleHandle("user32.dll"))
	{
		CHAR ntName[0xFF];
		sprintf_s(ntName, sizeof(ntName), "NtUser%s", lpProcName);

		HMODULE ntModule = GetModuleHandle("win32u.dll");

		FARPROC address = GetProcAddress(ntModule, ntName);

		if (address)
		{
			return address;
		}
	}

	return GetProcAddress(hModule, lpProcName);
}


HDC GetDeskTopDC()
{

	HDC hDC;

	_asm
	{
		PUSH 0
		PUSH RetLabel
		MOV ECX, OFFSET g_GetDCCode[1]
		MOV EAX, DWORD PTR[ECX]			// NtKernelに渡すシステムサービスの番号
		MOV ECX, g_pGetDC
		ADD ECX, 5
		JMP ECX
		RetLabel :
		MOV hDC, EAX
	}

	return hDC;
}


VOID UpdateThumbnailDC()
{
	POINT pos;
	HDC hDesktop = GetDeskTopDC();
	HDC hShadow = GetDC(g_Info.hShadow);
	pos.x = pos.y = 0;
	ClientToScreen(g_Info.hWnd, &pos);
	int cx = g_Info.ClientSize.x * g_Info.Scale / 100;
	int cy = g_Info.ClientSize.y * g_Info.Scale / 100;

	BitBlt(hShadow, 0, 0, cx, cy, hDesktop, pos.x, pos.y, SRCCOPY);

	HDC hMain = GetDC(g_Info.hWnd);
	BitBlt(hMain, 0, 0, cx, cy, hShadow, 0, 0, SRCCOPY);

	ReleaseDC(0, hDesktop);
	ReleaseDC(g_Info.hShadow, hShadow);
	ReleaseDC(g_Info.hWnd, hMain);

}

VOID CreateCapturedDC(HDC OUT *hBuffer, HBITMAP OUT *hBitmap, HGDIOBJ OUT *hOldObj)
{
	HDC hdc = GetDC(g_Info.hWnd);
	*hBitmap = CreateCompatibleBitmap(hdc, g_Info.ClientSize.x, g_Info.ClientSize.y);
	*hBuffer = CreateCompatibleDC(hdc);

	*hOldObj = SelectObject(*hBuffer, *hBitmap);
	int cx = g_Info.ClientSize.x * g_Info.Scale / 100;
	int cy = g_Info.ClientSize.y * g_Info.Scale / 100;

	BitBlt(*hBuffer, 0, 0, cx, cy, hdc, 0, 0, SRCCOPY);
	ReleaseDC(g_Info.hWnd, hdc);
}

VOID RestoreCapturedDC(HDC hBuffer, HBITMAP hBitmap, HGDIOBJ hOldObj)
{
	HDC hdc = GetDC(g_Info.hWnd);
	int cx = g_Info.ClientSize.x;
	int cy = g_Info.ClientSize.y;

	BitBlt(hdc, 0, 0, cx, cy, hBuffer, 0, 0, SRCCOPY);

	ReleaseDC(g_Info.hWnd, hdc);
	SelectObject(hBuffer, hOldObj);
	DeleteObject(hBitmap);
	DeleteDC(hBuffer);
}

// タスクバー領域をRECTで取得
VOID GetTaskbarRect(RECT* rc)
{
	APPBARDATA abd;

	memset(&abd, 0, sizeof(abd));
	abd.cbSize = sizeof(abd);

	SHAppBarMessage(ABM_GETTASKBARPOS, &abd);
	*rc = abd.rc;
}

// デスクトップ領域のタスクバー分のオフセットを取得
VOID GetTaskbarOffset(POINT* offset)
{
	int dispx = GetSystemMetrics(SM_CXSCREEN);
	int dispy = GetSystemMetrics(SM_CYSCREEN);


	RECT taskbar;
	GetTaskbarRect(&taskbar);

	offset->x = offset->y = 0;


	if ((taskbar.left == 0) && (taskbar.top == 0))
	{
		if (taskbar.bottom == dispy)
		{
			// 左タスクバー
			offset->x += (taskbar.right - taskbar.left);
		}
		else
		{
			// 上タスクバー
			offset->y += (taskbar.bottom - taskbar.top);
		}

	}
	else
	{
		if (taskbar.left == 0)
		{
			// 下タスクバー
			offset->y -= (taskbar.bottom - taskbar.top);
		}
		else
		{
			// 右タスクバー
			offset->x -= (taskbar.right - taskbar.left);
		}
	}
}

void Install(void)
{
	InitializeCriticalSection(&g_CriticalSection);

	// 共有メモリ開く
	HANDLE hMap = OpenFileMapping(FILE_MAP_READ, FALSE, "SandBurstShareMemory");
	HOOK_INFO *Info = (HOOK_INFO*)MapViewOfFile(hMap, FILE_MAP_READ, 0, 0, 0x1000);
	if (Info)
	{
		// フック情報を読み込む
		memcpy(&g_Info, Info, sizeof(g_Info));
		UnmapViewOfFile(Info);
		g_hWnd = g_Info.hWnd;

		HMODULE user32 = GetModuleHandle("user32.dll");

		// ウィンドウプロシージャフック
		if (g_Info.ApiHooks.MessageHook)
		{
			g_OriginalProc = (WNDPROC)GetWindowLong(g_hWnd, GWL_WNDPROC);
			SetWindowLong(g_hWnd, GWL_WNDPROC, (LONG)HookProc);
			EnumChildWindows(g_hWnd, EnumChildProc, 0);
		}

		DWORD osver = GetOsVersion();
		if (osver == 7)
		{
			g_RectNo = 0x08;
		}
		else if (osver == 8)
		{
			g_RectNo = 0x20;
		}

		switch (g_Info.HookType)
		{
		case HookTypeApi:
			// GetCursorPos フック
			if (g_Info.ApiHooks.GetCursorPosHook)
			{
				g_pGetCursorPos = NtGetProcAddress(user32, "GetCursorPos");
				memcpy(g_GetCursorPosCode, g_pGetCursorPos, sizeof(g_GetCursorPosCode));
				Hook(g_pGetCursorPos, HkGetCursorPos);
			}

			// SetCursorPos フック
			if (g_Info.ApiHooks.SetCursorPosHook)
			{
				g_pSetCursorPos = NtGetProcAddress(user32, "SetCursorPos");
				memcpy(g_SetCursorPosCode, g_pSetCursorPos, sizeof(g_SetCursorPosCode));
				Hook(g_pSetCursorPos, HkSetCursorPos);
			}

			// ClipCursor フック
			if (g_Info.ApiHooks.ClipCursolHook)
			{
				g_pClipCursor = NtGetProcAddress(user32, "ClipCursor");
				memcpy(g_ClipCursorCode, g_pClipCursor, sizeof(g_ClipCursorCode));
				Hook(g_pClipCursor, HkClipCursor);
			}

			// MoveWindow フック
			if (g_Info.ApiHooks.MoveWindowHook)
			{
				g_pMoveWindow = NtGetProcAddress(user32, "MoveWindow");
				memcpy(g_MoveWindowCode, g_pMoveWindow, sizeof(g_MoveWindowCode));
				Hook(g_pMoveWindow, HkMoveWindow);
			}

			// SetWindowPos フック
			if (g_Info.ApiHooks.SetWindowPosHook)
			{
				g_pSetWindowPos = NtGetProcAddress(user32, "SetWindowPos");
				memcpy(g_SetWindowPosCode, g_pSetWindowPos, sizeof(g_SetWindowPosCode));
				Hook(g_pSetWindowPos, HkSetWindowPos);
			}

			// SetWindowPlacement フック
			if (g_Info.ApiHooks.SetWindowPlacementHook)
			{
				g_pSetWindowPlacement = NtGetProcAddress(user32, "SetWindowPlacement");
				memcpy(g_SetWindowPlacementCode, g_pSetWindowPlacement, sizeof(g_SetWindowPlacementCode));
				Hook(g_pSetWindowPlacement, HkSetWindowPlacement);
			}

			// TrackPopupMenuExをフック
			{
				g_pTrackPopupMenuEx = NtGetProcAddress(user32, "TrackPopupMenuEx");
				memcpy(g_TrackPopupMenuExCode, g_pTrackPopupMenuEx, sizeof(g_TrackPopupMenuExCode));
				Hook(g_pTrackPopupMenuEx, HkTrackPopupMenuEx);
			}

			break;
		case HookTypeWow64:
			Wow64Init();
			Wow64Hook();
			break;
		}



		// スクリーンショットの補正
		if (g_Info.ApiHooks.ScreenShotHook)
		{

			// GetDC フック
			g_pGetDC = NtGetProcAddress(user32, "GetDC");
			memcpy(g_GetDCCode, g_pGetDC, sizeof(g_GetDCCode));
			Hook(g_pGetDC, HkGetDC);

			// ReleaseDC フック
			g_pReleaseDC = GetProcAddress(user32, "ReleaseDC");
			memcpy(g_ReleaseDCCode, g_pReleaseDC, sizeof(g_ReleaseDCCode));

			if (g_pReleaseDC != GetProcAddress(user32, "NtUserReleaseDC"))
			{
				Hook(g_pReleaseDC, HkReleaseDC);
			}
			else
			{
				Hook(g_pReleaseDC, HkNtUserReleaseDC);
			}

			// BitBlt フック
			g_pBitBlt = NtGetProcAddress(GetModuleHandle("gdi32.dll"), "BitBlt");
			memcpy(g_BitBltCode, g_pBitBlt, sizeof(g_BitBltCode));
			Hook(g_pBitBlt, HkBitBlt);
		}

		// GetWindowRectの補正
		if (g_Info.ApiHooks.GetWindowRectHook)
		{
			g_pGetWindowRect = NtGetProcAddress(user32, "GetWindowRect");
			memcpy(g_GetWindowRectCode, g_pGetWindowRect, sizeof(g_GetWindowRectCode));

			// このAPIだけOSごとにコードが異なる
			if (g_GetWindowRectCode[0] != 0x8B)
			{
				memcpy(&g_GetWindowRectArg, (void*)((DWORD)(g_pGetWindowRect)+3), 4);
			}

			Hook(g_pGetWindowRect, HkGetWindowRect);
		}

		// GetClientRectの補正
		if (g_Info.ApiHooks.GetClientRectHook)
		{
			g_pGetClientRect = NtGetProcAddress(user32, "GetClientRect");
			memcpy(g_GetClientRectCode, g_pGetClientRect, sizeof(g_GetClientRectCode));

			// このAPIだけOSごとにコードが異なる
			if (g_GetClientRectCode[0] != 0x8B)
			{
				memcpy(&g_GetClientRectArg, (void*)((DWORD)(g_pGetClientRect)+3), 4);
			}

			Hook(g_pGetClientRect, HkGetClientRect);
		}

		// IDirect3DDevice->Presentの補正
		if (g_Info.ApiHooks.D3DHook)
		{
			HookDirect3D();
		}

		// ウィンドウの大きさを変更
		if (g_Info.ModifiesWindowSize)
		{
			int newWidth = g_Info.ClientSize.x * g_Info.Scale / 100 + (g_Info.WindowSize.x - g_Info.ClientSize.x);
			int newHeight = g_Info.ClientSize.y * g_Info.Scale / 100 + (g_Info.WindowSize.y - g_Info.ClientSize.y);

			ModifyWindowSize(g_hWnd, newWidth, newHeight);

			// メインウィンドウをディスプレイの中心に移動
			if (g_Info.CentralizesWindow)
			{
				int dispx = g_Info.Display.right;
				int dispy = g_Info.Display.bottom;

				int x = dispx / 2 - newWidth / 2 + g_Info.Display.left;
				int y = dispy / 2 - newHeight / 2;

				// タスクバー分位置をずらす
				if (g_Info.ExcludesTaskbar)
				{
					POINT offset;
					GetTaskbarOffset(&offset);
					x += offset.x / 2;
					y += offset.y / 2;
				}
				

				SetWindowPos(g_hWnd, 0, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_ASYNCWINDOWPOS);
			}
			
		}

		// 子ウィンドウの大きさを変更
		if (g_Info.ModifiesChildWindowSize)
		{
			EnumChildWindows(g_hWnd, EnumChildProcModifyWindow, (LPARAM)g_hWnd);
		}

	}

	if (hMap)
	{
		CloseHandle(hMap);
	}

}

void Uninstall(void)
{
	LPARAM LParam = NULL;
	WPARAM WParam = NULL;

	// D3Dの解放
	if (g_pD3D9Present)
		UnhookDirect3D();

	// WindowProcedureの復元
	if (g_OriginalProc)
	{
		std::map<HWND, WNDPROC>::iterator it;
		SetWindowLong(g_hWnd, GWL_WNDPROC, (LONG)g_OriginalProc);
		it = g_Procs.begin();
		while (it != g_Procs.end())
		{
			SetWindowLong((*it).first, GWL_WNDPROC, (LONG)(*it).second);
			it++;
		}
	}

	UNHOOK(g_pGetCursorPos, g_GetCursorPosCode);
	UNHOOK(g_pSetCursorPos, g_SetCursorPosCode);
	UNHOOK(g_pMoveWindow, g_MoveWindowCode);
	UNHOOK(g_pSetWindowPos, g_SetWindowPosCode);
	UNHOOK(g_pMoveWindow, g_MoveWindowCode);
	UNHOOK(g_pSetWindowPos, g_SetWindowPosCode);
	UNHOOK(g_pSetWindowPlacement, g_SetWindowPlacementCode);
	UNHOOK(g_pClipCursor, g_ClipCursorCode);
	UNHOOK(g_pGetDC, g_GetDCCode);
	UNHOOK(g_pReleaseDC, g_ReleaseDCCode);
	UNHOOK(g_pBitBlt, g_BitBltCode);
	UNHOOK(g_pGetWindowRect, g_GetWindowRectCode);
	UNHOOK(g_pGetClientRect, g_GetClientRectCode);
	UNHOOK(g_pUser32Native, g_User32NativeCode);
	UNHOOK(g_pTrackPopupMenuEx, g_TrackPopupMenuExCode);

	Wow64Unhook();

	// ウィンドウサイズを元に戻す
	std::map<HWND, PWINDOW_SIZE_INFO>::iterator it = g_ModifiedWindows.begin();
	while (it != g_ModifiedWindows.end())
	{
		RestoreWindowSize(it->second->hWnd);
		delete it->second;
		it++;
	}

	g_ModifiedWindows.clear();

	WParam = (WPARAM)g_Info.hWnd;

	// SandBurst本体に終了通知
	PostMessage(g_Info.hSandBurst, WM_USER + 1, WParam, LParam);

	// クリティカルセクション削除
	DeleteCriticalSection(&g_CriticalSection);
}

// パイプの待機関数
// http://eternalwindows.jp/ipc/namedpipe/namedpipe02.html
BOOL WaitEvent(HANDLE hEvent)
{
	HANDLE hEventArray[2];
	DWORD  dwEventNo;

	hEventArray[0] = g_hEventExit;
	hEventArray[1] = hEvent;
	dwEventNo = WaitForMultipleObjects(2, hEventArray, FALSE, INFINITE) - WAIT_OBJECT_0;
	if (dwEventNo == 0)
		return FALSE;
	else if (dwEventNo == 1)
		return TRUE;
	else
		return FALSE;
}

// パイプ名の取得
// "\\.\pipe\SBPipe_プロセス名.exe"
VOID GetPipeName(char* name, int maxSize)
{
	char exeName[256];
	char pipeName[256] = "\\\\.\\pipe\\";
	char *p;

	GetModuleFileName(NULL, exeName, sizeof(exeName));
	p = strrchr(exeName, '\\') + 1;
	strncat_s(pipeName, "SBPipe_", sizeof(pipeName));
	strncat_s(pipeName, p, sizeof(pipeName));

	strncpy_s(name, maxSize, pipeName, maxSize);
}

// 本体とのパイプ通信用スレッド
DWORD WINAPI pipeThread(LPVOID param)
{
	char pipeName[256];
	OVERLAPPED ov;

	GetPipeName(pipeName, sizeof(pipeName));
	
	HANDLE hPipe = CreateNamedPipe(pipeName, PIPE_ACCESS_DUPLEX | FILE_FLAG_OVERLAPPED, PIPE_TYPE_BYTE | PIPE_WAIT, 1, 0, 0, NMPWAIT_WAIT_FOREVER, NULL);

	HANDLE hEvent = CreateEvent(NULL, FALSE, FALSE, NULL);


	while (1)
	{
		ZeroMemory(&ov, sizeof(OVERLAPPED));
		ov.hEvent = hEvent;
		ConnectNamedPipe(hPipe, &ov);

		if (!WaitEvent(hEvent))
		{
			break;
		}

		char buf[256];
		DWORD read;
		ReadFile(hPipe, buf, sizeof(buf), &read, NULL);


		// 本体から送られてくるデータ
		// 'i' でインストール。'u'でアンインストール
		switch (buf[0])
		{
		case 'i':
			Install();
			break;
		case 'u':
			Uninstall();
			break;
		default:
			break;
		}

		DisconnectNamedPipe(hPipe);
	}

	CloseHandle(hEvent);
	CloseHandle(hPipe);

	return 0;
}


BOOL APIENTRY DllMain(HANDLE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	static HANDLE hThread = NULL;

	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		g_hEventExit = CreateEvent(NULL, TRUE, FALSE, NULL);
		Install();
		hThread = CreateThread(NULL, 0, pipeThread, NULL, 0, NULL);
		break;
	
	case DLL_PROCESS_DETACH:
		// 先に本体に終了通知を送っておく
		PostMessage(g_Info.hSandBurst, WM_USER + 1, (WPARAM)g_Info.hWnd, NULL);
		
		Uninstall();
		SetEvent(g_hEventExit);
		WaitForSingleObject(hThread, 1000);
		CloseHandle(hThread);
		break;
	}
	return TRUE;
}