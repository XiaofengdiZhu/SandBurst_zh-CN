///////////////////////////////////////////////////////
//
// フックしたWindowsAPIの実装
//
///////////////////////////////////////////////////////


#include "SandBurstCore.h"



BOOL WINAPI HkGetCursorPos(LPPOINT lpPoint)
{
	BOOL Result;

	__asm
	{
		PUSH lpPoint
		MOV ECX, g_pGetCursorPos
		ADD ECX, 5
		PUSH RetLabel
		PUSH EBP
		MOV EBP, ESP
		JMP ECX
		RetLabel :
		MOV Result, EAX
	}

	if (lpPoint && Result)
	{
		POINT Pos;

		Pos.x = lpPoint->x;
		Pos.y = lpPoint->y;

		ScreenToClient(g_hWnd, &Pos);

		Pos.x = Pos.x * 100 / g_Info.Scale;
		Pos.y = Pos.y * 100 / g_Info.Scale;

		ClientToScreen(g_hWnd, &Pos);

		lpPoint->x = Pos.x;
		lpPoint->y = Pos.y;
	}


	return Result;
}

BOOL WINAPI HkSetCursorPos(int x, int y)
{
	BOOL Result;

	POINT Pos;

	Pos.x = x;
	Pos.y = y;

	ScreenToClient(g_hWnd, &Pos);

	Pos.x = Pos.x * g_Info.Scale / 100;
	Pos.y = Pos.y * g_Info.Scale / 100;

	ClientToScreen(g_hWnd, &Pos);

	x = Pos.x;
	y = Pos.y;

	__asm
	{
		PUSH y
		PUSH x
		MOV ECX, g_pSetCursorPos
		ADD ECX, 5
		PUSH RetLabel
		PUSH EBP
		MOV EBP, ESP
		JMP ECX
		RetLabel :
		MOV Result, EAX
	}

	return Result;
}

BOOL WINAPI HkMoveWindow(HWND hWnd, int x, int y, int nWidth, int nHeight, BOOL bRepaint)
{
	return SetWindowPos(hWnd, 0, x, y, nWidth, nHeight, SWP_NOZORDER);
}

BOOL WINAPI HkSetWindowPos(HWND hWnd, HWND hWndInsertAfter, int x, int y, int _cx, int _cy, UINT uFlags)
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

	_asm
	{
		PUSH uFlags
		PUSH _cy
		PUSH _cx
		PUSH y
		PUSH x
		PUSH hWndInsertAfter
		PUSH hWnd
		PUSH RetLabel

		MOV ECX, OFFSET g_SetWindowPosCode[1]
		MOV EAX, DWORD PTR[ECX]			// NtKernelに渡すシステムサービスの番号
		MOV ECX, g_pSetWindowPos
		ADD ECX, 5
		JMP ECX
		RetLabel :
	}
}

BOOL WINAPI HkSetWindowPlacement(HWND hWnd, WINDOWPLACEMENT *lpwndpl)
{
	std::map<HWND, PWINDOW_SIZE_INFO>::iterator it = g_ModifiedWindows.find(hWnd);
	if (it != g_ModifiedWindows.end())
	{
		return TRUE;
	}


	_asm
	{
		PUSH lpwndpl
		PUSH hWnd
		PUSH RetLabel
		MOV ECX, OFFSET g_SetWindowPlacementCode[1]
		MOV EAX, DWORD PTR[ECX]			// NtKernelに渡すシステムサービスの番号
		MOV ECX, g_pSetWindowPlacement
		ADD ECX, 5
		JMP ECX
		RetLabel :
	}
}


BOOL WINAPI HkClipCursor(RECT *lpRect)
{
	BOOL Result;

	POINT Pos;

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

	__asm
	{
		PUSH lpRect
		PUSH RetLabel
		MOV ECX, OFFSET g_ClipCursorCode[1]
		MOV EAX, DWORD PTR[ECX]			// NtKernelに渡すシステムサービスの番号
		MOV ECX, g_pClipCursor
		ADD ECX, 5
		JMP ECX
		RetLabel :
		MOV Result, EAX
	}

	return Result;
}




