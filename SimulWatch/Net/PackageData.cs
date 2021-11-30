using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SimulWatch.Utility;

namespace SimulWatch.Net
{
    public class PackageData
    {
        public SyncAction Action { get; }
        public object ExtraData { get; }
        public string Source { get; set; }

        public byte[] OutgoingData
        {
            get
            {
                int l = 5;
                byte[] buffer = new byte[2048];
                byte[] data = null;
                switch (Action)
                {
                    case SyncAction.Pause:
                        data = new byte[]
                        {
                            1, 0, 0, 0, 0,
                            (byte)SyncAction.Pause
                        };

                        break;
                    case SyncAction.Play:
                        data = new byte[]
                        {
                            1, 0, 0, 0, 0,
                            (byte)SyncAction.Play
                        };

                        break;
                    case SyncAction.SkipIntro:
                        data = new byte[]
                        {
                            1, 0, 0, 0, 0,
                            (byte)SyncAction.SkipIntro
                        };

                        break;
                    case SyncAction.GoToStart:
                        data = new byte[]
                        {
                            1, 0, 0, 0, 0,
                            (byte)SyncAction.GoToStart
                        };
                        break;
                    case SyncAction.LoadSource:
                        Debug.WriteLine($"Message is {Source.Length} bytes long");
                        int stringLength = Source.Length;
                        byte[] length = new Byte[] { 0, 0, 0, 0, 0 };
                        if (stringLength > 255)
                        {
                            length[0] = 255;
                            int index = 1;

                            if (stringLength - 255 <= 255)
                            {
                                length[1] = (byte)(stringLength % 255);
                            }

                            while (stringLength - 255 >= 255)
                            {
                                length[index] = 255;
                                stringLength -= 255;
                                index++;
                                if (stringLength - 255 <= 255)
                                {
                                    length[index] = (byte)(stringLength - 255);
                                }
                            }
                        }
                        else
                        {
                            length[0] = (byte)stringLength;
                        }

                        data = new byte[]
                        {
                            length[0],
                            length[1],
                            length[2],
                            length[3],
                            length[4],
                            (byte)SyncAction.LoadSource
                        };
                        
                        byte[] bytes = Encoding.ASCII.GetBytes(Source);
                        data = data.Concatenate(bytes);
                        break;
                }
                Array.Copy((Array)data, (Array)buffer, data.Length);
                return buffer;
            }
        }

        public PackageData(SyncAction action, object extraData)
        {
            Action = action;
            ExtraData = extraData;
            var @switch = new Dictionary<Type, Action> {
                { typeof(string), () => TypeString() },
                { typeof(byte[]), () => TypeByteArr() }
            };
            @switch[extraData.GetType()]();
            
        }

        private void TypeString()
        {
            Source = (string)ExtraData;
        }

        private void TypeByteArr()
        {
            
        }
    }
}