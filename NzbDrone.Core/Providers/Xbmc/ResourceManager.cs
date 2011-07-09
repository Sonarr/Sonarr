namespace NzbDrone.Core.Providers.Xbmc
{
    public class ResourceManager
    {
        public static System.Drawing.Icon GetIcon(string Name)
        {
            System.IO.Stream stm = typeof(ResourceManager).Assembly.GetManifestResourceStream(string.Format("NzbDrone.Core.{0}.ico", Name));
            if (stm == null) return null;
            return new System.Drawing.Icon(stm);
        }

        public static byte[] GetRawData(string Name)
        {
            byte[] data;
            using (System.IO.Stream stm = typeof(ResourceManager).Assembly.GetManifestResourceStream(string.Format("NzbDrone.Core.{0}.ico", Name)))
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
            using (System.IO.Stream stm = typeof(ResourceManager).Assembly.GetManifestResourceStream(string.Format("NzbDrone.Core.{0}", Name)))
            {
                if (stm == null) return null;
                data = new byte[stm.Length];
                stm.Read(data, 0, data.Length);
            }

            return data;
        }

        public static System.Drawing.Bitmap GetIconAsImage(string Name)
        {
            System.IO.Stream stm = typeof(ResourceManager).Assembly.GetManifestResourceStream(string.Format("{0}.Icons.{1}.ico", typeof(ResourceManager).Namespace, Name));
            if (stm == null) return null;
            System.Drawing.Bitmap bmp;
            using (System.Drawing.Icon ico = new System.Drawing.Icon(stm))
            {
                bmp = new System.Drawing.Bitmap(ico.Width, ico.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.DrawIcon(ico, 0, 0);
                }
            }

            return bmp;
        }
    }
}
