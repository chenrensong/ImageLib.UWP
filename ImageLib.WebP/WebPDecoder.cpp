#include "pch.h"
#include "WebPDecoder.h"
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

}

void ImageLib::WebP::WebPDecoder::Stop()
{

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
			std::vector<uint8_t> bytes;
			bytes.resize(buffer->Length);
			dataReader->ReadBytes(ArrayReference<uint8_t>(bytes.data(), bytes.size()));
			VP8StatusCode statusCode = WebPGetFeatures(bytes.data(), bytes.size(), &features);
			if (statusCode == VP8_STATUS_OK) {
				WriteableBitmap^ writeableBitmap = ref new   WriteableBitmap(features.width, features.height);
				dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([this, package, uri, uriSource, image, writeableBitmap]() {
					if (uri->AbsoluteUri == uriSource->AbsoluteUri)
					{
						image->Source = writeableBitmap;
					}
				}));
				byte* pixels = WebPDecodeBGRA(bytes.data(), bytes.size(), &features.width, &features.height);
				IBuffer^ buffer = writeableBitmap->PixelBuffer;
				ComPtr<IBufferByteAccess> pBufferByteAccess;
				ComPtr<IUnknown> pBuffer((IUnknown*)buffer);
				pBuffer.As(&pBufferByteAccess);
				byte *sourcePixels = nullptr;
				pBufferByteAccess->Buffer(&sourcePixels);
				memcpy(sourcePixels, (void *)pixels, features.width * features.height * 4);
				writeableBitmap->Invalidate();
				delete pixels;
				pixels = nullptr;
				package = ref new ImagePackage(this, writeableBitmap, writeableBitmap->PixelWidth, writeableBitmap->PixelHeight);
			}

			return package;

		});
		return concurrency::create_async([imagePackage]() -> concurrency::task<ImagePackage^>
		{
			return imagePackage;
		});
		/*auto dataReader = DataReader::FromBuffer(buffer);
		auto str = dataReader->ReadString(buffer->Length);
		return std::wstring(str->Begin(), str->End());*/
	});

	//IAsyncOperation<ImageLib::Support::ImagePackage ^> ^  op = create_async([dispatcher, image,uriSource, streamSource]()
	//{
	//	streamSource->ReadAsync()
	//	ImagePackage^ package = nullptr;
	//	return package;
	//});
	//return op;
}





