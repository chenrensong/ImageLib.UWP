#include "pch.h"
#include "WebPImage.h"

using namespace Windows::Storage;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::Storage::Streams;
using namespace ImageLib::WebP;
using namespace Platform;

WebPImage::WebPImage()
{

}

WebPImage ^ ImageLib::WebP::WebPImage::CreateFromByteArray(std::vector<uint8> vBuffer)
{

	WebPImage^ image = ref new WebPImage();
	WebPData webPData;
	webPData.bytes = vBuffer.data();
	webPData.size = vBuffer.size();

	auto spDemuxer = std::unique_ptr<WebPDemuxer, decltype(&WebPDemuxDelete)>
	{
		WebPDemux(&webPData),
		WebPDemuxDelete
	};
	if (!spDemuxer)
	{
		throw ref new InvalidArgumentException(ref new String(L"Failed to create demuxer"));
		return nullptr;
	}

	image->pixelWidth = WebPDemuxGetI(spDemuxer.get(), WEBP_FF_CANVAS_WIDTH);
	image->pixelHeight = WebPDemuxGetI(spDemuxer.get(), WEBP_FF_CANVAS_HEIGHT);
	image->numFrames = WebPDemuxGetI(spDemuxer.get(), WEBP_FF_FRAME_COUNT);
	image->loopCount = WebPDemuxGetI(spDemuxer.get(), WEBP_FF_LOOP_COUNT);

	std::vector<WebPBitmapFrame^> frames;
	int durationMs = 0;
	//std::vector<int> frameDurationsMs;
	WebPIterator iter;
	if (WebPDemuxGetFrame(spDemuxer.get(), 1, &iter))
	{
		do
		{
			durationMs += iter.duration;
			//frameDurationsMs.push_back(iter.duration);
			WebPBitmapFrame^ frame = ref new WebPBitmapFrame();
			frame->spDemuxer = image->spDemuxer;
			frame->frameNum = iter.frame_num;
			frame->offset = Point(static_cast<float>(iter.x_offset), static_cast<float>(iter.y_offset));
			frame->duration = iter.duration;
			frame->width = iter.width;
			frame->height = iter.height;
			frame->disposeToBackgroundColor = iter.dispose_method == WEBP_MUX_DISPOSE_BACKGROUND;
			frame->blendWithPreviousFrame = iter.blend_method == WEBP_MUX_BLEND;
			//frame->pPayload = std::make_unique<uint8_t[]>(iter.fragment.size);
			frame->pPayload = iter.fragment.bytes;
			frame->payloadSize = iter.fragment.size;

			//CopyMemory(frame->pPayload.get(), iter.fragment.bytes, iter.fragment.size);

			frames.push_back(frame);

		} while (WebPDemuxNextFrame(&iter));

		WebPDemuxReleaseIterator(&iter);
	}

	image->totalDuration = durationMs;
	//spNativeContext->frameDurationsMs = ref new Array<int>(frameDurationsMs.data(), frameDurationsMs.size());
	image->frames = ref new Array<WebPBitmapFrame^>(frames.data(), frames.size());
	image->spDemuxer = std::shared_ptr<WebPDemuxerWrapper>(new WebPDemuxerWrapper(std::move(spDemuxer), std::move(vBuffer)));

	return image;
}

WriteableBitmap ^ ImageLib::WebP::WebPImage::DecodeFromByteArray(std::vector<uint8> vBuffer)
{

	WebPData webPData;
	webPData.bytes = vBuffer.data();
	webPData.size = vBuffer.size();

	auto spDemuxer = std::unique_ptr<WebPDemuxer, decltype(&WebPDemuxDelete)>
	{
		WebPDemux(&webPData),
		WebPDemuxDelete
	};
	if (!spDemuxer)
	{
		throw ref new InvalidArgumentException(ref new String(L"Failed to create demuxer"));
		return nullptr;
	}

	WebPIterator iter;
	if (WebPDemuxGetFrame(spDemuxer.get(), 1, &iter))
	{
		WebPDecoderConfig config;
		int ret = WebPInitDecoderConfig(&config);
		if (!ret)
		{
			throw ref new FailureException(ref new String(L"WebPInitDecoderConfig failed"));
		}

		ret = (WebPGetFeatures(iter.fragment.bytes, iter.fragment.size, &config.input) == VP8_STATUS_OK);
		if (!ret)
		{
			throw ref new FailureException(ref new String(L"WebPGetFeatures failed"));
		}

		WriteableBitmap^ bitmap = ref new WriteableBitmap(iter.width, iter.height);

		unsigned int length;
		uint8_t* pixels = WebPBitmapFrame::GetPointerToPixelData(bitmap->PixelBuffer, &length);

		config.options.no_fancy_upsampling = 1;
		config.output.colorspace = MODE_bgrA;
		config.output.is_external_memory = 1;
		config.output.u.RGBA.rgba = pixels;
		config.output.u.RGBA.stride = iter.width * 4;
		config.output.u.RGBA.size = (iter.width * 4) * iter.height;

		ret = WebPDecode(iter.fragment.bytes, iter.fragment.size, &config);

		if (ret != VP8_STATUS_OK)
		{
			throw ref new FailureException(ref new String(L"Failed to decode frame"));
		}

		return bitmap;
	}

	return nullptr;
}

WebPImage^ WebPImage::CreateFromByteArray(const Array<uint8> ^bytes)
{
	auto vBuffer = std::vector<uint8_t>(bytes->Length);
	std::copy(bytes->begin(), bytes->end(), vBuffer.begin());
	return CreateFromByteArray(vBuffer);
}

WriteableBitmap^ WebPImage::DecodeFromByteArray(const Array<uint8> ^bytes)
{
	auto vBuffer = std::vector<uint8_t>(bytes->Length);
	std::copy(bytes->begin(), bytes->end(), vBuffer.begin());
	return DecodeFromByteArray(vBuffer);
}
