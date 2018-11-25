#pragma once

#include "ppltasks.h"

using namespace Platform;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::Foundation::Metadata;

namespace WebpLib
{
	[WebHostHidden]
	public ref class WebpCodec sealed
	{
	private:
		WebpCodec();
	public:
		static	bool GetInfo(const Array<byte> ^data, __RPC__deref_out_opt int* width, __RPC__deref_out_opt int* height);
		static	WriteableBitmap^ Decode(const Array<byte> ^data);
		static  WriteableBitmap^ WebpCodec::Decode(WriteableBitmap^  bitmap, const Array<byte> ^data);
		static Array<byte> ^ Parse(const Array<byte> ^data, __RPC__deref_out_opt int* width, __RPC__deref_out_opt int* height);
	};
}