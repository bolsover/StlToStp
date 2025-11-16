using System;
using Bolsover.Decimator;
using NUnit.Framework;

namespace TestStlToStp
{
    [TestFixture]
    public class DecimatorTests
    {
        [Test]
        public void TestDecimate()
        {
            var simplifier = new StlSimplifier();
            StlLoader.LoadAscii("Bucket.stl", simplifier);
            simplifier.Simplify(targetFaceCount: Convert.ToInt32(simplifier.Faces.Count *0.5));
            StlExporter.SaveAscii("Decimated.stl", simplifier);
        }
    }
}