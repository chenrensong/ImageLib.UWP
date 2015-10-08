using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace ImageLib
{
    public class ImageAnimation
    {
        /// <summary>
        /// 当前动画状态
        /// </summary>
        private AnimationState _state = AnimationState.Stop;

        public AnimationState State
        {
            get
            {
                return _state;
            }
        }

        private volatile Storyboard _storyboard = new Storyboard()
        {
            RepeatBehavior = RepeatBehavior.Forever
        };

        internal void SetAnimation(Image targetImage, List<ImageFrame> frames)
        {
            this.Clear();

            if (frames == null && frames.Count == 0)
            {
                return;
            }

            //  Now create the animation as a set of ObjectAnimationUsingKeyFrames (I love this name!)
            var anim = new ObjectAnimationUsingKeyFrames();
            anim.BeginTime = TimeSpan.FromSeconds(0);

            var ts = new TimeSpan();

            // Create each DiscreteObjectKeyFrame and advance the KeyTime by 100 ms (=10 fps) and add it to 
            // the storyboard.
            foreach (var item in frames)
            {
                var keyFrame = new DiscreteObjectKeyFrame();
                keyFrame.KeyTime = KeyTime.FromTimeSpan(ts);
                keyFrame.Value = item.BitmapFrame;
                ts = ts.Add(item.Delay);
                anim.KeyFrames.Add(keyFrame);

                Debug.Write("width:" + item.BitmapFrame.PixelWidth +
                    "height:" + item.BitmapFrame.PixelHeight + "delay:" + item.Delay + "\n");
            }

            //  Connect the image control with the story board
            Storyboard.SetTarget(anim, targetImage);
            Storyboard.SetTargetProperty(anim, "Source");
            //targetImage.Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill;
            //  And finally add the animation-set to the storyboard
            _storyboard.Children.Add(anim);
        }


        public void Begin()
        {
            if (this._state != AnimationState.Playing)
            {
                if (_storyboard.Children.Count > 0)
                {
                    this._storyboard.Begin();
                    this._state = AnimationState.Playing;
                }
                else
                {
                    this._state = AnimationState.Stop;
                }
            }
        }

        public void Resume()
        {
            if (this._state != AnimationState.Playing)
            {
                if (_storyboard.Children.Count > 0)
                {
                    this._storyboard.Resume();
                    this._state = AnimationState.Playing;
                }
                else
                {
                    this._state = AnimationState.Stop;
                }
            }
        }

        public void Pause()
        {
            if (this._state != AnimationState.Pause)
            {
                if (_storyboard.Children.Count > 0)
                {
                    this._storyboard.Pause();
                    this._state = AnimationState.Pause;
                }
                else
                {
                    this._state = AnimationState.Stop;
                }
            }

        }

        public void Stop()
        {
            if (_storyboard.Children.Count > 0 && _state != AnimationState.Stop)
            {
                _storyboard.Stop();
            }
            _state = AnimationState.Stop;
        }

        /// <summary>
        /// 清空Animation
        /// </summary>
        public void Clear()
        {
            if (_state != AnimationState.Stop)
            {
                _storyboard.Stop();
            }
            //  Clear the story board, if it has previously been filled
            if (_storyboard.Children.Count > 0)
            {
                _storyboard.Children.Clear();
            }
            _state = AnimationState.Stop;
        }



    }
}
