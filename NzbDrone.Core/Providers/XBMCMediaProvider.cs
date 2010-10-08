using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public class XBMCMediaProvider : IMediaProvider
    {
        public OpenSource.XBMC.XBMCAVDevice XBMCDevice{ get; set; }
        public XBMCMediaProvider(OpenSource.XBMC.XBMCAVDevice XBMCDevice)
        {
            this.XBMCDevice = XBMCDevice;
        }

        #region IAVDevice Members

        public DateTime FirstSeen { get { return XBMCDevice.FirstSeen; }  }
        public DateTime LastSeen { get { return XBMCDevice.LastSeen; } }
        public bool IsActive
        {
            get
            {
                return XBMCDevice.IsActive;
            }
        }

public void Play()
        {
            XBMCDevice.Play();
        }
        public void Play(string Location)
        {
            XBMCDevice.Play(Location);
        }
        public void Play(string Location, bool AddToQueue)
        {
            XBMCDevice.Play(Location, AddToQueue);
        }
        public void Stop()
        {
            XBMCDevice.Stop();
           
        }

        public void Next()
        {
            XBMCDevice.Next();
        }
        public void Previous()
        {
            XBMCDevice.Previous();
        }
        public void Seek(string RawTime)
        {
            XBMCDevice.Seek(RawTime);
        }
        public void Seek(int Hour, int Minute, int Second)
        {
            XBMCDevice.Seek(Hour, Minute, Second);
        }
        public void Seek(System.TimeSpan SeekTime)
        {
            XBMCDevice.Seek(SeekTime);
        }


        public void Queue(string Location)
        {
            XBMCDevice.Queue(Location);
        }
        
        
        public OpenSource.UPnP.UPnPDevice Device {
            get { return XBMCDevice.Device; }
        }
        public void Pause()
        {
            XBMCDevice.Pause();
        }

        public System.Uri Uri
        {
            get { return XBMCDevice.Uri; }
        }
        public string UniqueDeviceName
        {
            get { return XBMCDevice.UniqueDeviceName; }
        }
        
        public OpenSource.UPnP.AV.CpAVTransport Transport
        {
            get
            {
                return XBMCDevice.Transport;
            }
        }

        public string FriendlyName
        {
            get
            {
                return XBMCDevice.FriendlyName;
            }
        }

        #endregion
    }
}
