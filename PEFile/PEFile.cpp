#include <windows.h>
#include <DbgHelp.h>
#include <tchar.h>
#include <stdio.h>
#pragma comment(lib, "DbgHelp.lib")

#define RELEASE(x) if(x!=NULL){free(x); x=NULL;}

#ifdef _WIN64
typedef unsigned long long _tuint;
#else 
typedef unsigned int _tuint;
#endif

typedef struct
{
	DWORD Address;
	WORD Oridinal;
	char Name[128];
}EXPORT_DATA, *PEXPORT_DATA;



class CPEFile
{
private:
	void	*FMem;
	PIMAGE_DOS_HEADER FImageDosHeader;
	IMAGE_NT_HEADERS32* FImageNtHeader;

	INT		FSectionCount;
	INT		FFileSize;
	PEXPORT_DATA FExportDatas;
	INT		FExportCount;
	PIMAGE_SECTION_HEADER *FSections;
	_tuint	RVAToRAW(_tuint RVA);
	_tuint	RVAToMEM(_tuint RVA);

public:
	CPEFile();
	~CPEFile();
	BOOL	LoadFromFile(CHAR* fileName);

	BOOL	Release();
	_tuint	GetProcRVA(CHAR* procName);

};


CPEFile::CPEFile()
{
	FMem = NULL;
	FSections = NULL;
	FExportDatas = NULL;
}

CPEFile::~CPEFile()
{
	Release();
}

BOOL CPEFile::Release()
{
	RELEASE(FMem);
	RELEASE(FSections);
	RELEASE(FExportDatas);
	return TRUE;
}


BOOL CPEFile::LoadFromFile(CHAR* fileName)
{
	Release();
	HANDLE hFile = CreateFileA(fileName, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, 0);

	if (hFile == INVALID_HANDLE_VALUE)
	{
		MessageBox(GetActiveWindow(), _T("CreateFile failed\n"), _T("Error"), 0);
		MessageBox(GetActiveWindow(), fileName, _T("ƒtƒ@ƒCƒ‹–¼"), 0);
		return FALSE;
	}

	ULONG size = GetFileSize(hFile, NULL);
	FMem = malloc(size);

	DWORD read;
	ReadFile(hFile, FMem, size, &read, NULL);

	CloseHandle(hFile);



	this->FImageDosHeader = (PIMAGE_DOS_HEADER)FMem;
	this->FImageNtHeader = (IMAGE_NT_HEADERS32*)((_tuint)FImageDosHeader + FImageDosHeader->e_lfanew);
	FSectionCount = FImageNtHeader->FileHeader.NumberOfSections;


	IMAGE_OPTIONAL_HEADER32* opHeader = (IMAGE_OPTIONAL_HEADER32*)&FImageNtHeader->OptionalHeader;

	PIMAGE_SECTION_HEADER sectionEntry = IMAGE_FIRST_SECTION(FImageNtHeader);

	FSections = (PIMAGE_SECTION_HEADER*)malloc(sizeof(PIMAGE_SECTION_HEADER) * FSectionCount);
	for (int i = 0; i < FSectionCount; i++)
	{
		FSections[i] = sectionEntry;
		sectionEntry++;
	}

	PIMAGE_EXPORT_DIRECTORY exDir = (PIMAGE_EXPORT_DIRECTORY)ImageDirectoryEntryToData(FMem, 0, IMAGE_DIRECTORY_ENTRY_EXPORT, &size);
	PDWORD exTable = (PDWORD)(RVAToMEM((exDir->AddressOfFunctions)));
	PDWORD nameTable = (PDWORD)(RVAToMEM(exDir->AddressOfNames));
	PWORD orgTable = (PWORD)(RVAToMEM(exDir->AddressOfNameOrdinals));
	char* funcName;

	FExportCount = exDir->NumberOfFunctions;
	FExportDatas = (PEXPORT_DATA)malloc(FExportCount * sizeof(EXPORT_DATA));


	for (unsigned int i = 0; i < exDir->NumberOfFunctions; i++)
	{

		funcName = (char*)RVAToMEM((_tuint)nameTable[i]);
		FExportDatas[i].Address = exTable[orgTable[i]];
		strcpy_s(FExportDatas[i].Name, funcName);
	}


	return TRUE;
}


_tuint CPEFile::RVAToRAW(_tuint RVA)
{
	int i;
	_tuint tmp;


	for (i = 0; i < FSectionCount; i++)
	{
		tmp = FSections[i]->VirtualAddress;
		if ((RVA >= tmp) && (RVA <= (tmp + FSections[i]->SizeOfRawData)))
		{
			return FSections[i]->PointerToRawData + RVA - FSections[i]->VirtualAddress;
		}
	}

	return 0;
}

_tuint CPEFile::RVAToMEM(_tuint RVA)
{
	_tuint m = RVAToRAW(RVA);

	if (m)
		return m + (_tuint)FMem;
	else
		return 0;
}

_tuint CPEFile::GetProcRVA(CHAR* procName)
{
	for (int i = 0; i < FExportCount; i++)
	{
		if (strcmp(procName, (char*)FExportDatas[i].Name) == 0)
			return FExportDatas[i].Address;
	}

	return 0;
}

void* __stdcall PeInit()
{
	return new CPEFile();
}

void __stdcall PeFree(CPEFile *peFile)
{
	delete peFile;
}

BOOL __stdcall PeLoadFromFile(CPEFile *peFile, CHAR* fileName)
{
	return peFile->LoadFromFile(fileName);
}

BOOL __stdcall PeReleaseFile(CPEFile *peFile)
{
	return peFile->Release();
}



_tuint __stdcall PeGetProcRVA(CPEFile *peFile, CHAR* procName)
{
	return peFile->GetProcRVA(procName);
}


BOOL APIENTRY DllMain(HANDLE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
	)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_PROCESS_DETACH:
		break;
	}

	return TRUE;
}