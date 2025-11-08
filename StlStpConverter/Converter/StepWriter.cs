using System;
using System.Collections.Generic;
using System.IO;


namespace Bolsover.Converter
{
    public class StepWriter
    {
        private List<Entity> Entities { get; } = new();

        // Helper: Calculate a normalized direction vector between two points
        private static (double x, double y, double z, double distance) CalculateDirection(double[] from, double[] to)
        {
            var dx = to[0] - from[0];
            var dy = to[1] - from[1];
            var dz = to[2] - from[2];
            var distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (!(distance > 0)) return (dx, dy, dz, distance);
            dx /= distance;
            dy /= distance;
            dz /= distance;

            return (dx, dy, dz, distance);
        }

        // Helper: Calculate cross-product of two vectors
        private static double[] CrossProduct(double[] v1, double[] v2)
        {
            return new[]
            {
                v1[1] * v2[2] - v1[2] * v2[1],
                v1[2] * v2[0] - v1[0] * v2[2],
                v1[0] * v2[1] - v1[1] * v2[0]
            };
        }

        // Helper: Normalize a vector in-place
        private static void NormalizeVector(double[] vector)
        {
            var length = Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
            if (!(length > 0)) return;
            for (var i = 0; i < 3; i++)
                vector[i] /= length;
        }


        private EdgeCurve CreateEdgeCurve(Vertex vert1, Vertex vert2, bool dir)
        {
            var linePoint1 = new Point(Entities, vert1.Point.X, vert1.Point.Y, vert1.Point.Z);

            var (vx, vy, vz, _) = CalculateDirection(
                new[] { vert1.Point.X, vert1.Point.Y, vert1.Point.Z },
                new[] { vert2.Point.X, vert2.Point.Y, vert2.Point.Z }
            );

            var lineDir1 = new Direction(Entities, vx, vy, vz);
            var lineVector1 = new Vector(Entities, lineDir1, 1.0);
            var line1 = new Line(Entities, linePoint1, lineVector1);
            var surfCurve1 = new SurfaceCurve(Entities, line1);
            return new EdgeCurve(Entities, vert1, vert2, surfCurve1, dir);
        }


