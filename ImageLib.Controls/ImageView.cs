using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ImageLib.Controls
{
    public sealed class ImageView : Control
    {

        #region Private Fields

        private static readonly DependencyProperty UriSourceProperty = DependencyProperty.Register
        (
          "UriSource", typeof(Uri), typeof(ImageView), new PropertyMetadata(null, OnUriSourcePropertyChanged)
        );

        private static readonly DependencyProperty StretchProperty = DependencyProperty.Register
        (
            "Stretch", typeof(Stretch), typeof(ImageView), new PropertyMetadata(Stretch.None)
        );

        private static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register
        (
          "IsLoading", typeof(bool), typeof(ImageView), new PropertyMetadata(false)
        );

        private static readonly DependencyProperty IsAutoPlayProperty = DependencyProperty.Register
        (
          "IsAutoPlay", typeof(bool), typeof(ImageView), new PropertyMetadata(true)
        );


        private Image _image;

        /// <summary>
        /// 当前动画状态
        /// </summary>
        private AnimationState State
        {
            get
            {
                return _animation.State;
            }
        }

        /// <summary>
        /// 动画
        /// </summary>
        private volatile ImageAnimation _animation = new ImageAnimation();

        /// <summary>
        /// Presenter
        /// </summary>
        private volatile ImagePresenter _imagePresenter = new ImagePresenter();


        /// <summary>
        /// 像素高度
        /// </summary>
        public int PixelHeight
        {
            get
            {
                return _imagePresenter.PixelHeight;
            }
        }

        /// <summary>
        /// 像素宽度
        /// </summary>
        public int PixelWidth
        {
            get
            {
                return _imagePresenter.PixelWidth;
            }
        }


        #endregion


        public ImageView()
        {
            this.DefaultStyleKey = typeof(ImageView);
            this.Loaded += ((s, e) =>
            {
                // 注册事件（VisibilityChanged），当最小化的时候停止动画。
                Window.Current.VisibilityChanged += OnVisibilityChanged;
                _animation?.Begin();
            });
            this.Unloaded -= ((s, e) =>
            {
                // 解注册事件
                Window.Current.VisibilityChanged -= OnVisibilityChanged;
                _animation?.Stop();
            });

        }

        private void OnVisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            if (e.Visible)
            {
                _animation?.Begin();
            }
            else if (!e.Visible)
            {
                _animation?.Stop();
            }
        }

        protected override void OnDisconnectVisualChildren()
        {
            base.OnDisconnectVisualChildren();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._image = GetTemplateChild("Image") as Image;
            this.UpdateUriSourceAsync(this.UriSource);
        }


        #region Public Properties

        /// <summary>
        /// 是否正在Loading
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return (bool)base.GetValue(IsLoadingProperty);
            }
            private set
            {
                base.SetValue(IsLoadingProperty, value);
            }
        }

        public Stretch Stretch
        {
            get
            {
                return (Stretch)base.GetValue(StretchProperty);
            }
            set
            {
                base.SetValue(StretchProperty, value);
            }
        }

        public Uri UriSource
        {
            get
            {
                return (Uri)GetValue(UriSourceProperty);
            }
            set
            {
                SetValue(UriSourceProperty, value);
            }
        }

        /// <summary>
        /// 当载入完成自动播放
        /// </summary>
        public bool IsAutoPlay
        {
            get
            {
                return (bool)GetValue(IsAutoPlayProperty);
            }
            set
            {
                SetValue(IsAutoPlayProperty, value);
            }
        }
        #endregion


        #region Private Methods



        public async Task ExecuteOnUI(Action action)
        {
            if (Dispatcher.HasThreadAccess)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => action());
            }
            else
            {
                action();
            }
        }


        private async void UpdateUriSourceAsync(Uri uriSource)
        {
            var t = DateTime.Now.Ticks;
            //  Yeah, I know this is kinda' "cowboyish" - but hey, I don't want it to fail in the designer!
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                return;
            }
            //OnApplyTemplate method don't execute
            if (_image == null)
            {
                return;
            }
            try
            {
                _animation?.Clear();
                _imagePresenter?.Clear();
                _image.Source = null;

                IsLoading = true;
                //开始载入
                if (LoadingStarted != null)
                {
                    LoadingStarted(this, EventArgs.Empty);
                }

                if (uriSource == null)
                {
                    if (LoadingFailed != null)
                    {
                        LoadingFailed(this, new NullReferenceException("UriSource shouldn't be null"));
                    }
                    return;
                }

                await _imagePresenter.BuildAsync(uriSource, _image, _animation, IsAutoPlay);

                if (LoadingCompleted != null)
                {
                    LoadingCompletedEventArgs LoadingCompletedEventArgs = new LoadingCompletedEventArgs(PixelWidth, PixelHeight);
                    LoadingCompleted(this, LoadingCompletedEventArgs);
                }
            }
            catch (Exception ex)
            {
                if (LoadingFailed != null)
                {
                    LoadingFailed(this, ex);
                }
            }
            finally
            {
                IsLoading = false;
            }
            System.Diagnostics.Debug.WriteLine("ImageView" + (DateTime.Now.Ticks - t));

        }


        private static void OnUriSourcePropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var that = sender as ImageView;
            that?.UpdateUriSourceAsync(args.NewValue as Uri);
        }
        #endregion

        #region Public Events
        public event EventHandler LoadingStarted;
        public event EventHandler<LoadingCompletedEventArgs> LoadingCompleted;
        public event EventHandler<Exception> LoadingFailed;
        #endregion


    }
}
