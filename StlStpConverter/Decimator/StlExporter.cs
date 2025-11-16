using System.Numerics;

namespace Bolsover.Decimator
{
    public static class StlExporter {
        public static void SaveAscii(string path, StlSimplifier simplifier) {
            using (var writer = new System.IO.StreamWriter(path)) {
                writer.WriteLine("solid simplified");

                foreach (var face in simplifier.Faces) {
                    var v0 = simplifier.Vertices[face.Vertices[0]].Position;
                    var v1 = simplifier.Vertices[face.Vertices[1]].Position;
                    var v2 = simplifier.Vertices[face.Vertices[2]].Position;
                    var normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));

                    writer.WriteLine($"  facet normal {normal.X} {normal.Y} {normal.Z}");
                    writer.WriteLine("    outer loop");
                    writer.WriteLine($"      vertex {v0.X} {v0.Y} {v0.Z}");
                    writer.WriteLine($"      vertex {v1.X} {v1.Y} {v1.Z}");
                    writer.WriteLine($"      vertex {v2.X} {v2.Y} {v2.Z}");
                    writer.WriteLine("    endloop");
                    writer.WriteLine("  endfacet");
                }

                writer.WriteLine("endsolid simplified");
            }
        }
    }
}