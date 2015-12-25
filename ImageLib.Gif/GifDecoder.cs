// ===============================================================================
// GifDecoder.cs
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================

using ImageLib.IO;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;


namespace ImageLib.Gif
{
    public sealed class GifDecoder : IImageDecoder
    {
        /// <summary>
        /// Gif头为6
        /// </summary>
        public int HeaderSize
        {
            get
            {
                return 6;
            }
        }

        public bool IsSupportedFileFormat(byte[] header)
        {
            return header != null && header.Length >= 6 &&
                   header[0] == 0x47 && // G
                   header[1] == 0x49 && // I
                   header[2] == 0x46 && // F
                   header[3] == 0x38 && // 8
                   (header[4] == 0x39 || header[4] == 0x37) && // 9 or 7
                   header[5] == 0x61;   // a
        }

        #region Private struct declarations
        private struct ImageProperties
        {
            public readonly int PixelWidth;
            public readonly int PixelHeight;
            public readonly bool IsAnimated;
            public readonly int LoopCount;

            public ImageProperties(int pixelWidth, int pixelHeight, bool isAnimated, int loopCount)
            {
                PixelWidth = pixelWidth;
                PixelHeight = pixelHeight;
                IsAnimated = isAnimated;
                LoopCount = loopCount;
            }
        }

        private class FrameProperties
        {
            private bool _isDecode = false;
            public readonly Rect Rect;
            public readonly double DelayMilliseconds;
            public readonly bool ShouldDispose;
            public readonly uint FrameIndex;
            public byte[] Pixels { get; private set; }

            public FrameProperties(uint frameIndex, Rect rect, double delayMilliseconds, bool shouldDispose)
            {
                FrameIndex = frameIndex;
                Rect = rect;
                DelayMilliseconds = delayMilliseconds;
                ShouldDispose = shouldDispose;
            }

            public async Task<byte[]> DecodeAsync(BitmapDecoder bitmapDecoder)
            {
                if (_isDecode)
                {
                    return this.Pixels;
                }
                var frame = await bitmapDecoder.GetFrameAsync(FrameIndex).AsTask().ConfigureAwait(false);
                var pixelData = await frame.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    new BitmapTransform(),
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                    ).AsTask().ConfigureAwait(false);
                this.Pixels = pixelData.DetachPixelData();
                _isDecode = true;
                return Pixels;
            }

            public void ReleasePixels()
            {
                this.Pixels = null;
            }


        }

        #endregion

        private DispatcherTimer _animationTimer;

        private int _currentFrameIndex;
        private int _completedLoops;
        private bool _disposeRequested;

        private BitmapDecoder _bitmapDecoder;
        private ImageProperties _imageProperties;
        private IList<FrameProperties> _frameProperties;

        private CanvasImageSource _canvasImageSource;
        private CanvasRenderTarget _accumulationRenderTarget;

        private bool _isInitialized;
        private bool _isAnimating;
        private bool _hasCanvasResources;
        private CoreDispatcher _dispatcher;


