using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Demo
{

    public sealed partial class MainPage : Page
    {
        List<Uri> list = new List<Uri>();
        public MainPage()
        {
            this.InitializeComponent();
            base.Loaded += MainPage_Loaded;
            this.NavigationCacheMode = NavigationCacheMode.Disabled;

            list.Add(new Uri("ms-appx:///Images/2.gif"));
            list.Add(new Uri("ms-appx:///Images/3.gif"));
            list.Add(new Uri("ms-appx:///Images/2.gif"));
            list.Add(new Uri("ms-appx:///Images/3.gif"));
            list.Add(new Uri("ms-appx:///Images/2.gif"));
            list.Add(new Uri("ms-appx:///Images/3.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));
            list.Add(new Uri("ms-appx:///Images/test.webp"));
            list.Add(new Uri("http://b.hiphotos.baidu.com/zhidao/pic/item/9f510fb30f2442a768a9fe5fd043ad4bd01302a0.webp"));
            list.Add(new Uri("http://imgsrc.baidu.com/forum/w%3D580%3B/sign=ca3845692bf5e0feee1889096c5b35a8/7e3e6709c93d70cf01e0e07dfedcd100baa12ba3.webp"));
            list.Add(new Uri("http://img5.duitang.com/uploads/item/201411/25/20141125200101_QQjNY.gif"));


            listView.ItemsSource = list;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //test.Width = 100;
        }
    }
}
