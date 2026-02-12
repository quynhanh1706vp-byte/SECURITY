using System.Collections.Generic;

namespace DeMasterProCloud.Common.Infrastructure
{
    public class NullOrEmptyStringReducer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var xNull = x == null;
            var yNull = y == null;

            if (xNull && yNull)
                return 0;

            if (xNull)
                return 1;

            if (yNull)
                return -1;

            var xEmpty = x == "";
            var yEmpty = y == "";

            if (xEmpty && yEmpty)
                return 0;

            if (xEmpty)
                return 1;

            if (yEmpty)
                return -1;

            return string.Compare(x, y);
        }
    }
}
