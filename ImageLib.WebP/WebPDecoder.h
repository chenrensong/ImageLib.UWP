#pragma once
#include "WebPImage.h"
using namespace ImageLib::Support;
using namespace Windows::UI::Xaml;
namespace ImageLib
{
	namespace WebP
	{
		[Windows::Foundation::Metadata::WebHostHidden]
		public ref class WebPDecoder sealed : IImageDecoder
		{
		private:
			ImageLib::WebP::WebPImage^ _webPImage = nullptr;
			Windows::UI::Xaml::Controls::Image ^ _image = nullptr;
			bool _isAnimating = false;
			bool _isInitialized = false;
			int _currentFrameIndex = 0;
			int _completedLoops = 0;
			int _headerSize = 12;

			//WriteableBitmap^ _writeableBitmap = nullptr;
			Windows::UI::Xaml::DispatcherTimer^ _animationTimer = nullptr;
		public:
			WebPDecoder();
			virtual	property int HeaderSize
			{
				int get() { return _headerSize; }
			}

			virtual int GetPriority(Windows::Storage::Streams::IBuffer ^headerBuffer);

			virtual void Start();

			virtual void Stop();

			virtual Windows::UI::Xaml::Media::ImageSource ^ RecreateSurfaces();

			virtual Windows::Foundation::IAsyncOperation<ImageLib::Support::ImagePackage ^> ^ InitializeAsync(Windows::UI::Core::CoreDispatcher ^dispatcher, Windows::UI::Xaml::Controls::Image ^image, Windows::Foundation::Uri ^uriSource, Windows::Storage::Streams::IRandomAccessStream ^streamSource);



			void OnTick(Platform::Object ^sender, Platform::Object ^args);
		};

	}
}
