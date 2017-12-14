using System;
using System.Collections.Generic;
using System.Text;

namespace Chame
{
    internal sealed class AdHocTheme : ITheme
    {
        private readonly string _name;

        public AdHocTheme(string name)
        {
            _name = name;
        }

        public string GetName()
        {
            return _name;
        }
    }

}
