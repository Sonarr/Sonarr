/*
 *Source:http://stackoverflow.com/questions/1520945/nlog-to-output-db-out 
 *DamienG
  */

using System;
using System.IO;
using System.Text;
using NLog;

namespace NzbDrone.Core
{
    class SonicTrace : TextWriter
    {
        private static readonly Logger Logger = LogManager.GetLogger("DB");


        public override void Write(char[] buffer, int index, int count)
        {
            Write(new string(buffer, index, count));
        }

        public override void Write(string value)
        {
            DbAction(value);
        }

        private static void DbAction(string value)
        {
            Logger.Trace(value);
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }

}