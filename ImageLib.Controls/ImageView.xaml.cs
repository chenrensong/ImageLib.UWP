// ===============================================================================
// ImageView.cs
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;
using ImageLib.IO;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;

namespace ImageLib.Controls
{
    public sealed partial class ImageView : UserControl
    {

        #region Public Events
        /// <summary>
        /// 开始加载
        /// </summary>
        public event EventHandler LoadingStarted;
        /// <summary>
        /// 加载完成
        /// </summary>
        public event EventHandler<LoadingCompletedEventArgs> LoadingCompleted;
        /// <summary>
        /// 加载失败
        /// </summary>
        public event EventHandler<Exception> LoadingFailed;
        #endregion

        public static DependencyProperty StretchProperty { get; } = DependencyProperty.Register(
            nameof(Stretch),
            typeof(Stretch),
            typeof(ImageView),
            new PropertyMetadata(Stretch.None)
            );

        public static DependencyProperty UriSourceProperty { get; } = DependencyProperty.Register(
            nameof(UriSource),
            typeof(Uri),
            typeof(ImageView),
            new PropertyMetadata(null, new PropertyChangedCallback(OnSourcePropertyChanged))
            );

        private static DependencyProperty IsLoadingProperty { get; } = DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(ImageView),
            new PropertyMetadata(false)
            );

        private static DependencyProperty ImageLoaderKeyProperty { get; } = DependencyProperty.Register(
            nameof(ImageLoaderKey),
            typeof(string),
            typeof(ImageView),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// 用于定义当前的ImageLoader
        /// </summary>
        public string ImageLoaderKey
        {
            get { return GetValue(ImageLoaderKeyProperty) as string; }
            set { SetValue(ImageLoaderKeyProperty, value); }
        }


        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public Uri UriSource
        {
            get { return (Uri)GetValue(UriSourceProperty); }
            set { SetValue(UriSourceProperty, value); }
        }

        /// <summary>
        /// Pixel Width
        /// </summary>
        public double PixelWidth
        {
            get; private set;
        }

        /// <summary>
        /// Pixel Height
        /// </summary>
        public double PixelHeight
        {
            get; private set;
        }


        /// <summary>
        /// 获取当前的Image Loader
        /// </summary>
        private ImageLoader CurrentLoader
        {
            get
            {
                if (!string.IsNullOrEmpty(ImageLoaderKey))
                {
                    return ImageLoader.Collection[ImageLoaderKey];
                }
                return ImageLoader.Instance;
            }
        }

        private IImageDecoder _imageDecoder;
        private bool _isControlLoaded;
        private CancellationTokenSource _initializationCancellationTokenSource;

        public ImageView()
        {
            this.InitializeComponent();
        }

        private async static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = d as ImageView;
            await that?.UpdateSourceAsync();
        }

        private async Task UpdateSourceAsync()
        {
            _imageDecoder?.Dispose();
            _initializationCancellationTokenSource?.Cancel();
            _image.Source = null;
            this.PixelHeight = 0d;
            this.PixelWidth = 0d;
            Interlocked.Exchange(ref _imageDecoder, null);
            if (UriSource != null)
            {
                var uriSource = UriSource;
                var cancellationTokenSource = new CancellationTokenSource();
                _initializationCancellationTokenSource = cancellationTokenSource;
                try
                {
                    this.OnLoadingStarted();
                    var imageSource = await LoadImageByUri(uriSource, cancellationTokenSource);
                    if (uriSource.Equals(UriSource))
                    {
                        _image.Source = imageSource;
                    }
                    else
                    {
                        //不处理此情况
                    }
                    this.OnLoadingCompleted(imageSource);
                }
                catch (TaskCanceledException)
                {
                    // Task Canceled
                }
                catch (FileNotFoundException fnfex)
                {
                    this.OnFail(fnfex);
                }
                catch (Exception ex)
                {
                    this.OnFail(ex);
                }
            }
        }


