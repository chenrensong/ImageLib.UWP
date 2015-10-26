#pragma once

#include "ppltasks.h"

using namespace Platform;
using namespace Windows::UI::Xaml::Media::Imaging;

namespace WebpLib
{
	public ref class WebpCodec sealed
	{
	private:
		WebpCodec();
	public:
		static	void GetInfo(const Array<byte> ^data, __RPC__deref_out_opt int* width, __RPC__deref_out_opt int* height);
		static	WriteableBitmap^ DecodeRGBA(const Array<byte> ^data);
	};
}