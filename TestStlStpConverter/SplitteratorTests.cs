
using NUnit.Framework;
using System;

using Bolsover.Splitterator;

namespace TestStlToStp
{

    [TestFixture]
    class SplitteratorTests
    {
        [Test]
        public static void Triangles()
        {
            string path = "roach_kit_card.stl";
            var triangles = StlSplitterator.ParseStl(path);
            var tianglscount = triangles.Count;
            Console.WriteLine($"Number of triangles: {tianglscount}");
             int bodies = StlSplitterator.CountConnectedComponents(triangles);
             Console.WriteLine($"Estimated number of solid bodies: {bodies}");
        }

        [Test]
        public static void SeparateBodies()
        {
            string path = "roach_kit_card.stl";
            var triangles = StlSplitterator.ParseStl(path);
            var bodies = StlSplitterator.GetConnectedComponents(triangles);

            for (int i = 0; i < bodies.Count; i++)
            {
                string outputPath = $"body_{i + 1}.stl";
                StlSplitterator.WriteAsciiStl(outputPath, bodies[i], $"body_{i + 1}");
                Console.WriteLine($"Exported: {outputPath}");
            }
        }
    }
}