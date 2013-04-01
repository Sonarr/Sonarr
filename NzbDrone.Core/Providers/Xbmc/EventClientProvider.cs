using System;
using System.Collections.Generic;
using System.Text;
using NzbDrone.Common;
using NzbDrone.Core.Model.Xbmc;

namespace NzbDrone.Core.Providers.Xbmc
{
    public class EventClientProvider
    {
        private readonly UdpProvider _udpProvider;

        public EventClientProvider(UdpProvider udpProvider)
        {
            _udpProvider = udpProvider;
        }

        public EventClientProvider()
        {
        }

        public virtual bool SendNotification(string caption, string message, IconType iconType, string iconFile, string address)
        {
            byte[] icon = new byte[0];
            if (iconType != IconType.None)
            {
                icon = ResourceManager.GetRawLogo(iconFile);
            }

            byte[] payload = new byte[caption.Length + message.Length + 7 + icon.Length];

            int offset = 0;

            for (int i = 0; i < caption.Length; i++)
                payload[offset++] = (byte)caption[i];
            payload[offset++] = (byte)'\0';

            for (int i = 0; i < message.Length; i++)
                payload[offset++] = (byte)message[i];
            payload[offset++] = (byte)'\0';

            payload[offset++] = (byte)iconType;

            for (int i = 0; i < 4; i++)
                payload[offset++] = 0;

            Array.Copy(icon, 0, payload, caption.Length + message.Length + 7, icon.Length);

            return _udpProvider.Send(address, UdpProvider.PacketType.Notification, payload);
        }

        public virtual bool SendAction(string address, ActionType action, string messages)
        {
            var payload = new byte[messages.Length + 2];
            int offset = 0;
            payload[offset++] = (byte)action;

            for (int i = 0; i < messages.Length; i++)
                payload[offset++] = (byte)messages[i];

            payload[offset++] = (byte)'\0';

            return _udpProvider.Send(address, UdpProvider.PacketType.Action, payload);
        }
    }
}