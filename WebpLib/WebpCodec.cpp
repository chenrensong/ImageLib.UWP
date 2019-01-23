// ===============================================================================
// WebpLib.cpp
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================
#include "pch.h"
#include "WebpCodec.h"
#include "./webp/decode.h"
#include "./webp/demux.h"
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

bool WebpCodec::GetInfo(const Array<byte> ^data, __RPC__deref_out_opt int* width, __RPC__deref_out_opt int* height) {
	return WebPGetInfo(data->Data, data->Length, width, height) ? true : false;
}

bool WebpCodec::GetFrameInfo(const Array<byte> ^data, __RPC__deref_out_opt int* numLoops, __RPC__deref_out_opt int* numFrames) {

	WebPData webpData = WebPData();
	webpData.bytes = reinterpret_cast<const uint8_t*>(data->Data);
	webpData.size = static_cast<std::size_t>(data->Length);
	WebPDemuxer *demuxer = WebPDemux(&webpData);
	if (!demuxer)
	{
		return false;
	}

	*numLoops = static_cast<int>(WebPDemuxGetI(demuxer, WEBP_FF_LOOP_COUNT));
	*numFrames = static_cast<int>(WebPDemuxGetI(demuxer, WEBP_FF_FRAME_COUNT));
	WebPIterator iter;
	iter.duration = 100;
	if (!WebPDemuxGetFrame(demuxer, 1, &iter))
	{
		WebPDemuxDelete(demuxer);
		return false;
	}

	WebPDemuxDelete(demuxer);
	return true;
}

bool WebpCodec::GetInfo(const Array<byte> ^data, __RPC__deref_out_opt int* width, __RPC__deref_out_opt int* height,
	__RPC__deref_out_opt bool* has_alpha, __RPC__deref_out_opt bool* has_animation, __RPC__deref_out_opt int* format) {
	WebPBitstreamFeatures features = WebPBitstreamFeatures();
	VP8StatusCode statusCode = WebPGetFeatures(data->Data, data->Length, &features);
	if (statusCode != VP8_STATUS_OK) {
		return false;
	}
	if (width != NULL) {
		*width = features.width;
	}
	if (height != NULL) {
		*height = features.height;
	}
	*has_alpha = features.has_alpha == 1;
	*has_animation = features.has_animation == 1;
	*format = features.format;
	return true;
}

Array<byte> ^ WebpCodec::Parse(const Array<byte> ^data, __RPC__deref_out_opt int* width, __RPC__deref_out_opt int* height) {
	if (!WebPGetInfo(data->Data, data->Length, width, height)) {
		return nullptr;
	}
	byte* pixels = WebPDecodeBGRA(data->Data, data->Length, width, height);
	Array<byte>^ pixelsArray = ref new Array<byte>(pixels, *width * *height * 4);
	delete pixels;
	return pixelsArray;
}

WriteableBitmap^ WebpCodec::Decode(WriteableBitmap^  bitmap, const Array<byte> ^data) {
	int width = bitmap->PixelWidth;
	int height = bitmap->PixelHeight;
	byte* pixels = WebPDecodeBGRA(data->Data, data->Length, &width, &height);
	IBuffer^ buffer = bitmap->PixelBuffer;
	ComPtr<IBufferByteAccess> pBufferByteAccess;
	ComPtr<IUnknown> pBuffer((IUnknown*)buffer);
	pBuffer.As(&pBufferByteAccess);
	byte *sourcePixels = nullptr;
	pBufferByteAccess->Buffer(&sourcePixels);
	memcpy(sourcePixels, (void *)pixels, width * height * 4);
	bitmap->Invalidate();
	delete pixels;
	pixels = nullptr;
	return bitmap;
}

WriteableBitmap^ WebpCodec::Decode(const Array<byte> ^data) {
	int width = 0;
	int height = 0;
	if (!WebPGetInfo(data->Data, data->Length, &width, &height)) {
		return nullptr;
	}
	WriteableBitmap^ bitmap = ref new WriteableBitmap(width, height);
	Decode(bitmap, data);
	return bitmap;
}
