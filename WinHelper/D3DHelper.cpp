#include <Windows.h>
#include <d3d9.h>
#include <d3dx9.h>
//#include <d3d11.h>
#pragma comment(lib, "d3d9.lib")
#pragma comment(lib, "d3dx9.lib")
//#pragma comment (lib, "d3d11.lib")

struct EffectDatas
{
	IDirect3DDevice9* device;
	IDirect3DVertexBuffer9* vBuffer;
	IDirect3DVertexDeclaration9* vDec;
	ID3DXEffect* effect;
};

struct MyVertex {
	D3DXVECTOR3 Pos;
	D3DXVECTOR3 Color;
};

struct FVFVertex
{
	FLOAT x, y, z;
	FLOAT u, v;
};

const DWORD CustomFVF = D3DFVF_XYZ | D3DFVF_DIFFUSE;


EffectDatas* __stdcall CreateEffect(IDirect3DDevice9* device, LPCWSTR fileName)
{
	EffectDatas* effect = new EffectDatas;
	ID3DXBuffer* errors;
	HRESULT hr = D3DXCreateEffectFromFile(device, fileName, NULL, NULL, 0, NULL, &effect->effect, &errors);

	if (hr != D3D_OK)
	{
		MessageBox(GetActiveWindow(), (LPCWSTR)errors->GetBufferPointer(), L"Error", 0);
	}

	effect->device = device;

	//頂点宣言の作成
	D3DVERTEXELEMENT9 elements[] = {
		//v0 = Position
		{ 0, 0,  D3DDECLTYPE_FLOAT3, D3DDECLMETHOD_DEFAULT, D3DDECLUSAGE_POSITION, 0 },

		//v3 = UV
		{ 0, 12,  D3DDECLTYPE_FLOAT2, D3DDECLMETHOD_DEFAULT, D3DDECLUSAGE_TEXCOORD, 0 },

		D3DDECL_END()
	};

	if (device->CreateVertexDeclaration(elements, &effect->vDec) != D3D_OK)
	{
		MessageBox(GetActiveWindow(), L"CreateVertexDeclaration", L"Error", 0);
	}

	return effect;
}

void __stdcall CreateVertexBuffer(EffectDatas* effect, float scale, float texSize, float clientWidth, float clientHeight, float menuHeight)
{
	float invTex = 1.0f / texSize;
	float invTex2 = invTex * 2.0f;
	float invWidth = 2.0f / clientWidth;
	float invWidth2 = invWidth * 2;
	float invHeight = 2.0f / clientHeight;
	float invHeight2 = invHeight * 2;
	float u2 = clientWidth  * invTex - invTex2 * 2;
	float v2 = clientHeight * invTex - invTex2 * 2;
	float menuRatio = menuHeight / clientHeight * 2.0f;

	FVFVertex v[] = {

		// 左下
		{ -1.0f + scale * invWidth, -1.0f + scale * invHeight2 - menuRatio, 0,  invTex, v2 },
		// 左上
		{ -1.0f + scale * invWidth, 1.0f - scale * invHeight, 0,  invTex, invTex },
		// 右上
		{ 1.0f - scale * invWidth2, 1.0f - scale * invHeight, 0,  u2, invTex },
		// 右下
		{ 1.0f - scale * invWidth2, -1.0f + scale * invHeight2 - menuRatio, 0,  u2, v2 },

	};	
	
	IDirect3DVertexBuffer9* p = NULL;
	effect->device->CreateVertexBuffer(4 * sizeof(FVFVertex), 0, CustomFVF, D3DPOOL_MANAGED, &p, NULL);

	void* buffer;
	p->Lock(0, 0, &buffer, 0);

	memcpy(buffer, v, sizeof(v));

	p->Unlock();

	effect->vBuffer = p;
}

void __stdcall ReleaseEffect(EffectDatas* effect)
{
	effect->effect->Release();
	effect->vBuffer->Release();
	effect->vDec->Release();
	delete effect;
}

void SetEffectParams(EffectDatas* effect, float scale, float texSize)
{
	ID3DXEffect* xEffect = effect->effect;
	D3DXHANDLE hScale = xEffect->GetParameterByName(NULL, "scale");
	D3DXHANDLE hA = xEffect->GetParameterByName(NULL, "a");
	D3DXHANDLE hParams = xEffect->GetParameterByName(NULL, "params");
	D3DXHANDLE hTexSize = xEffect->GetParameterByName(NULL, "texSize");

	float a = -1.0f;
	float params[5];
	params[0] = a + 3.0f;
	params[1] = a + 2.0f;
	params[2] = -a * 4.0f;
	params[3] = a * 8.0f;
	params[4] = a * 5.0f;

	float texSizeArr[2];
	texSizeArr[0] = texSize;
	texSizeArr[1] = texSize;

	xEffect->SetFloat(hScale, scale);
	xEffect->SetFloat(hA, a);
	xEffect->SetFloatArray(hParams, params, 5);
	xEffect->SetFloatArray(hTexSize, texSizeArr, 2);
}

