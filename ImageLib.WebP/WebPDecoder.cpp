#include "pch.h"
#include "WebPDecoder.h"
#include "WebPImage.h"
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;
using namespace ImageLib::WebP;
using namespace Platform;
using namespace ImageLib::Support;
using namespace concurrency;
using namespace Microsoft::WRL::Details;
using namespace Windows::UI::Core;
using namespace Platform::Collections;

WebPDecoder::WebPDecoder()
{
}

int ImageLib::WebP::WebPDecoder::GetPriority(Windows::Storage::Streams::IBuffer ^headerBuffer)
{
	return  ImageFormat::IsWebP(headerBuffer) ? 1 : -1;
}

void ImageLib::WebP::WebPDecoder::Start()
{
	if (_isInitialized && !_isAnimating)
	{
		_currentFrameIndex = 0;
		_completedLoops = 0;
		if (_animationTimer != nullptr) {
			_animationTimer->Stop();
		}
		_animationTimer = ref new DispatcherTimer();
		_animationTimer->Tick += ref new Windows::Foundation::EventHandler<Platform::Object ^>(this, &ImageLib::WebP::WebPDecoder::OnTick);
		_animationTimer->Interval = TimeSpan{ 0 };
		_animationTimer->Start();
		_isAnimating = true;
	}
}


void ImageLib::WebP::WebPDecoder::Stop()
{
	if (!_isAnimating)
	{
		return;
	}
	if (_animationTimer != nullptr) {
		_animationTimer->Stop();
	}
	_isAnimating = false;
}

void ImageLib::WebP::WebPDecoder::OnTick(Platform::Object ^sender, Platform::Object ^args)
{
	auto webPImage = _webPImage;
	if (!_isInitialized || webPImage == nullptr)
	{
		return;
	}

	// Increment frame index and loop count
	_currentFrameIndex++;
	if (_currentFrameIndex >= webPImage->Frames->Length)
	{
		_completedLoops++;
		_currentFrameIndex = 0;
	}
	auto frameIndex = _currentFrameIndex;
	auto frame = this->_webPImage->Frames->get(frameIndex);
	// Set up the timer to display the next frame
	if (this->_webPImage->LoopCount == 0 || _completedLoops < this->_webPImage->LoopCount)
	{
		/*	TimeSpan span;
			span.Duration =*/
		_animationTimer->Interval = TimeSpan{ frame->Duration * 10000 };
	}
	else
	{
		_animationTimer->Stop();
	}

	_image->Source = frame->RenderFrame();

}

Windows::UI::Xaml::Media::ImageSource ^ ImageLib::WebP::WebPDecoder::RecreateSurfaces()
{
	return nullptr;
}

Windows::Foundation::IAsyncOperation<ImageLib::Support::ImagePackage ^> ^
ImageLib::WebP::WebPDecoder::InitializeAsync(Windows::UI::Core::CoreDispatcher ^dispatcher,
	Windows::UI::Xaml::Controls::Image ^image,
	Windows::Foundation::Uri ^uriSource,
	Windows::Storage::Streams::IRandomAccessStream ^streamSource)
{
	_image = image;
	return create_async([this, dispatcher, image, uriSource, streamSource]()
	{
		IAsyncOperation<ImageLib::Support::ImagePackage ^> ^  op;
		auto buffer = ref new Buffer(streamSource->Size);
		Uri^ uri = nullptr;
		if (image->Tag != nullptr) {
			uri = dynamic_cast<Uri^>(image->Tag);
		}
		ImagePackage^ result = nullptr;
		auto imagePackage = create_task(streamSource->ReadAsync(buffer, streamSource->Size, InputStreamOptions::None))
			.then([this, dispatcher, uri, uriSource, image](IBuffer^ buffer)
		{
			ImagePackage^ package = nullptr;
			WebPBitstreamFeatures features = WebPBitstreamFeatures();
			auto dataReader = DataReader::FromBuffer(buffer);
			std::vector<uint8_t> vBuffer;
			vBuffer.resize(buffer->Length);
			dataReader->ReadBytes(ArrayReference<uint8_t>(vBuffer.data(), vBuffer.size()));
			VP8StatusCode statusCode = WebPGetFeatures(vBuffer.data(), vBuffer.size(), &features);
			if (statusCode == VP8_STATUS_OK) {
				WriteableBitmap^ writeableBitmap = nullptr;
				if (features.has_animation == 1) {
					auto webPImage = WebPImage::CreateFromByteArray(vBuffer);
					_webPImage = webPImage;
					if (webPImage->Frames->Length > 0) {
						auto first = webPImage->Frames->get(0);
						writeableBitmap = first->RenderFrame();
						package = ref new ImagePackage(this, writeableBitmap, writeableBitmap->PixelWidth, writeableBitmap->PixelHeight);
					}
				}
				else
				{
					/*	writeableBitmap = ref new WriteableBitmap(features.width, features.height);
						byte* pixels = WebPDecodeBGRA(vBuffer.data(), vBuffer.size(), &features.width, &features.height);
						IBuffer^ buffer = writeableBitmap->PixelBuffer;
						ComPtr<IBufferByteAccess> pBufferByteAccess;
						ComPtr<IUnknown> pBuffer((IUnknown*)buffer);
						pBuffer.As(&pBufferByteAccess);
						byte *sourcePixels = nullptr;
						pBufferByteAccess->Buffer(&sourcePixels);
						memcpy(sourcePixels, (void *)pixels, features.width * features.height * 4);
						writeableBitmap->Invalidate();
						delete pixels;
						pixels = nullptr;*/
					writeableBitmap = WebPImage::DecodeFromByteArray(vBuffer);
					package = ref new ImagePackage(this, writeableBitmap, writeableBitmap->PixelWidth, writeableBitmap->PixelHeight);
				}
				dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([this, package, uri, uriSource, image, writeableBitmap]() {
					if (uri->AbsoluteUri == uriSource->AbsoluteUri)
					{
						image->Source = writeableBitmap;
					}
				}));
			}
			return package;

		});
		return concurrency::create_async([imagePackage, this]() -> concurrency::task<ImagePackage^>
		{
			_isInitialized = true;//初始化成功
			return imagePackage;
		});
		/*auto dataReader = DataReader::FromBuffer(buffer);
		auto str = dataReader->ReadString(buffer->Length);
		return std::wstring(str->Begin(), str->End());*/
	});
}








