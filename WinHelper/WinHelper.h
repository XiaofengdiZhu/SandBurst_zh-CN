#pragma once

#include <Windows.h>

typedef struct _UNICODE_STRING {
	USHORT Length;
	USHORT MaximumLength;
	PWSTR  Buffer;
} UNICODE_STRING, *PUNICODE_STRING;

typedef enum _PROCESSINFOCLASS {
	ProcessBasicInformation = 0,
	ProcessWow64Information = 26
} PROCESSINFOCLASS;

typedef struct _PEB_LDR_DATA {
	BYTE       Reserved1[8];
	PVOID      Reserved2[3];
	LIST_ENTRY InMemoryOrderModuleList;
} PEB_LDR_DATA, *PPEB_LDR_DATA;

typedef struct _RTL_USER_PROCESS_PARAMETERS {
	BYTE			Reserved1[16];
	PVOID			Reserved2[10];
	UNICODE_STRING	ImagePathName;
	UNICODE_STRING	CommandLine;
} RTL_USER_PROCESS_PARAMETERS, *PRTL_USER_PROCESS_PARAMETERS;

typedef ULONG	PPS_POST_PROCESS_INIT_ROUTINE;
#define NTSTATUS			LONG
#define STATUS_SUCCESS		((NTSTATUS)0x00000000L)

typedef struct _PEB {
	BYTE							Reserved1[2];
	BYTE							BeingDebugged;
	BYTE							Reserved2[1];
	PVOID							Reserved3[2];
	PPEB_LDR_DATA					Ldr;
	PRTL_USER_PROCESS_PARAMETERS	ProcessParameters;
	BYTE							Reserved4[104];
	PVOID							Reserved5[52];
	PPS_POST_PROCESS_INIT_ROUTINE	PostProcessInitRoutine;
	BYTE							Reserved6[128];
	PVOID							Reserved7[1];
	ULONG							SessionId;
} PEB, *PPEB;

typedef struct _PROCESS_BASIC_INFORMATION {
	LONG		ExitStatus;
	PPEB		PebBaseAddress;
	ULONG_PTR	AffinityMask;
	LONG		BasePriority;
	ULONG_PTR	UniqueProcessId;
	ULONG_PTR	InheritedFromUniqueProcessId;
} PROCESS_BASIC_INFORMATION, *PPROCESS_BASIC_INFORMATION;

typedef NTSTATUS(NTAPI *PNtQueryInformationProcess)
(
	IN	HANDLE				ProcessHandle,
	IN	PROCESSINFOCLASS	ProcessInformationClass,
	OUT	PVOID				ProcessInformation,
	IN	ULONG				ProcessInformationLength,
	OUT	PULONG				ReturnLength	OPTIONAL
);

BOOL __stdcall GetCommandLineEx(DWORD processId, LPSTR commandLine, size_t size);