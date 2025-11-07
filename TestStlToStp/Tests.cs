using System;
using Bolsover.StlToStp.Converter;
using NUnit.Framework;

namespace TestStlToStp
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Test1()
        {
            StepKernel stepKernel = new StepKernel();
            stepKernel.ReadStep("bucket.stp");
        }
    }
}