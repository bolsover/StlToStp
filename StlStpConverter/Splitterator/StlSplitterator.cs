

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Bolsover.Converter;

namespace Bolsover.Splitterator
{


    public abstract class StlSplitterator
        {

        /// <summary>
        /// Synchronous STL parser that delegates to the async implementation.
        /// Prefer using <see cref="ParseStlAsync"/> in new code for non-blocking I/O.
        /// </summary>
        public static List<Triangle> ParseStl(string path)
        {
            // Delegate to async version to ensure a single I/O implementation path.
            return ParseStlAsync(path).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously reads an STL file using <see cref="StlReader.ReadStlAsync"/> and converts the
        /// returned node list (x, y, z flattened) into a list of <see cref="Triangle"/> objects.
        /// </summary>
        /// <param name="path">Path to the STL file.</param>
        /// <param name="token">Cancellation token.</param>
        /// <param name="progress">Optional textual progress reporter.</param>
        /// <returns>List of triangles parsed from the STL file.</returns>
        public static async Task<List<Triangle>> ParseStlAsync(string path, CancellationToken token = default, IProgress<string> progress = null)
        {
            var nodes = await StlReader.ReadStlAsync(path, token, progress).ConfigureAwait(false);
            return ConvertNodesToTriangles(nodes);
        }

        /// <summary>
        /// Converts a flattened list of doubles [x0,y0,z0, x1,y1,z1, x2,y2,z2, ...] coming from the
        /// STL reader into a list of <see cref="Triangle"/> instances.
        /// </summary>
        private static List<Triangle> ConvertNodesToTriangles(List<double> nodes)
        {
            var triangles = new List<Triangle>(nodes == null ? 0 : nodes.Count / 9);
            if (nodes == null || nodes.Count < 9) return triangles;

            for (int i = 0; i + 8 < nodes.Count; i += 9)
            {
                var tri = new Triangle();
                tri.Vertices[0] = new Vector3((float)nodes[i],     (float)nodes[i + 1], (float)nodes[i + 2]);
                tri.Vertices[1] = new Vector3((float)nodes[i + 3], (float)nodes[i + 4], (float)nodes[i + 5]);
                tri.Vertices[2] = new Vector3((float)nodes[i + 6], (float)nodes[i + 7], (float)nodes[i + 8]);
                triangles.Add(tri);
            }

            return triangles;
        }

        // // Legacy local parsers retained for reference; no longer used by default code path.
        // private static List<Triangle> ParseAsciiStl(Stream stream)
        // {
        //     var triangles = new List<Triangle>();
        //     using var reader = new StreamReader(stream);
        //     string line;
        //     while ((line = reader.ReadLine()) != null)
        //     {
        //         if (line.Trim().StartsWith("facet normal"))
        //         {
        //             var triangle = new Triangle();
        //             reader.ReadLine(); // outer loop
        //             for (int v = 0; v < 3; v++)
        //             {
        //                 var parts = reader.ReadLine().Trim().Split(' ');
        //                 triangle.Vertices[v] = new Vector3(
        //                     float.Parse(parts[1]),
        //                     float.Parse(parts[2]),
        //                     float.Parse(parts[3])
        //                 );
        //             }
        //
        //             reader.ReadLine(); // endloop
        //             reader.ReadLine(); // endfacet
        //             triangles.Add(triangle);
        //         }
        //     }
        //
        //     return triangles;
        // }
        //
        // private static List<Triangle> ParseBinaryStl(BinaryReader reader)
        // {
        //     reader.BaseStream.Seek(80, SeekOrigin.Begin); // skip header
        //     uint triangleCount = reader.ReadUInt32();
        //     var triangles = new List<Triangle>((int)triangleCount);
        //
        //     for (int i = 0; i < triangleCount; i++)
        //     {
        //         reader.ReadBytes(12); // normal vector
        //         var triangle = new Triangle();
        //         for (int v = 0; v < 3; v++)
        //         {
        //             triangle.Vertices[v] = new Vector3(
        //                 reader.ReadSingle(),
        //                 reader.ReadSingle(),
        //                 reader.ReadSingle()
        //             );
        //         }
        //
        //         reader.ReadUInt16(); // attribute byte count
        //         triangles.Add(triangle);
        //     }
        //
        //     return triangles;
        // }



        // --- Optimized topology helpers (vertex snapping variant) ---
        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            public readonly int A, B; // canonicalized so A <= B

            public EdgeKey(int u, int v)
            {
                if (u <= v)
                {
                    A = u;
                    B = v;
                }
                else
                {
                    A = v;
                    B = u;
                }
            }

            public bool Equals(EdgeKey other) => A == other.A && B == other.B;
            public override bool Equals(object obj) => obj is EdgeKey o && Equals(o);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (A * 397) ^ B;
                }
            }
        }

        // Quantized vertex key for tolerant matching
        private readonly struct VertexKeyEps : IEquatable<VertexKeyEps>
        {
            public readonly int X, Y, Z; // quantized coordinates

            public VertexKeyEps(Vector3 v, float invEps)
            {
                X = (int)Math.Round(v.X * invEps);
                Y = (int)Math.Round(v.Y * invEps);
                Z = (int)Math.Round(v.Z * invEps);
            }

            public bool Equals(VertexKeyEps other) => X == other.X && Y == other.Y && Z == other.Z;
            public override bool Equals(object obj) => obj is VertexKeyEps o && Equals(o);

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = 17;
                    h = h * 31 + X;
                    h = h * 31 + Y;
                    h = h * 31 + Z;
                    return h;
                }
            }
        }

        private static void BuildTopologyWithEps(
            List<Triangle> tris, float epsilon,
            out int[][] triVerts,
            out Dictionary<EdgeKey, List<int>> edgeToTris)
        {
            int n = tris.Count;
            triVerts = new int[n][];
            float inv = 1.0f / epsilon;

            // Map snapped vertices to sequential IDs
            var vtxIds = new Dictionary<VertexKeyEps, int>(capacity: Math.Max(4, n * 3));
            int nextId = 0;

            for (int i = 0; i < n; i++)
            {
                var t = tris[i];
                var ids = new int[3];
                for (int k = 0; k < 3; k++)
                {
                    var key = new VertexKeyEps(t.Vertices[k], inv);
                    if (!vtxIds.TryGetValue(key, out int id))
                    {
                        id = nextId++;
                        vtxIds.Add(key, id);
                    }

                    ids[k] = id;
                }

                triVerts[i] = ids;
            }

            edgeToTris = new Dictionary<EdgeKey, List<int>>(capacity: Math.Max(4, n * 3));
            for (int i = 0; i < n; i++)
            {
                var ids = triVerts[i];
                var e0 = new EdgeKey(ids[0], ids[1]);
                var e1 = new EdgeKey(ids[1], ids[2]);
                var e2 = new EdgeKey(ids[2], ids[0]);

                if (!edgeToTris.TryGetValue(e0, out var l0)) edgeToTris[e0] = l0 = new List<int>(2);
                l0.Add(i);
                if (!edgeToTris.TryGetValue(e1, out var l1)) edgeToTris[e1] = l1 = new List<int>(2);
                l1.Add(i);
                if (!edgeToTris.TryGetValue(e2, out var l2)) edgeToTris[e2] = l2 = new List<int>(2);
                l2.Add(i);
            }
        }

        public static int CountConnectedComponents(List<Triangle> triangles)
        {
            int n = triangles.Count;
            if (n == 0) return 0;

            const float epsilon = 1e-5f; // tolerant snapping grid size
            BuildTopologyWithEps(triangles, epsilon, out var triVerts, out var edgeToTris);

            var visited = new bool[n];
            int components = 0;
            var queue = new Queue<int>(Math.Min(1024, n));

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;
                components++;
                visited[i] = true;
                queue.Enqueue(i);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    var ids = triVerts[current];

                    List<int> neigh;
                    if (edgeToTris.TryGetValue(new EdgeKey(ids[0], ids[1]), out neigh))
                        for (int k = 0; k < neigh.Count; k++)
                        {
                            int nb = neigh[k];
                            if (!visited[nb])
                            {
                                visited[nb] = true;
                                queue.Enqueue(nb);
                            }
                        }

                    if (edgeToTris.TryGetValue(new EdgeKey(ids[1], ids[2]), out neigh))
                        for (int k = 0; k < neigh.Count; k++)
                        {
                            int nb = neigh[k];
                            if (!visited[nb])
                            {
                                visited[nb] = true;
                                queue.Enqueue(nb);
                            }
                        }

                    if (edgeToTris.TryGetValue(new EdgeKey(ids[2], ids[0]), out neigh))
                        for (int k = 0; k < neigh.Count; k++)
                        {
                            int nb = neigh[k];
                            if (!visited[nb])
                            {
                                visited[nb] = true;
                                queue.Enqueue(nb);
                            }
                        }
                }
            }

            return components;
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
        
        // needs work!!
        // public static void WriteBinaryStl(string path, List<Triangle> triangles, string header = "solid")
        // {
        //     using var stream = File.OpenWrite(path);
        //     using var writer = new BinaryWriter(stream);
        //
        //     // Write 80-byte header (padded or trimmed to exactly 80 bytes)
        //     var headerBytes = System.Text.Encoding.ASCII.GetBytes(header);
        //     Array.Resize(ref headerBytes, 80);
        //     writer.Write(headerBytes);
        //
        //     // Write the number of triangles
        //     writer.Write((uint)triangles.Count);
        //
        //     // Write triangle data
        //     foreach (var triangle in triangles)
        //     {
        //         // Write the normal vector (for simplicity, write zero-normal; adjust if computed normals needed)
        //         writer.Write(0.0f); // Normal X
        //         writer.Write(0.0f); // Normal Y
        //         writer.Write(0.0f); // Normal Z
        //
        //         // Write the 3 vertices of the triangle
        //         foreach (var vertex in triangle.Vertices)
        //         {
        //             writer.Write(vertex.X);
        //             writer.Write(vertex.Y);
        //             writer.Write(vertex.Z);
        //         }
        //
        //         // Write the attribute byte count (set to 0 as per STL specification)
        //         writer.Write((ushort)0);
        //     }
        // }
        
        

        public static List<List<Triangle>> GetConnectedComponents(List<Triangle> triangles)
        {
            int n = triangles.Count;
            var result = new List<List<Triangle>>();
            if (n == 0) return result;

            const float epsilon = 1e-5f; // tolerant snapping grid size
            BuildTopologyWithEps(triangles, epsilon, out var triVerts, out var edgeToTris);

            var visited = new bool[n];
            var queue = new Queue<int>(Math.Min(1024, n));

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;
                var compIdxs = new List<int>();
                visited[i] = true;
                queue.Enqueue(i);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    compIdxs.Add(current);
                    var ids = triVerts[current];

                    List<int> neigh;
                    if (edgeToTris.TryGetValue(new EdgeKey(ids[0], ids[1]), out neigh))
                        for (int k = 0; k < neigh.Count; k++)
                        {
                            int nb = neigh[k];
                            if (!visited[nb])
                            {
                                visited[nb] = true;
                                queue.Enqueue(nb);
                            }
                        }

                    if (edgeToTris.TryGetValue(new EdgeKey(ids[1], ids[2]), out neigh))
                        for (int k = 0; k < neigh.Count; k++)
                        {
                            int nb = neigh[k];
                            if (!visited[nb])
                            {
                                visited[nb] = true;
                                queue.Enqueue(nb);
                            }
                        }

                    if (edgeToTris.TryGetValue(new EdgeKey(ids[2], ids[0]), out neigh))
                        for (int k = 0; k < neigh.Count; k++)
                        {
                            int nb = neigh[k];
                            if (!visited[nb])
                            {
                                visited[nb] = true;
                                queue.Enqueue(nb);
                            }
                        }
                }

                var group = new List<Triangle>(compIdxs.Count);
                for (int k = 0; k < compIdxs.Count; k++) group.Add(triangles[compIdxs[k]]);
                result.Add(group);
            }

            return result;
        }
        
        public static void SeparateBodies(string inFile, string outDir)
        {
            string path = "Pencil Case.stl";
            var triangles = ParseStl(path);
            var bodies = GetConnectedComponents(triangles);

            for (int i = 0; i < bodies.Count; i++)
            {
                string outputPath = $"body_{i + 1}.stl";
                WriteAsciiStl(outputPath, bodies[i], $"body_{i + 1}");
                Console.WriteLine($"Exported: {outputPath}");
            }
        }
    }
}