        private async Task<ImageSource> LoadImageByUri(Uri uriSource, CancellationTokenSource cancellationTokenSource)
        {

            var randStream = await this.CurrentLoader.LoadImageStream(uriSource, cancellationTokenSource);
            if (randStream == null)
            {
                throw new Exception("stream is null");
            }
            ImageSource imageSource = null;
            bool hasDecoder = false;
            //debug模式不允许Decoders,直接采用默认方案
            if (!DesignMode.DesignModeEnabled)
            {
                var decoders = this.CurrentLoader.GetAvailableDecoders();
                if (decoders.Count > 0)
                {
                    int maxHeaderSize = decoders.Max(x => x.HeaderSize);
                    if (maxHeaderSize > 0)
                    {
                        byte[] header = new byte[maxHeaderSize];
                        var readStream = randStream.AsStreamForRead();
                        readStream.Position = 0;
                        await readStream.ReadAsync(header, 0, maxHeaderSize);
                        readStream.Position = 0;
                        var decoder = decoders.FirstOrDefault(x => x.IsSupportedFileFormat(header));
                        if (decoder != null)
                        {
                            var source = await decoder.InitializeAsync(this.Dispatcher, randStream,
                                    cancellationTokenSource);
                            imageSource = source.ImageSource;
                            this.PixelHeight = source.PixelHeight;
                            this.PixelWidth = source.PixelWidth;
                            if (!cancellationTokenSource.IsCancellationRequested)
                            {
                                _imageDecoder?.Dispose();
                                Interlocked.Exchange(ref _imageDecoder, decoder);
                                if (_isControlLoaded)
                                {
                                    _imageDecoder.Start();
                                }
                            }
                            hasDecoder = true;
                        }
                    }
                }
            }
            if (!hasDecoder)
            {
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(randStream).AsTask(cancellationTokenSource.Token);
                this.PixelHeight = bitmapImage.PixelHeight;
                this.PixelWidth = bitmapImage.PixelWidth;
                imageSource = bitmapImage;
            }
            return imageSource;

        }


        private void OnLoadingStarted()
        {
            this.IsLoading = true;
            if (LoadingStarted != null)
            {
                LoadingStarted.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnLoadingCompleted(ImageSource imageSource)
        {
            this.IsLoading = false;
            if (LoadingCompleted != null)
            {
                LoadingCompleted.Invoke(this, new LoadingCompletedEventArgs(this.PixelWidth, this.PixelWidth));
            }
        }

        private void OnFail(Exception ex)
        {
            this.IsLoading = false;
            _image.Source = null;
            if (LoadingFailed != null)
            {
                LoadingFailed.Invoke(this, ex);
            }
        }

        #region 控件生命周期

        private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible)
            {
                _imageDecoder?.Start();
            }
            else if (!e.Visible)
            {
                _imageDecoder?.Stop(); // Prevent unnecessary work
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 注册事件（VisibilityChanged），当最小化的时候停止动画。
            Window.Current.VisibilityChanged += OnVisibilityChanged;
            // Register for SurfaceContentsLost to recreate the image source if necessary
            CompositionTarget.SurfaceContentsLost += OnSurfaceContentsLost;
            _isControlLoaded = true;
            _imageDecoder?.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // 解注册事件
            Window.Current.VisibilityChanged -= OnVisibilityChanged;
            CompositionTarget.SurfaceContentsLost -= OnSurfaceContentsLost;
            _isControlLoaded = false;
            _initializationCancellationTokenSource?.Cancel();
            _image.Source = null;
            _imageDecoder?.Dispose();
        }

        private void OnSurfaceContentsLost(object sender, object e)
        {
            var source = _imageDecoder?.RecreateSurfaces();
            if (source != null)
            {
                _image.Source = source;
            }
        }

        public void Stop()
        {
            _imageDecoder?.Stop();
        }

        public void Start()
        {
            _imageDecoder?.Start();
        }

        #endregion


    }
}
