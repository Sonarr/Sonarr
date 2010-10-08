using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public class MediaDiscoveryProvider : IMediaDiscoveryProvider
    {
        #region IMediaDiscoveryProvider Members
        
        public bool DiscoveredMedia
        {
            get { return (OpenSource.UPnP.AudioVideoDevices.Instance.Devices.Count > 0); }
        }

        private object _lock = new object();
        public List<IMediaProvider> Providers
        {
            get {
                lock (_lock)
                {
                    List<IMediaProvider> list = new List<IMediaProvider>();
                    foreach (OpenSource.UPnP.IAVDevice device in OpenSource.UPnP.AudioVideoDevices.Instance.Devices)
                    {
                        OpenSource.XBMC.XBMCAVDevice xbmc = (device as OpenSource.XBMC.XBMCAVDevice);
                        if (xbmc != null)
                        {
                            XBMCMediaProvider newX = new XBMCMediaProvider(xbmc);
                            list.Add(newX);
                        }
                    }
                    return list;
                }
            }
        }

        #endregion
    }
}