HDC WINAPI HkGetDC(HWND hWnd)
{

	HDC hDC;

	_asm
	{
		PUSH hWnd
		PUSH RetLabel
		MOV ECX, OFFSET g_GetDCCode[1]
		MOV EAX, DWORD PTR[ECX]			// NtKernelに渡すシステムサービスの番号
		MOV ECX, g_pGetDC
		ADD ECX, 5
		JMP ECX
		RetLabel :
		MOV hDC, EAX
	}

	if (hDC)
	{
		HWND hDesktop = GetDesktopWindow();
		if ((hWnd == NULL) || (hWnd == hDesktop))
		{
			g_hDC.push_back(hDC);
		}
	}

	return hDC;
}


BOOL WINAPI HkReleaseDC(HWND hWnd, HDC hDC)
{

	BOOL Result;

	_asm
	{
		PUSH hDC
		PUSH hWnd
		MOV ECX, g_pReleaseDC
		ADD ECX, 5
		PUSH RetLabel
		PUSH EBP
		MOV EBP, ESP
		JMP ECX
		RetLabel :
		MOV Result, EAX
	}

	if (Result)
	{
		for (std::vector<HDC>::iterator it = g_hDC.begin(); it != g_hDC.end(); it++)
		{
			if (*it == hDC)
			{
				g_hDC.erase(it);
				break;
			}
		}
	}


	return Result;
}

BOOL WINAPI HkNtUserReleaseDC(HDC hDC)
{

	BOOL Result;

	_asm
	{
		PUSH hDC
		PUSH RetLabel
		MOV ECX, OFFSET g_ReleaseDCCode[1]
		MOV EAX, DWORD PTR[ECX]			// NtKernelに渡すシステムサービスの番号
		MOV ECX, g_pReleaseDC
		ADD ECX, 5
		JMP ECX
		RetLabel :
		MOV Result, EAX
	}

	if (Result)
	{
		for (std::vector<HDC>::iterator it = g_hDC.begin(); it != g_hDC.end(); it++)
		{
			if (*it == hDC)
			{
				g_hDC.erase(it);
				break;
			}
		}
	}


	return Result;
}


BOOL WINAPI HkBitBlt(HDC hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
	HDC hdcSrc, int nXSrc, int nYSrc, DWORD dwRop)
{

	BOOL Result;

	for (unsigned int i = 0; i < g_hDC.size(); i++)
	{
		// ソースDCがデスクトップだった場合、ゲームのウィンドウのDCから描画する
		if (hdcSrc == g_hDC[i])
		{
			HDC hDC = GetDC(g_hWnd);

			Result = BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hDC, 0, 0, dwRop);

			ReleaseDC(g_hWnd, hDC);

			LeaveCriticalSection(&g_CriticalSection);

			return Result;
		}
	}

	_asm
	{
		PUSH dwRop
		PUSH nYSrc
		PUSH nXSrc
		PUSH hdcSrc
		PUSH nHeight
		PUSH nWidth
		PUSH nYDest
		PUSH nXDest
		PUSH hdcDest
		MOV ECX, g_pBitBlt
		ADD ECX, 5
		PUSH RetLabel
		PUSH EBP
		MOV EBP, ESP
		JMP ECX
		RetLabel :
		MOV Result, EAX
	}

	return Result;
}

BOOL WINAPI HkGetWindowRect(HWND hWnd, LPRECT lpRect)
{

	BOOL Result;

	if (g_GetWindowRectArg != 0) {
		_asm
		{
			PUSH lpRect
			PUSH hWnd
			PUSH RetLabel
			PUSH g_RectNo
			PUSH g_GetWindowRectArg
			MOV ECX, g_pGetWindowRect
			ADD ECX, 7
			JMP ECX
			RetLabel :
			MOV Result, EAX
		}
	}
	else
	{
		_asm
		{
			PUSH lpRect
			PUSH hWnd
			PUSH RetLabel2
			PUSH EBP
			MOV EBP, ESP
			MOV ECX, g_pGetWindowRect
			ADD ECX, 5
			JMP ECX
			RetLabel2 :
			MOV Result, EAX
		}
	}

	if ((hWnd == g_hWnd) && (lpRect))
	{

		lpRect->right = lpRect->left + g_Info.WindowSize.x;
		lpRect->bottom = lpRect->top + g_Info.WindowSize.y;

		Result = TRUE;
	}


	return Result;
}