void __stdcall DrawEffectVertex(EffectDatas* effect, IDirect3DTexture9* texture, float scale, float texSize)
{
	
	IDirect3DDevice9* device = effect->device;;
	ID3DXEffect* xEffect = effect->effect;
	D3DXMATRIX mat;
	D3DXMatrixIdentity(&mat);
	
	D3DXMatrixIdentity(&mat);
	xEffect->SetMatrix("matWorldViewProj", &mat);
	
	// 描画開始
	xEffect->SetTechnique("BicubicTec");
	xEffect->SetTexture("gTexture0", texture);

	SetEffectParams(effect, scale, texSize);

	UINT numPass;
	xEffect->Begin(&numPass, 0);
	xEffect->BeginPass(0);

	device->SetRenderState(D3DRS_LIGHTING, FALSE);
	device->LightEnable(0, FALSE);

	device->SetStreamSource(0, effect->vBuffer, 0, sizeof(FVFVertex));
	device->SetVertexDeclaration(effect->vDec);
	device->DrawPrimitive(D3DPT_TRIANGLEFAN, 0, 2);

	xEffect->EndPass();
	xEffect->End();
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam)
{
	return DefWindowProc(hWnd, Msg, wParam, lParam);
}

// ID3DDevice9::Presentの相対アドレスを得る
DWORD __stdcall GetD3DDevice9PresentRVA()
{
	// Windowの作成

	WNDCLASSEX wc;

	wc.cbSize = sizeof(WNDCLASSEX);
	wc.style = 0;
	wc.lpfnWndProc = WndProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = GetModuleHandle(NULL);
	wc.hIcon = NULL;
	wc.hCursor = NULL;
	wc.hbrBackground = NULL;
	wc.lpszMenuName = NULL;
	wc.lpszClassName = L"D3DDummy";
	wc.hIconSm = NULL;

	RegisterClassEx(&wc);

	HWND hWnd = CreateWindow(L"D3DDummy", L"D3DDummy", WS_OVERLAPPEDWINDOW, 100, 100, 256, 256,
		GetDesktopWindow(), NULL, wc.hInstance, NULL);

	// D3DDeviceの作成

	IDirect3D9 *d3d9 = Direct3DCreate9(D3D_SDK_VERSION);
	IDirect3DDevice9 *device9;
	D3DPRESENT_PARAMETERS param;

	ZeroMemory(&param, sizeof(param));
	param.Windowed = TRUE;
	param.SwapEffect = D3DSWAPEFFECT_DISCARD;
	param.BackBufferFormat = D3DFMT_UNKNOWN;

	LRESULT lresult = d3d9->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, hWnd,
		D3DCREATE_SOFTWARE_VERTEXPROCESSING,
		&param, &device9);

	// IDirect3DDevice9 から Presentメソッドのアドレスを得る
	DWORD result = 0;
	if (lresult == S_OK)
	{
		PDWORD p = (PDWORD)(*((PDWORD)(device9)) + 68);

		result = *p - (DWORD)GetModuleHandle(L"d3d9.dll");

		device9->Release();
	}

	d3d9->Release();
	DestroyWindow(hWnd);
	UnregisterClass(L"D3DDummy", GetModuleHandle(NULL));

	return result;
}


//ID3DDevice11::SetViewportsの相対アドレスを得る
/*
DWORD __stdcall GetD3D11SetViewportRVA()
{
	WNDCLASSEX wc;

	wc.cbSize = sizeof(WNDCLASSEX);
	wc.style = 0;
	wc.lpfnWndProc = WndProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = GetModuleHandle(NULL);
	wc.hIcon = NULL;
	wc.hCursor = NULL;
	wc.hbrBackground = NULL;
	wc.lpszMenuName = NULL;
	wc.lpszClassName = L"D3DDummy";
	wc.hIconSm = NULL;

	RegisterClassEx(&wc);

	HWND hWnd = CreateWindow(L"D3DDummy", L"D3DDummy", WS_OVERLAPPEDWINDOW, 100, 100, 256, 256,
		GetDesktopWindow(), NULL, wc.hInstance, NULL);



	HRESULT              hr;
	RECT                rect;
	DXGI_SWAP_CHAIN_DESC scDesc;

	::GetClientRect(hWnd, &rect);
	::ZeroMemory(&scDesc, sizeof(scDesc));
	scDesc.BufferCount = 1;
	scDesc.BufferDesc.Width = rect.right;
	scDesc.BufferDesc.Height = rect.bottom;
	scDesc.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	scDesc.BufferUsage = 0x20;
	scDesc.OutputWindow = hWnd;
	scDesc.SampleDesc.Count = 1;
	scDesc.SampleDesc.Quality = 0;
	scDesc.Windowed = TRUE;

	UINT flags = 0;
	D3D_FEATURE_LEVEL pLevels[] = { D3D_FEATURE_LEVEL_11_0 };
	D3D_FEATURE_LEVEL level;

	ID3D11Device*           pDevice;
	ID3D11DeviceContext*    pImmediateContext;
	IDXGISwapChain*         pSwapChain;

	hr = D3D11CreateDeviceAndSwapChain(NULL,
		D3D_DRIVER_TYPE_HARDWARE,
		NULL,
		flags,
		pLevels,
		1,
		7,
		&scDesc,
		&pSwapChain,
		&pDevice,
		&level,
		&pImmediateContext);


	DestroyWindow(hWnd);
	UnregisterClass(L"D3DDummy", GetModuleHandle(NULL));

	PDWORD p = (PDWORD)(*((PDWORD)(pImmediateContext)) + 176);

	pImmediateContext->Release();
	pSwapChain->Release();
	pDevice->Release();

	return *p - (DWORD)GetModuleHandle(L"d3d11.dll");
}
*/
