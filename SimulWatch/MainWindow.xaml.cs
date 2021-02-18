using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SimulWatch.Net;
using Vlc.DotNet.Core;

namespace SimulWatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public bool IsHost = false;
        public VlcMediaPlayer mediaPlayer;
        public Host host;
        public MainWindow()
        {
            InitializeComponent();
            InitMediaPlayer();
            mediaPlayer.MediaChanged += delegate(object sender, VlcMediaPlayerMediaChangedEventArgs args) { mediaPlayer.Pause(); };
            
            mediaPlayer.Play("https://s27.stream.proxer.me/files/2/ehv0g7davh9vxa/video.mp4");
            
        }

        private void InitMediaPlayer()
        {
            var vlcLibDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
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

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            button.ContextMenu.IsOpen = true;
            
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
        }
    }
}