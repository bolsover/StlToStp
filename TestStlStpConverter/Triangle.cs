
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace TestStlToStp
{

    [TestFixture]
    public class Triangle
    {
        public Vector3[] Vertices = new Vector3[3];

        public IEnumerable<(Vector3, Vector3)> GetEdges()
        {
            yield return (Vertices[0], Vertices[1]);
            yield return (Vertices[1], Vertices[2]);
            yield return (Vertices[2], Vertices[0]);
        }
    }
    
   



    public class STLParser
    {
        
        public static List<Triangle> ParseStl(string path)
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);

            // Check if ASCII or Binary
            var header = reader.ReadBytes(80);
            var isAscii = System.Text.Encoding.ASCII.GetString(header).Contains("solid");

            stream.Seek(0, SeekOrigin.Begin); // Reset stream

            return isAscii ? ParseAsciiStl(stream) : ParseBinaryStl(reader);
        }
        private static List<Triangle> ParseAsciiStl(Stream stream)
        {
            var triangles = new List<Triangle>();
            using var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim().StartsWith("facet normal"))
                {
                    var triangle = new Triangle();
                    reader.ReadLine(); // outer loop
                    for (int v = 0; v < 3; v++)
                    {
                        var parts = reader.ReadLine().Trim().Split(' ');
                        triangle.Vertices[v] = new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])
                        );
                    }
                    reader.ReadLine(); // endloop
                    reader.ReadLine(); // endfacet
                    triangles.Add(triangle);
                }
            }
            return triangles;
        }

        private static List<Triangle> ParseBinaryStl(BinaryReader reader)
        {
            reader.BaseStream.Seek(80, SeekOrigin.Begin); // skip header
            uint triangleCount = reader.ReadUInt32();
            var triangles = new List<Triangle>((int)triangleCount);

            for (int i = 0; i < triangleCount; i++)
            {
                reader.ReadBytes(12); // normal vector
                var triangle = new Triangle();
                for (int v = 0; v < 3; v++)
                {
                    triangle.Vertices[v] = new Vector3(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                    );
                }
                reader.ReadUInt16(); // attribute byte count
                triangles.Add(triangle);
            }

            return triangles;
        }



        public static int CountConnectedComponents(List<Triangle> triangles)
        {
            var visited = new HashSet<int>();
            var components = 0;

            for (int i = 0; i < triangles.Count; i++)
            {
                if (visited.Contains(i)) continue;
                components++;
                var queue = new Queue<int>();
                queue.Enqueue(i);
                visited.Add(i);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    foreach (int neighbor in FindConnectedTriangles(current, triangles))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return components;
        }

        private static IEnumerable<int> FindConnectedTriangles(int index, List<Triangle> triangles)
        {
            var edges = new HashSet<(Vector3, Vector3)>(NormalizeEdges(triangles[index].GetEdges()));
            for (int i = 0; i < triangles.Count; i++)
            {
                if (i == index) continue;
                var otherEdges = NormalizeEdges(triangles[i].GetEdges());
                foreach (var edge in otherEdges)
                {
                    if (edges.Contains(edge))
                    {
                        yield return i;
                        break;
                    }
                }
            }
        }

        private static IEnumerable<(Vector3, Vector3)> NormalizeEdges(IEnumerable<(Vector3, Vector3)> edges)
        {
            foreach (var (a, b) in edges)
            {
                yield return (Vector3.Min(a, b), Vector3.Max(a, b));
            }
        }
        
        public static void WriteAsciiStl(string path, List<Triangle> triangles, string solidName = "body")
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine($"solid {solidName}");

            foreach (var tri in triangles)
            {
                writer.WriteLine("  facet normal 0 0 0");
                writer.WriteLine("    outer loop");
                foreach (var v in tri.Vertices)
                {
                    writer.WriteLine($"      vertex {v.X} {v.Y} {v.Z}");
                }
                writer.WriteLine("    endloop");
                writer.WriteLine("  endfacet");
            }

            writer.WriteLine($"endsolid {solidName}");
        }
        
        public static List<List<Triangle>> GetConnectedComponents(List<Triangle> triangles)
        {
            var visited = new HashSet<int>();
            var components = new List<List<Triangle>>();

            for (int i = 0; i < triangles.Count; i++)
            {
                if (visited.Contains(i)) continue;
                var group = new List<Triangle>();
                var queue = new Queue<int>();
                queue.Enqueue(i);
                visited.Add(i);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    group.Add(triangles[current]);

                    foreach (int neighbor in FindConnectedTriangles(current, triangles))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                components.Add(group);
            }

            return components;
        }
    }

    [TestFixture]
    class TriangleTests
    {
        [Test]
        public static void Triangles()
        {
            string path = "Pencil Case.stl";
            var triangles = STLParser.ParseStl(path);
            Console.WriteLine($"Number of triangles: {triangles}");
            int bodies = STLParser.CountConnectedComponents(triangles);
            Console.WriteLine($"Estimated number of solid bodies: {bodies}");
        }

        [Test]
        public static void SeparateBodies()
        {
            string path = "Pencil Case.stl";
            var triangles = STLParser.ParseStl(path);
            var bodies = STLParser.GetConnectedComponents(triangles);

            for (int i = 0; i < bodies.Count; i++)
            {
                string outputPath = $"body_{i + 1}.stl";
                STLParser.WriteAsciiStl(outputPath, bodies[i], $"body_{i + 1}");
                Console.WriteLine($"Exported: {outputPath}");
            }
        }
    }
}