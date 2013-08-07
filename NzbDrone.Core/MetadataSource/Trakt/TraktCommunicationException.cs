using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.Trakt
{
   public class TraktCommunicationException : Exception
    {
       public TraktCommunicationException(string message, Exception innerException) : base(message, innerException)
       {
       }
    }
}
