#include "WinHelper.h"
#include <string>

using namespace std;



BOOL GetProcessBasicInformation(HANDLE hProcess, PROCESS_BASIC_INFORMATION* pbi)
{
	HMODULE ntdll = GetModuleHandle(L"ntdll.dll");
	PNtQueryInformationProcess NtQueryInformationProcess = (PNtQueryInformationProcess)GetProcAddress(ntdll, "NtQueryInformationProcess");


	NTSTATUS status = NtQueryInformationProcess(hProcess, ProcessBasicInformation, pbi, sizeof(PROCESS_BASIC_INFORMATION), NULL);


	return status == STATUS_SUCCESS;
}

// コマンドライン引数から実行ファイル名以外を結合して１つの文字列にする
void CombineCommandLine(LPWSTR* args, LPWSTR command, int numArgs, int size)
{
	
	wstring str = L"";

	for (int i = 1; i < numArgs; i++)
	{
		wstring arg = args[i];

		if (i != 1)
		{
			str += L" ";
		}

		if ((wcschr(args[i], L' ') || ((wcschr(args[i], L'\\')) && (wcschr(args[i], L':')))))
		{
			str += (L"\"" + arg + L"\"");
		}
		else
		{
			str += args[i];
		}
	}

	wcscpy_s(command, size / 2, str.c_str());
}

// 指定したプロセスのコマンドラインを取得する
// 実行ファイルパスは取り除かれる
BOOL __stdcall GetCommandLineEx(DWORD processId, OUT LPWSTR commandLine, size_t size)
{
	
	HANDLE hProcess = 0;
	PROCESS_BASIC_INFORMATION	pbi;
	PEB							peb;
	RTL_USER_PROCESS_PARAMETERS peb_upp;
	LPWSTR buffer = NULL;;

	__try 
	{
		// 対象プロセスのPEBを取得
		hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, processId);

		if (!hProcess)
			return FALSE;

		if (!GetProcessBasicInformation(hProcess, &pbi))
			return FALSE;

		if (!ReadProcessMemory(hProcess, pbi.PebBaseAddress, &peb, sizeof(peb), NULL))
			return FALSE;

		if (!ReadProcessMemory(hProcess, peb.ProcessParameters, &peb_upp, sizeof(RTL_USER_PROCESS_PARAMETERS), NULL))
			return FALSE;

		if (size < peb_upp.CommandLine.Length + 2)
			return FALSE;

		buffer = (LPWSTR)malloc(peb_upp.CommandLine.Length + 2);

		// コマンドラインを取得
		if (!ReadProcessMemory(hProcess, peb_upp.CommandLine.Buffer, buffer, peb_upp.CommandLine.Length + 2, NULL))
			return FALSE;

		// コマンドラインを分割
		int numArgs;
		LPWSTR* args = CommandLineToArgvW(buffer, &numArgs);

		CombineCommandLine(args, commandLine, numArgs, size);

		LocalFree(args);

		return TRUE;
	}
	__finally
	{
		if (hProcess != 0) 
			CloseHandle(hProcess);

		if (buffer)
			free(buffer);
	}
	
}
