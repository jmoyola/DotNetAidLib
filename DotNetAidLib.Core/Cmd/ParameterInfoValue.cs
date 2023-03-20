using System;
using System.Globalization;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cmd
{
    public class ParameterInfoValue
    {
        private ParameterInfo parameter;
        private Object value;

        public ParameterInfoValue (ParameterInfo parameter, String value)
        {
            Assert.NotNull (parameter, nameof(parameter));
            Assert.NotNullOrEmpty (value, nameof (value));

            this.parameter = parameter;

            try{
                this.value = this.parameter.Cast(value);
            }
            catch (Exception ex){
                throw new ParameterInfoException("No valid value '" + value + "' for parameter " + parameter.Name + ".", ex);
            }
        }

        public ParameterInfo Parameter { get => parameter;}

        public Object Value {
            get
            {
                return this.value;
            }
        }

        public override string ToString ()
        {
            return this.parameter.Name + " = " + (this.Value==null?"null":this.Value.ToString());
        }
    }
}
