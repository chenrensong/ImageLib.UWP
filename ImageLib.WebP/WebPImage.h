#pragma once

#include "WebPBitmapFrame.h"

using namespace Platform;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;

namespace ImageLib
{
	namespace WebP
	{
		[Windows::Foundation::Metadata::WebHostHidden]
		public ref class WebPImage sealed
		{
		internal:
			WebPImage();
			std::shared_ptr<WebPDemuxerWrapper> spDemuxer;

			static WebPImage^ CreateFromByteArray(std::vector<uint8> vBuffer);

			static WriteableBitmap^ DecodeFromByteArray(std::vector<uint8> vBuffer);
		private:
			int pixelWidth;
			int pixelHeight;
			int numFrames;
			int loopCount;
			int totalDuration;

			//const Array<int>^ frameDurationsMs;
			Array<WebPBitmapFrame^>^ frames;

		public:
			static WebPImage^ CreateFromByteArray(const Array<uint8> ^bytes);

			static WriteableBitmap^ DecodeFromByteArray(const Array<uint8> ^bytes);

			property int PixelWidth
			{
				int get() { return pixelWidth; }
			}

			property int PixelHeight
			{
				int get() { return pixelHeight; }
			}

			property int LoopCount
			{
				int get() { return loopCount; }
			}

			property int TotalDuration
			{
				int get() { return totalDuration; }
			}

			//property const Array<int>^ FrameDurations
			//{
			//	const Array<int>^ get() { return frameDurationsMs; }
			//}

			property Array<WebPBitmapFrame^>^ Frames
			{
				Array<WebPBitmapFrame^>^ get() { return frames; }
			}

			//WebPBitmapFrame^ GetFrame(int index);
		};
	}
}
