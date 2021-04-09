#pragma once

#include <Windows.h>
#include <map>
#include <vector>
#include <d3d9.h>
#include <d3d11.h>

// API�t�b�N���
typedef struct
{
	BOOL MessageHook;
	BOOL SetCursorPosHook;
	BOOL GetCursorPosHook;
	BOOL MoveWindowHook;
	BOOL SetWindowPosHook;
	BOOL SetWindowPlacementHook;
	BOOL ClipCursolHook;
	BOOL ScreenShotHook;
	BOOL GetWindowRectHook;
	BOOL GetClientRectHook;
	BOOL D3DHook;
	BOOL WM_SIZE_Hook;
	BOOL WM_WINDOWPOS_Hook;
}API_HOOK_INFO;

enum HOOK_TYPE
{
	HookTypeApi = 0,
	HookTypeWow64,
	FORCE_DWORD = 0xFFFFFFFF
};

// �t�b�N���\����
typedef struct
{
	HWND hWnd;
#ifndef _WIN64
	DWORD Reserved1;	// 64bit�p�o�b�t�@
#endif
	HWND hSandBurst;
#ifndef _WIN64
	DWORD Reserved2;	// 64bit�p�o�b�t�@
#endif
	HWND hShadow;
#ifndef _WIN64
	DWORD Reserved3;	// 64bit�p�o�b�t�@
#endif
	int Scale;
	POINT WindowSize;
	POINT ClientSize;
	DWORD D3D9PresentRVA;
	DWORD D3D11SetViewportRVA;
	BOOL ModifiesWindowSize;
	BOOL ModifiesChildWindowSize;
	HOOK_TYPE HookType;
	BOOL CentralizesWindow;
	BOOL ExcludesTaskbar;
	RECT Display;
	API_HOOK_INFO ApiHooks;
}HOOK_INFO;

typedef struct
{
	HWND hWnd;
	POINT OriginalSize;
	POINT ModifiedSize;
}WINDOW_SIZE_INFO, *PWINDOW_SIZE_INFO;

#define SWP_SANDBURST_CONTROL 0x80000000


// API �̃I���W�i���A�h���X
extern FARPROC g_pGetCursorPos;
extern FARPROC g_pSetCursorPos;
extern FARPROC g_pMoveWindow;
extern FARPROC g_pSetWindowPos;
extern FARPROC g_pSetWindowPlacement;
extern FARPROC g_pClipCursor;
extern FARPROC g_pGetDC;
extern FARPROC g_pReleaseDC;
extern FARPROC g_pBitBlt;
extern FARPROC g_pGetWindowRect;
extern FARPROC g_pGetClientRect;
extern FARPROC g_pD3D9Present;
extern FARPROC g_pTrackPopupMenuEx;
extern FARPROC g_pD3D11SetViewport;

// API �̃I���W�i���R�[�h
extern BYTE g_GetCursorPosCode[5];
extern BYTE g_SetCursorPosCode[5];
extern BYTE g_MoveWindowCode[5];
extern BYTE g_SetWindowPosCode[5];
extern BYTE g_SetWindowPlacementCode[5];
extern BYTE g_ClipCursorCode[5];
extern BYTE g_GetDCCode[5];
extern BYTE g_ReleaseDCCode[5];
extern BYTE g_BitBltCode[5];
extern BYTE g_GetWindowRectCode[5];
extern BYTE g_GetClientRectCode[5];
extern DWORD g_GetWindowRectArg;
extern DWORD g_GetClientRectArg;		// GetWindowRect,ClientRect��OS���Ƃɐ擪�̃R�[�h���Ⴄ
extern BYTE g_D3D9PresentCode[5];
extern BYTE g_TrackPopupMenuExCode[5];
extern BYTE g_D3D11SetViewportCode[5];

// �{���̃E�B���h�E�v���V�[�W��
extern WNDPROC g_OriginalProc;

// �q�E�B���h�E�̃v���V�[�W��
extern std::map<HWND, WNDPROC> g_Procs;

// �f�o�C�X�R���e�L�X�g�n���h��
extern std::vector<HDC> g_hDC;

// �t�b�N�̏��
extern HOOK_INFO g_Info;

// �E�B���h�E�n���h��
extern HWND g_hWnd;

// Direct3D�p�̃N���e�B�J���Z�N�V����
extern CRITICAL_SECTION g_CriticalSection;

// �T�C�Y���C�����ꂽ�E�B���h�E
extern std::map<HWND, PWINDOW_SIZE_INFO> g_ModifiedWindows;

// GetXXXRect�ɓn������(7��8�ňقȂ�)
extern DWORD g_RectNo;

// �t�b�N����API
extern HDC WINAPI HkGetDC(HWND hWnd);
extern BOOL WINAPI HkReleaseDC(HWND hWnd, HDC hDC);
extern BOOL WINAPI HkBitBlt(HDC hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
	HDC hdcSrc, int nXSrc, int nYSrc, DWORD dwRop);
extern BOOL WINAPI HkGetWindowRect(HWND hWnd, LPRECT lpRect);
extern BOOL WINAPI HkGetClientRect(HWND hWnd, LPRECT lpRect);
extern BOOL WINAPI HkGetCursorPos(LPPOINT lpPoint);
extern BOOL WINAPI HkSetCursorPos(int x, int y);
extern BOOL WINAPI HkMoveWindow(HWND hWnd, int x, int y, int nWidth, int nHeight, BOOL bRepaint);
extern BOOL WINAPI HkSetWindowPos(HWND hWnd, HWND hWndInsertAfter, int x, int y, int _cx, int _cy, UINT uFlags);
extern BOOL WINAPI HkSetWindowPlacement(HWND hWnd, WINDOWPLACEMENT *lpwndpl);
extern BOOL WINAPI HkClipCursor(RECT *lpRect);
extern BOOL WINAPI HkTrackPopupMenuEx(HMENU hmenu, UINT fuFlags, int x, int y, HWND hwnd, LPTPMPARAMS lptpm);
extern BOOL WINAPI HkNtUserReleaseDC(HDC hDC);

extern HRESULT WINAPI HkD3D9Present(
	IDirect3DDevice9* Device,
	RECT * pSourceRect,
	RECT * pDestRect,
	HWND hDestWindowOverride,
	RGNDATA * pDirtyRegion);

void WINAPI
hkD3D11RSSetViewports(
	ID3D11DeviceContext* context,
	UINT NumViewports,
	D3D11_VIEWPORT *pViewports
);

extern FARPROC NtGetProcAddress(HMODULE hModule, LPCSTR lpProcName);

extern BOOL Hook(FARPROC APIAddress, LPVOID HookProc);
extern void Wow64Init();
extern void Wow64Hook();
extern void Wow64Unhook();
extern BOOL Wow64IsHooked();
typedef void (WINAPI *procRtlGetVersion)(OSVERSIONINFOEX*);

VOID UpdateThumbnailDC();
HDC GetDeskTopDC();
VOID CreateCapturedDC(HDC OUT *hBuffer, HBITMAP OUT *hBitmap, HGDIOBJ OUT *hOldObj);
VOID RestoreCapturedDC(HDC hBuffer, HBITMAP hBitmap, HGDIOBJ hOldObj);
DWORD GetOsVersion();
#define UNHOOK(a, b) if (a) { WriteProcessMemory(GetCurrentProcess(), a, b, sizeof(b), NULL);  a = NULL; }
