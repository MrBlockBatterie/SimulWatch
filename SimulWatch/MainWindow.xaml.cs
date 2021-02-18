﻿using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SimulWatch.Net;
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

        private bool IsHost
        {
            get => _isHost;
            set
            {
                _isHost = value;
                OpenStreamItem.IsEnabled = true;
            }
        }
        public VlcMediaPlayer mediaPlayer;
        public Host host;
        public MainWindow()
        {
            InitializeComponent();
            InitMediaPlayer();
            mediaPlayer.MediaChanged += delegate(object sender, VlcMediaPlayerMediaChangedEventArgs args) { mediaPlayer.Pause(); };
            
            //mediaPlayer.Play("https://s27.stream.proxer.me/files/2/ehv0g7davh9vxa/video.mp4");
            
        }

        private void InitMediaPlayer()
        {
            var vlcLibDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
                "--fullscreen"
            };
            VlcControl.SourceProvider.CreatePlayer(vlcLibDirectory, options);
            mediaPlayer = VlcControl.SourceProvider.MediaPlayer;
        }

        private void PlayPauseButton(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.IsPlaying())
            {
                mediaPlayer.Pause();
                if (IsHost)
                {
                    host.SendPackage(SyncAction.Pause);
                }
            }
            else
            {
                mediaPlayer.Play();
                if (IsHost)
                {
                    host.SendPackage(SyncAction.Play);
                }
            }
            
        }

        

        private void VolumeLower(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Audio.Volume > 10)
            {
                mediaPlayer.Audio.Volume -= 10;
            }
            else
            {
                mediaPlayer.Audio.Volume = 0;
            }
            VolumeUpdate();
        }

        private void VolumeUpdate()
        {
            Volume.Value = mediaPlayer.Audio.Volume;
        }

        private void VolumeRaise(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Audio.Volume < 90)
            {
                mediaPlayer.Audio.Volume += 10;
            }
            else
            {
                mediaPlayer.Audio.Volume = 100;
            }
            VolumeUpdate();
        }

        public void SkipIntro(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Time += 90000;
            mediaPlayer.Pause();
            if (IsHost)
            {
                host.SendPackage(SyncAction.SkipIntro);
            }
        }

        private void NewConnectionWindow(object sender, RoutedEventArgs e)
        {
            ConnectionWindow window = new ConnectionWindow();
            window.Show();
        }

        private void HostSession(object sender, RoutedEventArgs e)
        {
            Thread hostThread = new Thread(() => host = new Host());
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
            mediaPlayer.Play(StreamURL.Text);
            if (IsHost)
            {
                host.SendPackage(StreamURL.Text);
            }

            
        }

        public void ToStart(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Position = 0;
            mediaPlayer.Pause();
            if (IsHost)
            {
                host.SendPackage(SyncAction.GoToStart);
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
            FullscreenWindow fSreenWindow = new FullscreenWindow();
            Dock.Children.Remove(VlcControl);
            fSreenWindow.Grid.Children.Add(VlcControl);
            fSreenWindow.Show();
        }
    }
}