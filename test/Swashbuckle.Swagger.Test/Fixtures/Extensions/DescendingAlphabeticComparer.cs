﻿using System.Collections.Generic;

namespace Swashbuckle.Swagger.Fixtures.Extensions
{
    public class DescendingAlphabeticComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return y.CompareTo(x);
        }
    }
}