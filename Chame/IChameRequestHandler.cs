using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame
{
    public interface IChameRequestHandler
    {
        Task<bool> HandleAsync(ChameContext context);
    }
}
