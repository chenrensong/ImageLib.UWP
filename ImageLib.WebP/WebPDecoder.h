#pragma once
using namespace ImageLib::Support;
namespace ImageLib
{
	namespace WebP
	{
		[Windows::Foundation::Metadata::WebHostHidden]
		public ref class WebPDecoder sealed : IImageDecoder
		{
		private:
			int headerSize = 12;
		public:
			WebPDecoder();
			virtual	property int HeaderSize
			{
				int get() { return headerSize; }
			}

			virtual int GetPriority(Windows::Storage::Streams::IBuffer ^headerBuffer);

			virtual void Start();

			virtual void Stop();

			virtual Windows::UI::Xaml::Media::ImageSource ^ RecreateSurfaces();

			virtual Windows::Foundation::IAsyncOperation<ImageLib::Support::ImagePackage ^> ^ InitializeAsync(Windows::UI::Core::CoreDispatcher ^dispatcher, Windows::UI::Xaml::Controls::Image ^image, Windows::Foundation::Uri ^uriSource, Windows::Storage::Streams::IRandomAccessStream ^streamSource);



		};

	}
}
