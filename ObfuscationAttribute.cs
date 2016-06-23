using System;
using System.Collections.Generic;
using System.Text;

namespace System.Reflection
{
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class ObfuscationAttribute : Attribute
    {
        public bool Exclude{ get; set; }
        public bool ApplyToMembers { get; set; }
        public String Feature { get; set; }
        public bool StripAfterObfuscation { get; set; }
    }
}
