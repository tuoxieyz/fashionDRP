using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;

namespace DomainLogicEncap
{
    public class IDEntityComparer<T> : IEqualityComparer<T> where T : IDEntity
    {
        public bool Equals(T x, T y)
        {
            if (x == null && y == null)
                return false;
            return x.ID == y.ID;
        }

        public int GetHashCode(T obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
}
