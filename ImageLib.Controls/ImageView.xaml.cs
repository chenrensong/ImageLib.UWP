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
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;
using ImageLib.IO;
using ImageLib.Helpers;

namespace ImageLib.Controls
{
    public sealed partial class ImageView : UserControl
    {
        #region Public Events
        public event EventHandler LoadingStarted;
        public event EventHandler LoadingCompleted;
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

        private IImageDecoder _imageDecoder;
        private bool _isLoaded;
        private CancellationTokenSource _initializationCancellationTokenSource;

        public ImageView()
        {
            this.InitializeComponent();

            this.Loaded += ((s, e) =>
            {
                // 注册事件（VisibilityChanged），当最小化的时候停止动画。
                Window.Current.VisibilityChanged += OnVisibilityChanged;
                // Register for SurfaceContentsLost to recreate the image source if necessary
                CompositionTarget.SurfaceContentsLost += OnSurfaceContentsLost;
            });
            this.Unloaded -= ((s, e) =>
            {
                // 解注册事件
                Window.Current.VisibilityChanged += OnVisibilityChanged;
                CompositionTarget.SurfaceContentsLost -= OnSurfaceContentsLost;

            });

        }

        private async static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = d as ImageView;
            await that?.UpdateSourceAsync();
        }
        private static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long CurrentTimeMillis(DateTime d)
        {
            return (long)((DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
        }
        private async Task UpdateSourceAsync()
        {

            _imageDecoder?.Stop();
            _initializationCancellationTokenSource?.Cancel();

            _image.Source = null;
            _imageDecoder = null;

            if (UriSource != null)
            {
                var uriSource = UriSource;
                var cancellationTokenSource = new CancellationTokenSource();

                _initializationCancellationTokenSource = cancellationTokenSource;

                try
                {
                    this.OnLoadingStarted();
                    var readStream = await uriSource.GetStreamFromUri(cancellationTokenSource.Token);
                    if (!uriSource.Equals(UriSource))
                    {
                        return;
                    }
                    ImageSource imageSource = null;
                    bool hasDecoder = false;
                    var decoders = Decoders.GetAvailableDecoders();
                    if (decoders.Count > 0)
                    {
                        int maxHeaderSize = decoders.Max(x => x.HeaderSize);
                        if (maxHeaderSize > 0)
                        {
                            byte[] header = new byte[maxHeaderSize];
                            await readStream.AsStreamForRead().ReadAsync(header, 0, maxHeaderSize);
                            var decoder = decoders.FirstOrDefault(x => x.IsSupportedFileFormat(header));
                            if (decoder != null)
                            {
                                imageSource = await decoder.InitializeAsync(readStream);
                                _imageDecoder = decoder;
                                if (_isLoaded)
                                {
                                    _imageDecoder.Start();
                                }
                                hasDecoder = true;
                            }
                        }
                    }
                    if (!hasDecoder)
                    {
                        var bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(readStream).AsTask(_initializationCancellationTokenSource.Token);
                        imageSource = bitmapImage;
                    }


                    _image.Source = imageSource;

                    this.OnLoadingCompleted();

                }
                catch (TaskCanceledException)
                {
                    // Task Canceled 需要设置Souce=null.
                    _image.Source = null;
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


        private void OnLoadingStarted()
        {
            this.IsLoading = true;
            if (LoadingStarted != null)
            {
                LoadingStarted.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnLoadingCompleted()
        {
            this.IsLoading = false;
            if (LoadingCompleted != null)
            {
                LoadingCompleted.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnFail(Exception ex)
        {
            this.IsLoading = false;
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
            _isLoaded = true;
            _imageDecoder?.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = false;
            _imageDecoder?.Stop();
        }

        private void OnSurfaceContentsLost(object sender, object e)
        {
            _image.Source = _imageDecoder?.RecreateSurfaces();
        }

        #endregion


    }
}
