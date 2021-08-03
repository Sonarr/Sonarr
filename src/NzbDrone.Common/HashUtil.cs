using System;
using System.Text;

namespace NzbDrone.Common
{
    public static class HashUtil
    {
        public static string CalculateCrc(string input)
        {
            uint mCrc = 0xffffffff;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            foreach (byte myByte in bytes)
            {
                mCrc ^= (uint)myByte << 24;
                for (var i = 0; i < 8; i++)
                {
                    if ((Convert.ToUInt32(mCrc) & 0x80000000) == 0x80000000)
                    {
                        mCrc = (mCrc << 1) ^ 0x04C11DB7;
                    }
                    else
                    {
                        mCrc <<= 1;
                    }
                }
            }

            return $"{mCrc:x8}";
        }

        public static string AnonymousToken()
        {
            var seed = $"{Environment.ProcessorCount}_{Environment.OSVersion.Platform}_{Environment.MachineName}_{Environment.UserName}";
            return HashUtil.CalculateCrc(seed);
        }
    }
}