        public void BuildTriBody(List<double> tris, double tol, ref int mergedEdgeCount)
        {
            var originPoint = new Point(Entities, 0.0, 0.0, 0.0);
            var direction1 = new Direction(Entities, 0.0, 0.0, 1.0);
            var direction2 = new Direction(Entities, 1.0, 0.0, 0.0);
            var baseCsys = new Csys3D(Entities, direction1, direction2, originPoint);
            var faces = new List<Face>();
            var edgeMap = new Dictionary<(double, double, double, double, double, double), EdgeCurve>();

            for (var i = 0; i < tris.Count / 9; i++)
            {
                double[] p0 = { tris[i * 9 + 0], tris[i * 9 + 1], tris[i * 9 + 2] };
                double[] p1 = { tris[i * 9 + 3], tris[i * 9 + 4], tris[i * 9 + 5] };
                double[] p2 = { tris[i * 9 + 6], tris[i * 9 + 7], tris[i * 9 + 8] };

                // Compute directions
                var (d0x, d0y, d0z, dist0) = CalculateDirection(p0, p1);
                if (dist0 < tol) continue;
                double[] d0 = { d0x, d0y, d0z };

                var (d1x, d1y, d1z, dist1) = CalculateDirection(p0, p2);
                if (dist1 < tol) continue;
                double[] d1 = { d1x, d1y, d1z };

                // Cross-product for normal
                var d2 = CrossProduct(d0, d1);
                NormalizeVector(d2);

                if (Math.Sqrt(d2[0] * d2[0] + d2[1] * d2[1] + d2[2] * d2[2]) < tol) continue;

                // Correct d1 to be orthogonal
                var d1Cor = CrossProduct(d2, d0);
                NormalizeVector(d1Cor);
                d1 = d1Cor;

                // Create vertices
                var vert1 = new Vertex(Entities, new Point(Entities, p0[0], p0[1], p0[2]));
                var vert2 = new Vertex(Entities, new Point(Entities, p1[0], p1[1], p1[2]));
                var vert3 = new Vertex(Entities, new Point(Entities, p2[0], p2[1], p2[2]));

                // Get edges
                GetEdgeFromMap(p0, p1, edgeMap, vert1, vert2, out var edgeCurve1, out var edgeDir1,
                    ref mergedEdgeCount);
                GetEdgeFromMap(p1, p2, edgeMap, vert2, vert3, out var edgeCurve2, out var edgeDir2,
                    ref mergedEdgeCount);
                GetEdgeFromMap(p2, p0, edgeMap, vert3, vert1, out var edgeCurve3, out var edgeDir3,
                    ref mergedEdgeCount);

                var orientedEdges = new List<OrientedEdge>
                {
                    new(Entities, edgeCurve1, edgeDir1),
                    new(Entities, edgeCurve2, edgeDir2),
                    new(Entities, edgeCurve3, edgeDir3)
                };

                // Plane and csys
                var planePoint = new Point(Entities, p0[0], p0[1], p0[2]);
                var planeDir1 = new Direction(Entities, d2[0], d2[1], d2[2]);
                var planeDir2 = new Direction(Entities, d0[0], d0[1], d0[2]);
                var planeCsys = new Csys3D(Entities, planeDir1, planeDir2, planePoint);
                var plane = new Plane(Entities, planeCsys);
                var edgeLoop = new EdgeLoop(Entities, orientedEdges);
                var faceBounds = new List<FaceBound> { new(Entities, edgeLoop, true) };
                faces.Add(new Face(Entities, faceBounds, plane, true));
            }

            var shell = new Shell(Entities, faces);
            var shells = new List<Shell> { shell };
            var shellModel = new ShellModel(Entities, shells);
            var manifoldShape = new ManifoldShape(Entities, baseCsys, shellModel);
        }

        // Get edge from map
        private void GetEdgeFromMap(
            double[] p0,
            double[] p1,
            Dictionary<(double, double, double, double, double, double), EdgeCurve> edgeMap,
            Vertex vert1,
            Vertex vert2,
            out EdgeCurve edgeCurve,
            out bool edgeDir,
            ref int mergeCount)
        {
            edgeCurve = null;
            edgeDir = true;
            var keyForward = (p0[0], p0[1], p0[2], p1[0], p1[1], p1[2]);
            var keyReverse = (p1[0], p1[1], p1[2], p0[0], p0[1], p0[2]);

            if (edgeMap.TryGetValue(keyForward, out var value1))
            {
                edgeCurve = value1;
                edgeDir = true;
                mergeCount++;
            }
            else if (edgeMap.TryGetValue(keyReverse, out var value))
            {
                edgeCurve = value;
                edgeDir = false;
                mergeCount++;
            }

            if (edgeCurve != null) return;
            edgeCurve = CreateEdgeCurve(vert1, vert2, true);
            edgeMap[keyForward] = edgeCurve;
        }


        public void WriteStep(string fileName)
        {
            var isoTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            using var writer = new StreamWriter(fileName);
            writer.WriteLine("ISO-10303-21;");
            writer.WriteLine("HEADER;");
            writer.WriteLine("/*Generated by StlStepConverter*/");
            writer.WriteLine("FILE_DESCRIPTION(('STP203'),'2;1');");
            writer.WriteLine(
                $"FILE_NAME('{fileName}','{isoTime}',('David Bolsover'),('bolsover.com'),' ','StlStepConverter',' ');");
            writer.WriteLine("FILE_SCHEMA(('CONFIG_CONTROL_DESIGN'));");
            writer.WriteLine("ENDSEC;");
            writer.WriteLine("DATA;");
            foreach (var e in Entities)
            {
                e.Serialize(writer);
            }

            writer.WriteLine("ENDSEC;");
            writer.WriteLine("END-ISO-10303-21;");
            writer.Flush();
            writer.Close();
        }
    }
}