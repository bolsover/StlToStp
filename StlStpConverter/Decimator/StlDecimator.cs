
using System;
using System.Collections.Generic;
using System.Numerics;


namespace Bolsover.Decimator
{


    public class StlSimplifier
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public List<Face> Faces = new List<Face>();
        private HashSet<(int, int)> boundaryEdges = new HashSet<(int, int)>();
        private SortedList<float, List<(int, int)>> edgeQueue = new SortedList<float, List<(int, int)>>();
        private HashSet<(int, int)> sharpEdges = new HashSet<(int, int)>();


        public void Simplify(int targetFaceCount)
        {
            float sharpAngleThreshold = 30f;
            IdentifyBoundaryEdges();
            ComputeQuadrics();
            EnqueueAllEdges();
            IdentifySharpEdges(sharpAngleThreshold);


            while (Faces.Count > targetFaceCount)
            {
                var (success, v1, v2) = DequeueEdge();
                if (!success) break;
                if (IsProtectedEdge(v1, v2) || IsBoundaryEdge(v1, v2)) continue;

              //  if (IsBoundaryEdge(v1, v2)) continue;
                CollapseEdge(v1, v2);
            }
        }
        
        private void IdentifySharpEdges(float angleThresholdDegrees = 30f) {
            var edgeToFaces = new Dictionary<(int, int), List<int>>();

            for (int i = 0; i < Faces.Count; i++) {
                var face = Faces[i];
                for (int j = 0; j < 3; j++) {
                    int a = face.Vertices[j];
                    int b = face.Vertices[(j + 1) % 3];
                    var edge = (Math.Min(a, b), Math.Max(a, b));
                    if (!edgeToFaces.ContainsKey(edge))
                        edgeToFaces[edge] = new List<int>();
                    edgeToFaces[edge].Add(i);
                }
            }

            foreach (var kvp in edgeToFaces) {
                var faces = kvp.Value;
                if (faces.Count != 2) continue;

                var n1 = ComputeFaceNormal(Faces[faces[0]]);
                var n2 = ComputeFaceNormal(Faces[faces[1]]);
                double angle = Math.Acos(Clamp(Vector3.Dot(n1, n2), -1f, 1f)) * (180f / (float)Math.PI);

                if (angle > angleThresholdDegrees) {
                    sharpEdges.Add(kvp.Key);
                }
            }
        }
        
        private bool IsProtectedEdge(int v1, int v2) {
            var edge = (Math.Min(v1, v2), Math.Max(v1, v2));
            return boundaryEdges.Contains(edge) || sharpEdges.Contains(edge);
        }

        
        public static double Clamp(double value, double min, double max)
        {
            // Ensure the value is not less than the minimum
            if (value < min)
            {
                return min;
            }
            // Ensure the value is not greater than the maximum
            else if (value > max)
            {
                return max;
            }
            // Otherwise, return the original value
            else
            {
                return value;
            }
        }

        private Vector3 ComputeFaceNormal(Face face) {
            var v0 = Vertices[face.Vertices[0]].Position;
            var v1 = Vertices[face.Vertices[1]].Position;
            var v2 = Vertices[face.Vertices[2]].Position;
            return Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
        }


        private void ComputeQuadrics()
        {
            foreach (var face in Faces)
            {
                var v0 = Vertices[face.Vertices[0]].Position;
                var v1 = Vertices[face.Vertices[1]].Position;
                var v2 = Vertices[face.Vertices[2]].Position;

                var normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
                float d = -Vector3.Dot(normal, v0);
                var plane = new Vector4(normal, d);
                var q = OuterProduct(plane, plane);

                foreach (var vi in face.Vertices)
                {
                    Vertices[vi].Quadric += q;
                }
            }
        }

        private void IdentifyBoundaryEdges()
        {
            var edgeCount = new Dictionary<(int, int), int>();

            foreach (var face in Faces)
            {
                for (int i = 0; i < 3; i++)
                {
                    int a = face.Vertices[i];
                    int b = face.Vertices[(i + 1) % 3];
                    var edge = (Math.Min(a, b), Math.Max(a, b));
                    if (!edgeCount.ContainsKey(edge))
                        edgeCount[edge] = 0;
                    edgeCount[edge]++;
                }
            }

            foreach (var kvp in edgeCount)
            {
                if (kvp.Value == 1) boundaryEdges.Add(kvp.Key);
            }
        }

        private bool IsBoundaryEdge(int v1, int v2)
        {
            var edge = (Math.Min(v1, v2), Math.Max(v1, v2));
            return boundaryEdges.Contains(edge);
        }

        private void EnqueueAllEdges()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                foreach (int j in GetAdjacentVertices(i))
                {
                    float cost = ComputeCollapseCost(i, j);
                    EnqueueEdge(i, j, cost);
                }
            }
        }

        private void EnqueueEdge(int v1, int v2, float cost)
        {
            if (!edgeQueue.TryGetValue(cost, out var list))
            {
                list = new List<(int, int)>();
                edgeQueue[cost] = list;
            }

            list.Add((v1, v2));
        }

        private (bool, int, int) DequeueEdge()
        {
            if (edgeQueue.Count == 0) return (false, -1, -1);
            var first = edgeQueue.Keys[0];
            var pairList = edgeQueue[first];
            var pair = pairList[0];
            pairList.RemoveAt(0);
            if (pairList.Count == 0) edgeQueue.Remove(first);
            return (true, pair.Item1, pair.Item2);
        }

        private float ComputeCollapseCost(int v1, int v2)
        {
            var q = Vertices[v1].Quadric + Vertices[v2].Quadric;
            var pos = (Vertices[v1].Position + Vertices[v2].Position) / 2;
            var v = new Vector4(pos, 1);
            return Vector4.Dot(v, Vector4.Transform(v, q));
        }

        private void CollapseEdge(int v1, int v2)
        {
            var newPos = (Vertices[v1].Position + Vertices[v2].Position) / 2;
            Vertices[v1].Position = newPos;
            Vertices[v1].Quadric += Vertices[v2].Quadric;

            for (int i = Faces.Count - 1; i >= 0; i--)
            {
                var face = Faces[i];
                for (int j = 0; j < 3; j++)
                {
                    if (face.Vertices[j] == v2) face.Vertices[j] = v1;
                }

                if (face.Vertices[0] == face.Vertices[1] ||
                    face.Vertices[1] == face.Vertices[2] ||
                    face.Vertices[2] == face.Vertices[0])
                {
                    Faces.RemoveAt(i);
                }
            }
        }

        private IEnumerable<int> GetAdjacentVertices(int index)
        {
            var adj = new HashSet<int>();
            foreach (var face in Faces)
            {
                if (Array.Exists(face.Vertices, v => v == index))
                {
                    foreach (var v in face.Vertices)
                    {
                        if (v != index) adj.Add(v);
                    }
                }
            }

            return adj;
        }

        private Matrix4x4 OuterProduct(Vector4 a, Vector4 b)
        {
            return new Matrix4x4(
                a.X * b.X, a.X * b.Y, a.X * b.Z, a.X * b.W,
                a.Y * b.X, a.Y * b.Y, a.Y * b.Z, a.Y * b.W,
                a.Z * b.X, a.Z * b.Y, a.Z * b.Z, a.Z * b.W,
                a.W * b.X, a.W * b.Y, a.W * b.Z, a.W * b.W
            );
        }

        
      
    }
}