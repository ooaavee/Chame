using System;
using System.Collections.Generic;
using System.Text;

namespace Chame.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FallbackService : Attribute
    {
    }
}
