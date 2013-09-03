using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace NzbDrone.Common
{
    public static class HashUtil
    {
        //This should never be changed. very bad things will happen!
        private static readonly DateTime Epoch = new DateTime(2010, 1, 1);

        private static readonly object _lock = new object();

        public static string CalculateCrc(string input)
        {
            uint mCrc = 0xffffffff;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            foreach (byte myByte in bytes)
            {
                mCrc ^= ((uint)(myByte) << 24);
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
            return String.Format("{0:x8}", mCrc);
        }

        public static string GenerateCommandId()
        {
            return GenerateId("c");
        }

        private static string GenerateId(string prefix)
        {
            lock (_lock)
            {
                Thread.Sleep(1);
                var tick = (DateTime.Now - Epoch).Ticks;
                return prefix + "." + ToBase(tick);
            }
        }

        private static string ToBase(long input)
        {
            const string BASE_CHARS = "0123456789abcdefghijklmnopqrstuvwxyz";
            int targetBase = BASE_CHARS.Length;

            var result = new StringBuilder();
            do
            {
                result.Append(BASE_CHARS[(int)(input % targetBase)]);
                input /= targetBase;
            } while (input > 0);

            return result.ToString();
        }

    }
}