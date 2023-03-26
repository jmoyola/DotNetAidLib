using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cmd
{
    public class ParameterInfoValue
    {
        public ParameterInfoValue(ParameterInfo parameter, string value)
        {
            Assert.NotNull(parameter, nameof(parameter));
            Assert.NotNullOrEmpty(value, nameof(value));

            Parameter = parameter;

            try
            {
                Value = Parameter.Cast(value);
            }
            catch (Exception ex)
            {
                throw new ParameterInfoException("No valid value '" + value + "' for parameter " + parameter.Name + ".",
                    ex);
            }
        }

        public ParameterInfo Parameter { get; }

        public object Value { get; }

        public override string ToString()
        {
            return Parameter.Name + " = " + (Value == null ? "null" : Value.ToString());
        }
    }
}