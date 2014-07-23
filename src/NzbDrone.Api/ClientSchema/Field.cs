using System;
using System.Collections.Generic;

namespace NzbDrone.Api.ClientSchema
{
    public class Field
    {
        public Int32 Order { get; set; }
        public String Name { get; set; }
        public String Label { get; set; }
        public String HelpText { get; set; }
        public String HelpLink { get; set; }
        public Object Value { get; set; }
        public String Type { get; set; }
        public Boolean Advanced { get; set; }
        public List<SelectOption> SelectOptions { get; set; }
    }
}