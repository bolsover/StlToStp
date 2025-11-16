using System.Collections.Generic;
using System.Numerics;

namespace Bolsover.Decimator
{
    public  class Vector3Comparer : IEqualityComparer<Vector3> {
        public bool Equals(Vector3 a, Vector3 b) => Vector3.Distance(a, b) < 1e-6;
        public int GetHashCode(Vector3 v) => v.GetHashCode();
    }
}