BOOL WINAPI HkGetClientRect(HWND hWnd, LPRECT lpRect)
{

	BOOL Result;


	if (g_GetClientRectArg != 0) {
		_asm
		{
			PUSH lpRect
			PUSH hWnd
			PUSH RetLabel
			PUSH g_RectNo
			PUSH g_GetClientRectArg
			MOV ECX, g_pGetClientRect
			ADD ECX, 7
			JMP ECX
			RetLabel :
			MOV Result, EAX
		}
	}
	else
	{
		_asm
		{
			PUSH lpRect
			PUSH hWnd
			PUSH RetLabel2
			PUSH EBP
			MOV EBP, ESP
			MOV ECX, g_pGetClientRect
			ADD ECX, 5
			JMP ECX
			RetLabel2 :
			MOV Result, EAX
		}
	}

	if ((hWnd == g_hWnd) && (lpRect))
	{

		lpRect->right = lpRect->left + g_Info.ClientSize.x;
		lpRect->bottom = lpRect->top + g_Info.ClientSize.y;

		Result = TRUE;
	}


	return Result;
}

BOOL WINAPI HkTrackPopupMenuEx(
	HMENU hmenu,       // ショートカットメニューのハンドル
	UINT fuFlags,      // オプション
	int x,             // 水平位置
	int y,             // 垂直位置
	HWND hwnd,         // ウィンドウのハンドル
	LPTPMPARAMS lptpm  // オーバーラップを禁止する領域
)
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
		PUSH lptpm
		PUSH hwnd
		PUSH y
		PUSH x
		PUSH fuFlags
		PUSH hmenu
		PUSH RetLabel

		MOV ECX, OFFSET g_TrackPopupMenuExCode[1]
		MOV EAX, DWORD PTR[ECX]			// NtKernelに渡すシステムサービスの番号
		MOV ECX, g_pTrackPopupMenuEx
		ADD ECX, 5
		JMP ECX
		RetLabel :
		MOV result, EAX
	}

//	RestoreCapturedDC(hBuffer, hBitmap, hOld);
	if (visible)
	{
		ShowWindow(g_Info.hShadow, SW_SHOW);
	}

	return result;
}


HRESULT WINAPI HkD3D9Present(
	IDirect3DDevice9* Device,
	RECT * pSourceRect,
	RECT * pDestRect,
	HWND hDestWindowOverride,
	RGNDATA * pDirtyRegion)
{
	EnterCriticalSection(&g_CriticalSection);


	RECT Dest;
	HRESULT Result;

	if (((hDestWindowOverride == g_Info.hWnd) || (hDestWindowOverride == NULL)) && pDestRect == NULL)
	{
		Dest.left = 0;
		Dest.right = g_Info.ClientSize.x;
		Dest.top = 0;
		Dest.bottom = g_Info.ClientSize.y;

		pDestRect = &Dest;
	}

	_asm
	{
		PUSH pDirtyRegion
		PUSH hDestWindowOverride
		PUSH pDestRect
		PUSH pSourceRect
		PUSH Device
		PUSH RetLabel
		PUSH EBP
		MOV EBP, ESP
		MOV ECX, g_pD3D9Present;
		ADD ECX, 5
		JMP ECX
		RetLabel :
		MOV Result, EAX
	}

	LeaveCriticalSection(&g_CriticalSection);

	return Result;
}


void WINAPI
hkD3D11RSSetViewports(
	ID3D11DeviceContext* context,
	UINT NumViewports,
	D3D11_VIEWPORT *pViewports
)
{
	for (UINT i = 0; i < NumViewports; i++)
	{
		pViewports[i].Width = g_Info.ClientSize.x;
		pViewports[i].Height = g_Info.ClientSize.y;
	}

	_asm
	{
		PUSH pViewports
		PUSH NumViewports
		PUSH context
		PUSH RetLabel
		PUSH EBP
		MOV EBP, ESP
		MOV ECX, g_pD3D11SetViewport
		ADD ECX, 5
		JMP ECX
		RetLabel :
	}
}