        public async Task<ImageSource> InitializeAsync(CoreDispatcher dispatcher,
            IRandomAccessStream streamSource, CancellationTokenSource cancellationTokenSource)
        {

            var inMemoryStream = new InMemoryRandomAccessStream();
            var copyAction = RandomAccessStream.CopyAndCloseAsync(
                           streamSource.GetInputStreamAt(0L),
                           inMemoryStream.GetOutputStreamAt(0L));
            await copyAction.AsTask(cancellationTokenSource.Token);

            var bitmapDecoder = await BitmapDecoder.
                CreateAsync(BitmapDecoder.GifDecoderId, inMemoryStream).AsTask(cancellationTokenSource.Token).ConfigureAwait(false);
            var imageProperties = await RetrieveImagePropertiesAsync(bitmapDecoder);
            var frameProperties = new List<FrameProperties>();
            for (var i = 0u; i < bitmapDecoder.FrameCount; i++)
            {
                var bitmapFrame = await bitmapDecoder.GetFrameAsync(i).AsTask(cancellationTokenSource.Token).ConfigureAwait(false); ;
                frameProperties.Add(await RetrieveFramePropertiesAsync(i, bitmapFrame));
            }

            _frameProperties = frameProperties;
            _bitmapDecoder = bitmapDecoder;
            _imageProperties = imageProperties;
            _dispatcher = dispatcher;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 CreateCanvasResources();
             });

            _isInitialized = true;
            return _canvasImageSource;
        }

        public void Start()
        {
            if (_isInitialized && !_isAnimating)
            {
                _currentFrameIndex = 0;
                _completedLoops = 0;
                _disposeRequested = false;

                _animationTimer?.Stop();

                _animationTimer = new DispatcherTimer();
                _animationTimer.Tick += AnimationTimer_Tick;
                _animationTimer.Interval = TimeSpan.Zero;
                _animationTimer.Start();

                _isAnimating = true;
            }
        }

        private async void AnimationTimer_Tick(object sender, object e)
        {
            try
            {
                await AdvanceFrame();
            }
            catch (Exception ex)
            {

            }
        }

        public void Stop()
        {
            _animationTimer?.Stop();
            _isAnimating = false;
        }

        private async Task<byte[]> DecodePixelAsync(uint frameIndex)
        {
            var frame = await _bitmapDecoder.GetFrameAsync(frameIndex).AsTask().ConfigureAwait(false);
            var pixelData = await frame.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
                ).AsTask().ConfigureAwait(false);
            var pixels = pixelData.DetachPixelData();
            return pixels;
        }

        private async Task AdvanceFrame()
        {

            if (!_isInitialized || _bitmapDecoder.FrameCount == 0)
            {
                return;
            }


            var frameIndex = _currentFrameIndex;
            var frameProperties = _frameProperties[frameIndex];
            var disposeRequested = _disposeRequested;


            // Increment frame index and loop count
            _currentFrameIndex++;
            if (_currentFrameIndex >= _bitmapDecoder.FrameCount)
            {
                _completedLoops++;
                _currentFrameIndex = 0;
            }

            // Set flag to clear before next frame if necessary
            _disposeRequested = frameProperties.ShouldDispose;

            // Set up the timer to display the next frame
            if (_imageProperties.IsAnimated &&
                (_imageProperties.LoopCount == 0 || _completedLoops < _imageProperties.LoopCount))
            {
                _animationTimer.Interval = TimeSpan.FromMilliseconds(frameProperties.DelayMilliseconds);
            }
            else
            {
                _animationTimer.Stop();
            }

            // Decode the frame
            //var pixels = await this.DecodePixelAsync((uint)frameIndex);
            var pixels = await frameProperties.DecodeAsync(_bitmapDecoder);
            var frameRectangle = frameProperties.Rect;
            var shouldClear = disposeRequested || frameIndex == 0;

            // Compose and display the frame
            try
            {
                PrepareFrame(pixels, frameRectangle, shouldClear);
                UpdateImageSource(frameRectangle);
            }
            catch (Exception e) when (_canvasImageSource.Device.IsDeviceLost(e.HResult))
            {
                // XAML will also raise a SurfaceContentsLost event, and we use this to trigger
                // redrawing the surface. Therefore, ignore this error.
            }
        }

        private void CreateCanvasResources()
        {
            if (_hasCanvasResources)
            {
                return;
            }

            const float desiredDpi = 96.0f; // GIF does not encode DPI

            var sharedDevice = GetSharedDevice();
            var pixelWidth = _imageProperties.PixelWidth;
            var pixelHeight = _imageProperties.PixelHeight;

            _canvasImageSource = new CanvasImageSource(
                sharedDevice,
                pixelWidth,
                pixelHeight,
                desiredDpi,
                CanvasAlphaMode.Premultiplied
                );
            _accumulationRenderTarget = new CanvasRenderTarget(
                sharedDevice,
                pixelWidth,
                pixelHeight,
                desiredDpi,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                CanvasAlphaMode.Premultiplied
                );

            _hasCanvasResources = true;
        }

        public ImageSource RecreateSurfaces()
        {
            if (_hasCanvasResources)
            {
                var sharedDevice = GetSharedDevice();

                _canvasImageSource = new CanvasImageSource(
                    sharedDevice,
                    _imageProperties.PixelWidth,
                    _imageProperties.PixelHeight,
                    _canvasImageSource.Dpi,
                    _canvasImageSource.AlphaMode
                    );

                _accumulationRenderTarget = new CanvasRenderTarget(
                    sharedDevice,
                    _imageProperties.PixelWidth,
                    _imageProperties.PixelHeight,
                    _accumulationRenderTarget.Dpi,
                    _accumulationRenderTarget.Format,
                    _accumulationRenderTarget.AlphaMode
                    );

                if (_isAnimating)
                {
                    Start(); // Reset animation as the accumulation buffer is invalid
                }

                return _canvasImageSource;
            }
            else
            {
                return null;
            }
        }

        private void PrepareFrame(byte[] pixels, Rect frameRectangle, bool clearAccumulation)
        {
            var sharedDevice = GetSharedDevice();

            // Create hardware bitmap from decoded frame
            var frameBitmap = CanvasBitmap.CreateFromBytes(
                sharedDevice,
                pixels,
                (int)frameRectangle.Width,
                (int)frameRectangle.Height,
                DirectXPixelFormat.B8G8R8A8UIntNormalized
                );

            // Draw bitmap to render target, potentially on top of the previous frame
            using (var drawingSession = _accumulationRenderTarget.CreateDrawingSession())
            {
                using (frameBitmap)
                {
                    if (clearAccumulation)
                    {
                        drawingSession.Clear(Colors.Transparent);
                    }
                    drawingSession.DrawImage(frameBitmap, frameRectangle);
                }
            }

        }

        private void UpdateImageSource(Rect updateRectangle)
        {
            if (Window.Current.Visible)
            {
                var imageRectangle = new Rect(new Point(), _canvasImageSource.Size);

                updateRectangle.Intersect(imageRectangle);

                if (updateRectangle.IsEmpty)
                {
                    updateRectangle = imageRectangle;
                }

                using (var drawingSession = _canvasImageSource.CreateDrawingSession(Colors.Transparent, updateRectangle))
                {
                    drawingSession.DrawImage(_accumulationRenderTarget); // Render target has the composed frame
                }

            }
        }

        private CanvasDevice GetSharedDevice()
        {
            return CanvasDevice.GetSharedDevice(forceSoftwareRenderer: false);
        }

        #region GIF file format helpers

        private static async Task<ImageProperties> RetrieveImagePropertiesAsync(BitmapDecoder bitmapDecoder)
        {
            // Properties not currently supported: background color, pixel aspect ratio.
            const string widthProperty = "/logscrdesc/Width";
            const string heightProperty = "/logscrdesc/Height";
            const string applicationProperty = "/appext/application";
            const string dataProperty = "/appext/data";

            var propertiesView = bitmapDecoder.BitmapContainerProperties;
            var requiredProperties = new[] { widthProperty, heightProperty };
            var properties = await propertiesView.GetPropertiesAsync(requiredProperties).AsTask().ConfigureAwait(false);

            var pixelWidth = (ushort)properties[widthProperty].Value;
            var pixelHeight = (ushort)properties[heightProperty].Value;

            var loopCount = 0; // Repeat forever by default
            var isAnimated = true;

            try
            {
                var extensionProperties = new[] { applicationProperty, dataProperty };
                properties = await propertiesView.GetPropertiesAsync(extensionProperties);

                if (properties.ContainsKey(applicationProperty) &&
                    properties[applicationProperty].Type == PropertyType.UInt8Array)
                {
                    var bytes = (byte[])properties[applicationProperty].Value;
                    var applicationName = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    if (applicationName == "NETSCAPE2.0" || applicationName == "ANIMEXTS1.0")
                    {
                        if (properties.ContainsKey(dataProperty) && properties[dataProperty].Type == PropertyType.UInt8Array)
                        {
                            //  The data is in the following format: 
                            //  byte 0: extsize (must be > 1) 
                            //  byte 1: loopType (1 == animated gif) 
                            //  byte 2: loop count (least significant byte) 
                            //  byte 3: loop count (most significant byte) 
                            //  byte 4: set to zero 

                            var data = (byte[])properties[dataProperty].Value;
                            loopCount = data[2] | data[3] << 8;
                            isAnimated = data[1] == 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // These properties are not required, so it's okay to ignore failure.
            }

            return new ImageProperties(pixelWidth, pixelHeight, isAnimated, loopCount);
        }

        private async Task<FrameProperties> RetrieveFramePropertiesAsync(uint index, BitmapFrame frame)
        {
            const string leftProperty = "/imgdesc/Left";
            const string topProperty = "/imgdesc/Top";
            const string widthProperty = "/imgdesc/Width";
            const string heightProperty = "/imgdesc/Height";
            const string delayProperty = "/grctlext/Delay";
            const string disposalProperty = "/grctlext/Disposal";

            var propertiesView = frame.BitmapProperties;
            var requiredProperties = new[] { leftProperty, topProperty, widthProperty, heightProperty };
            var properties = await propertiesView.GetPropertiesAsync(requiredProperties).AsTask().ConfigureAwait(false); ;

            var left = (ushort)properties[leftProperty].Value;
            var top = (ushort)properties[topProperty].Value;
            var width = (ushort)properties[widthProperty].Value;
            var height = (ushort)properties[heightProperty].Value;

            var delayMilliseconds = 30.0;
            var shouldDispose = false;

            try
            {
                var extensionProperties = new[] { delayProperty, disposalProperty };
                properties = await propertiesView.GetPropertiesAsync(extensionProperties).AsTask().ConfigureAwait(false); ;

                if (properties.ContainsKey(delayProperty))
                {
                    if (properties[delayProperty].Type == PropertyType.UInt16)
                    {
                        var delayInHundredths = (ushort)properties[delayProperty].Value;
                        if (delayInHundredths >= 3u) // Prevent degenerate frames with no delay time
                        {
                            delayMilliseconds = delayInHundredths * 10.0d;
                        }
                        if (delayInHundredths == 0u) // Prevent degenerate frames with no delay time
                        {
                            delayMilliseconds = 100.0d;
                        }
                    }
                }

                if (properties.ContainsKey(disposalProperty) && properties[disposalProperty].Type == PropertyType.UInt8)
                {
                    var disposal = (byte)properties[disposalProperty].Value;
                    if (disposal == 2)
                    {
                        // 0 = undefined 不使用处置方法
                        // 1 = none (compose next frame on top of this one, default)  不处置图形，把图形从当前位置移去
                        // 2 = dispose 回复到背景色
                        // 3 = revert to previous (not supported) 回复到先前状态
                        shouldDispose = true;
                    }
                }
            }
            catch
            {
                // These properties are not required, so it's okay to ignore failure.
            }

            return new FrameProperties(index,
                new Rect(left, top, width, height),
                delayMilliseconds,
                shouldDispose
                );
        }

        ~GifDecoder()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();
                _isInitialized = false;
                _hasCanvasResources = false;
                _bitmapDecoder = null;
                if (_frameProperties != null)
                {
                    foreach (var item in _frameProperties)
                    {
                        item.ReleasePixels();
                    }
                    _frameProperties.Clear();
                    _frameProperties = null;
                }
                _canvasImageSource = null;
                _accumulationRenderTarget = null;
                _animationTimer = null;
            }
        }


        #endregion

    }
}
