using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Exceptions
{


    public abstract class NzbDroneException : ApplicationException
    {
        protected NzbDroneException(string message, params object[] args)
            : base(string.Format(message, args))
        {

        }


        protected NzbDroneException(string message)
            : base(message)
        {

        }
    }

}
