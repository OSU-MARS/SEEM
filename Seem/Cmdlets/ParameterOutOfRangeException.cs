using System;

namespace Osu.Cof.Ferm.Cmdlets
{
    class ParameterOutOfRangeException : InvalidOperationException
    {
        public ParameterOutOfRangeException(string? paramName)
            : this(paramName, "Specified parameter was out of the range of valid values.")
        {
        }

        public ParameterOutOfRangeException(string? paramName, string? message)
            : base(message + " (Parameter '" + paramName + "')")
        {
        }
    }
}
