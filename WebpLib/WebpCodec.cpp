// ===============================================================================
// WebpLib.cpp
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================
#include "pch.h"
#include "WebpCodec.h"
#include "../webp/decode.h"
#include <wrl/implements.h>
#include <wrl\client.h>
#include <windows.storage.streams.h>
#include "Robuffer.h" 
using namespace WebpLib;
using namespace Platform;
using namespace Windows::Storage::Streams;
using namespace std;
using Windows::Devices::Enumeration::DeviceClass;
using Windows::Devices::Enumeration::DeviceInformation;
using Windows::Devices::Enumeration::DeviceInformationCollection;
using namespace Microsoft::WRL;
using namespace ABI::Windows::Foundation;

WebpCodec::WebpCodec()
{
}

void WebpCodec::GetInfo(const Array<byte> ^data, __RPC__deref_out_opt int* width, __RPC__deref_out_opt int* height) {
	WebPGetInfo(data->Data, data->Length, width, height);
}

WriteableBitmap^ WebpCodec::DecodeRGBA(const Array<byte> ^data) {
	int width = 0;
	int height = 0;
	byte* pixels = WebPDecodeRGBA(data->Data, data->Length, &width, &height);
	WriteableBitmap^ bitmap = ref new WriteableBitmap(width, height);
	IBuffer^ buffer = bitmap->PixelBuffer;
	Array<byte>^ pixelsArray = ref new Array<byte>(pixels, width * height * 4);
	//// Obtain IBufferByteAccess 
	ComPtr<IBufferByteAccess> pBufferByteAccess;
	ComPtr<IUnknown> pBuffer((IUnknown*)buffer);
	pBuffer.As(&pBufferByteAccess);
	byte *sourcePixels = nullptr;
	// Get pointer to pixel bytes 
	pBufferByteAccess->Buffer(&sourcePixels);
	memcpy(sourcePixels, (void *)pixelsArray->Data, pixelsArray->Length);
	bitmap->Invalidate();
	return bitmap;
}