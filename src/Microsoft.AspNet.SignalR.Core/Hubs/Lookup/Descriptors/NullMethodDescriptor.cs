using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class NullMethodDescriptor : MethodDescriptor
    {
        private static readonly IEnumerable<Attribute> _attributes = new List<Attribute>();
        private static readonly IList<ParameterDescriptor> _parameters = new List<ParameterDescriptor>();

        private string _methodName;

        public NullMethodDescriptor(string methodName)
        {
            _methodName = methodName;
        }

        public override Func<IHub, object[], object> Invoker
        {
            get
            {
                return (emptyHub, emptyParameters) =>
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Error_MethodCouldNotBeResolved, _methodName));
                };
            }
        }

        public override IList<ParameterDescriptor> Parameters
        {
            get { return _parameters; }
        }

        public override IEnumerable<Attribute> Attributes 
        {
            get { return _attributes; }
        }
    }
}
