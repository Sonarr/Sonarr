using System.Drawing;
using System.IO;

namespace NzbDrone.Core.Providers.Xbmc
{
    public class ResourceManager
    {
        public static Icon GetIcon(string Name)
        {
            Stream stm = typeof(ResourceManager).Assembly.GetManifestResourceStream(string.Format("NzbDrone.Core.{0}.ico", Name));
            if (stm == null) return null;
            return new Icon(stm);
        }

        public static byte[] GetRawData(string Name)
        {
            byte[] data;
            using (Stream stm = typeof(ResourceManager).Assembly.GetManifestResourceStream(string.Format("NzbDrone.Core.{0}.ico", Name)))
            {
                if (stm == null) return null;
                data = new byte[stm.Length];
                stm.Read(data, 0, data.Length);
            }

            return data;
        }

        public static byte[] GetRawLogo(string Name)
        {
            byte[] data;
            using (Stream stm = typeof(ResourceManager).Assembly.GetManifestResourceStream(string.Format("NzbDrone.Core.{0}", Name)))
            {
                if (stm == null) return null;
                data = new byte[stm.Length];
                stm.Read(data, 0, data.Length);
            }

            return data;
        }

        public static Bitmap GetIconAsImage(string Name)
        {
            Stream stm = typeof(ResourceManager).Assembly.GetManifestResourceStream(string.Format("NzbDrone.Core.{0}.ico", Name));
            if (stm == null) return null;
            Bitmap bmp;
            using (Icon ico = new Icon(stm))
            {
                bmp = new Bitmap(ico.Width, ico.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawIcon(ico, 0, 0);
                }
            }

            return bmp;
        }
    }
}
