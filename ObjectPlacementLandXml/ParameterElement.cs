using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPlacementLandXml
{
    public class ParameterElement 
    {
        public String ParameterName { get; set; }
        public String ParameterValue { get; set; }

        public ParameterElement(string parameterName, string parameterValue)
        {
            ParameterName = parameterName;
            ParameterValue = parameterValue;
        }
    }
}
