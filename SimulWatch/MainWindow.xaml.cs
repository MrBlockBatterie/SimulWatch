using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using HtmlAgilityPack;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using SimulWatch.Net;
using SimulWatch.Utility;
using Vlc.DotNet.Core;

namespace SimulWatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private readonly DoubleAnimation fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.2)));
        private readonly DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.2)));
        
        private bool _isHost;

        private InternetBrowser Browser => new InternetBrowser();

        private bool IsHost
        {
            get => _isHost;
            set
            {
                _isHost = value;
                OpenStreamItem.IsEnabled = true;
            }
        }
        public VlcMediaPlayer MediaPlayer;
        public Host Host;
        public MainWindow()
        {
            InitializeComponent();
            InitMediaPlayer();
            MediaPlayer.MediaChanged += delegate(object sender, VlcMediaPlayerMediaChangedEventArgs args)
            {
                MediaPlayer.SetPause(true);
                
                Thread test = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    var length = MediaPlayer.GetMedia().Duration.ToString(@"h\:mm\:ss");
                   

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        TotalLength.Content = length; 
                    
                    });
                });
                
                test.Start();
                

            };
            MediaPlayer.TimeChanged += MediaPlayerOnTimeChanged;
            Volume.Value = MediaPlayer.Audio.Volume;
            

            //mediaPlayer.Play("https://s27.stream.proxer.me/files/2/ehv0g7davh9vxa/video.mp4");

        }

        private void MediaPlayerOnTimeChanged(object sender, VlcMediaPlayerTimeChangedEventArgs e)
        {
            //Debug.WriteLine(MediaPlayer.Time % 100);
            if (MediaPlayer.Time % 100 <= 49) return;
            var time = TimeSpan.FromMilliseconds(e.NewTime);
            var length = time.ToString(@"h\:mm\:ss");
                
            Debug.WriteLine(e.NewTime);
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentTime.Content = length;
            });
        }

        private void InitMediaPlayer()
        {
            var vlcLibDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            var options = new string[]
            {
                "--demux", "h264"
            };
            VlcControl.SourceProvider.CreatePlayer(vlcLibDirectory, options);
            MediaPlayer = VlcControl.SourceProvider.MediaPlayer;
        }

        private void PlayPauseButton(object sender, RoutedEventArgs e)
        {
            
            if (MediaPlayer.IsPlaying())
            {
                MediaPlayer.Pause();
                if (IsHost)
                {
                    Host.SendPackage(SyncAction.Pause);
                }
            }
            else
            {
                MediaPlayer.Play();
                if (IsHost)
                {
                    Host.SendPackage(SyncAction.Play);
                }
            }

            if (Slider.Maximum != MediaPlayer.GetMedia().Duration.TotalMilliseconds)
            {
                Debug.WriteLine(MediaPlayer.GetMedia().Duration.TotalMilliseconds);
                Slider.Maximum = MediaPlayer.GetMedia().Duration.TotalMilliseconds;
                Debug.WriteLine(Slider.Maximum);
                Slider.TickFrequency = 500;
            }
            
        }

        

        private void VolumeLower(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Audio.Volume > 10)
            {
                MediaPlayer.Audio.Volume -= 10;
            }
            else
            {
                MediaPlayer.Audio.Volume = 0;
            }
            VolumeUpdate();
        }

        private void VolumeUpdate()
        {
            Volume.Value = MediaPlayer.Audio.Volume;
        }

        private void VolumeRaise(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Audio.Volume < 90)
            {
                MediaPlayer.Audio.Volume += 10;
            }
            else
            {
                MediaPlayer.Audio.Volume = 100;
            }
            VolumeUpdate();
        }

        public void SkipIntro(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Time += 90000;
            MediaPlayer.Pause();
            if (IsHost)
            {
                Host.SendPackage(SyncAction.SkipIntro);
            }
        }

        private void NewConnectionWindow(object sender, RoutedEventArgs e)
        {
            ConnectionWindow window = new ConnectionWindow();
            window.Show();
        }

        private void HostSession(object sender, RoutedEventArgs e)
        {
            Thread hostThread = new Thread(() => Host = new Host());
            hostThread.Start();
            IsHost = true;
            Title += " {Hosting}";

        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            button.ContextMenu.IsOpen = true;
            
        }
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var but = (Button) sender;
            but.ContextMenu.IsOpen = true;
        }

        private void OpenStream(object sender, RoutedEventArgs e)
        {
            
            MediaPlayer.Play(StreamURL.Text);
            
            if (IsHost)
            {
                Host.SendPackage(StreamURL.Text);
            }

            
        }

        public void ToStart(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position = 0;
            if (MediaPlayer.IsPlaying())
            {
                MediaPlayer.Pause();
            }
            if (IsHost)
            {
                Host.SendPackage(SyncAction.GoToStart);
            }
            
        }

        private void StackPanel_OnMouseEnter(object sender, MouseEventArgs e)
        {
            StackPanel.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void StackPanel_OnMouseLeave(object sender, MouseEventArgs e)
        {
            StackPanel.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void FullScreenMode(object sender, MouseButtonEventArgs e)
        {
            var fScreenWindow = new FullscreenWindow();
            Dock.Children.Remove(VlcControl);
            fScreenWindow.Grid.Children.Add(VlcControl);
            fScreenWindow.Show();
        }

        private void OpenBrowser(object sender, RoutedEventArgs e)
        {
            Browser.Show();


        }

        private async void CopyStreamLink(object sender, RoutedEventArgs e)
        {
            var doc = new HtmlDocument();

            //Debug.WriteLine(Browser.Browser.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" }).Result);
            var html = await Browser.Browser.InvokeScriptAsync("eval", new string[] {"document.documentElement.outerHTML;"});
            doc.LoadHtml(html);

            var link = doc.DocumentNode.Descendants("iframe").First().GetAttributeValue("src", "no link");
            Debug.WriteLine(link);
            
            Browser.Browser.Navigate(link);
            
            Browser.Browser.NavigationCompleted += async delegate(object o, WebViewControlNavigationCompletedEventArgs args)
            {
                html = await Browser.Browser.InvokeScriptAsync("eval", new string[] {"document.documentElement.outerHTML;"});
                doc.LoadHtml(html);
                var mp4 = doc.DocumentNode.Descendants("source").First().GetAttributeValue("src", "no link");
                Debug.WriteLine(mp4);
                
                Browser.Browser.NavigationCompleted += null;
                Browser.Browser.GoBack();
            };
            
            
        }

        private void Debug_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}