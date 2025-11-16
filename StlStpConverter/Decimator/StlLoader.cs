using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Bolsover.Decimator
{
    public static class StlLoader {
        public static void LoadAscii(string path, StlSimplifier simplifier) {
            var lines = System.IO.File.ReadAllLines(path);
            var vertexMap = new Dictionary<Vector3, int>(new Vector3Comparer());

            for (int i = 0; i < lines.Length; i++) {
                if (lines[i].Trim().StartsWith("vertex")) {
                    var face = new Face();
                    for (int j = 0; j < 3; j++) {
                        var parts = lines[i + j].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var v = new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])
                        );
                        if (!vertexMap.TryGetValue(v, out int index)) {
                            index = simplifier.Vertices.Count;
                            simplifier.Vertices.Add(new Vertex { Position = v });
                            vertexMap[v] = index;
                        }
                        face.Vertices[j] = index;
                    }
                    simplifier.Faces.Add(face);
                    i += 2;
                }
            }
        }

        public static void LoadBinary(string path, StlSimplifier simplifier) {
            var vertexMap = new Dictionary<Vector3, int>(new Vector3Comparer());

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream)) {
                reader.ReadBytes(80); // Skip header
                uint triangleCount = reader.ReadUInt32();

                for (int i = 0; i < triangleCount; i++) {
                    reader.ReadBytes(12); // Skip normal

                    var face = new Face();
                    for (int j = 0; j < 3; j++) {
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();
                        var v = new Vector3(x, y, z);

                        if (!vertexMap.TryGetValue(v, out int index)) {
                            index = simplifier.Vertices.Count;
                            simplifier.Vertices.Add(new Vertex { Position = v });
                            vertexMap[v] = index;
                        }
                        face.Vertices[j] = index;
                    }

                    simplifier.Faces.Add(face);
                    reader.ReadUInt16(); // Skip attribute byte count
                }
            }
        }

    }
}