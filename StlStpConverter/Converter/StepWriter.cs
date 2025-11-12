using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Bolsover.Converter
{
    public class StepWriter
    {
        /// <summary>
        /// List of STEP entities.
        /// </summary>
        private List<Entity> Entities { get; } = new();


       /// <summary>
       /// Calculate direction vector from two points.
       /// </summary>
       /// <param name="from"></param>
       /// <param name="to"></param>
       /// <returns></returns>
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

       /// <summary>
       /// Normalize a vector.
       /// </summary>
       /// <param name="vector"></param>
        private static void NormalizeVector(double[] vector)
        {
            var length = Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
            if (!(length > 0)) return;
            for (var i = 0; i < 3; i++)
                vector[i] /= length;
        }


        /// <summary>
        /// Create an EdgeCurve from two vertices.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="dir"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        private EdgeCurve CreateEdgeCurve(Vertex vertex1, Vertex vertex2, bool dir, double tol)
        {
            var linePoint1 = GetOrCreatePoint(_pointCache, vertex1.Point.X, vertex1.Point.Y, vertex1.Point.Z, tol);

            var (vx, vy, vz, _) = CalculateDirection(
                new[] { vertex1.Point.X, vertex1.Point.Y, vertex1.Point.Z },
                new[] { vertex2.Point.X, vertex2.Point.Y, vertex2.Point.Z }
            );

            var lineDir1 = GetOrCreateDirection(_directionCache, vx, vy, vz, tol);
            var lineVector1 = new Vector(Entities, lineDir1, 1.0);
            var line1 = new Line(Entities, linePoint1, lineVector1);
            var surfCurve1 = new SurfaceCurve(Entities, line1);
            return new EdgeCurve(Entities, vertex1, vertex2, surfCurve1, dir);
        }

        /// <summary>
        /// Caches for STEP entities.
        /// </summary>
        private Dictionary<DirectionKey, Direction> _directionCache = new();
        private Dictionary<PointKey, Point> _pointCache = new();
        private Dictionary<PointKey, Vertex> _vertexCache = new();
        private Dictionary<EdgeKey, EdgeCurve> _edgeCache = new();

        /// <summary>
        /// Build a triangular body from a list of triangles
        /// </summary>
        /// <param name="tris"></param>
        /// <param name="tol"></param>
        /// <param name="mergedEdgeCount"></param>

        public void BuildTriBody(List<double> tris, double tol, ref int mergedEdgeCount)
        {
            _directionCache = new Dictionary<DirectionKey, Direction>();
            _pointCache = new Dictionary<PointKey, Point>();
            _vertexCache = new Dictionary<PointKey, Vertex>(capacity: tris.Count / 3);
            _edgeCache = new Dictionary<EdgeKey, EdgeCurve>(capacity: tris.Count);

            var originPoint = GetOrCreatePoint(_pointCache, 0.0, 0.0, 0.0, tol);
            var direction1 = GetOrCreateDirection(_directionCache, 0.0, 0.0, 1.0, tol);
            var direction2 = GetOrCreateDirection(_directionCache, 1.0, 0.0, 0.0, tol);
            var baseCsys = new Csys3D(Entities, direction1, direction2, originPoint);
            var faces = new List<Face>();

            for (var i = 0; i < tris.Count / 9; i++)
            {
                double[] p0 = { tris[i * 9 + 0], tris[i * 9 + 1], tris[i * 9 + 2] };
                double[] p1 = { tris[i * 9 + 3], tris[i * 9 + 4], tris[i * 9 + 5] };
                double[] p2 = { tris[i * 9 + 6], tris[i * 9 + 7], tris[i * 9 + 8] };

                // Compute directions
                var (d0X, d0Y, d0Z, dist0) = CalculateDirection(p0, p1);
                if (dist0 < tol) continue;
                double[] d0 = { d0X, d0Y, d0Z };

                var (d1X, d1Y, d1Z, dist1) = CalculateDirection(p0, p2);
                if (dist1 < tol) continue;
                double[] d1 = { d1X, d1Y, d1Z };

                // Cross-product for normal
                var d2 = CrossProduct(d0, d1);
                NormalizeVector(d2);

                var vert1 = GetOrCreateVertex(_vertexCache, p0[0], p0[1], p0[2], tol);
                var vert2 = GetOrCreateVertex(_vertexCache, p1[0], p1[1], p1[2], tol);
                var vert3 = GetOrCreateVertex(_vertexCache, p2[0], p2[1], p2[2], tol);

                var edgeCurve1 = GetOrCreateEdge(_edgeCache, vert1, vert2, tol, out var edgeDir1, ref mergedEdgeCount);
                var edgeCurve2 = GetOrCreateEdge(_edgeCache, vert2, vert3, tol, out var edgeDir2, ref mergedEdgeCount);
                var edgeCurve3 = GetOrCreateEdge(_edgeCache, vert3, vert1, tol, out var edgeDir3, ref mergedEdgeCount);

                var orientedEdges = new List<OrientedEdge>
                {
                    new(Entities, edgeCurve1, edgeDir1),
                    new(Entities, edgeCurve2, edgeDir2),
                    new(Entities, edgeCurve3, edgeDir3)
                };

                // Plane and csys
                var planePoint = GetOrCreatePoint(_pointCache, p0[0], p0[1], p0[2], tol);
                var planeDir1 = GetOrCreateDirection(_directionCache, d2[0], d2[1], d2[2], tol);
                var planeDir2 = GetOrCreateDirection(_directionCache, d0[0], d0[1], d0[2], tol);
                var planeCsys = new Csys3D(Entities, planeDir1, planeDir2, planePoint);
                var plane = new Plane(Entities, planeCsys);
                var edgeLoop = new EdgeLoop(Entities, orientedEdges);
                var faceBounds = new List<FaceBound> { new(Entities, edgeLoop, true) };
                faces.Add(new Face(Entities, faceBounds, plane, true));
            }

            var shell = new Shell(Entities, faces);
            var shells = new List<Shell> { shell };
            var shellModel = new ShellModel(Entities, shells);
            // ReSharper disable once ObjectCreationAsStatement
            new ManifoldShape(Entities, baseCsys, shellModel);
        }

        /// <summary>
        /// Get an existing direction or create a new one.
        /// </summary>
        /// <param name="directionCache"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        private Direction GetOrCreateDirection(Dictionary<DirectionKey, Direction> directionCache, double x, double y,
            double z, double tol)
        {
            var pk = new DirectionKey(x, y, z, tol);
            if (directionCache.TryGetValue(pk, out var d)) return d;
            // Create a single unique Direction
            d = new Direction(Entities, x, y, z);
            directionCache.Add(pk, d);

            return d;
        }

        /// <summary>
        /// Get an existing point or create a new one.
        /// </summary>
        /// <param name="pointCache"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        private Point GetOrCreatePoint(Dictionary<PointKey, Point> pointCache, double x, double y, double z, double tol)
        {
            var pk = new PointKey(x, y, z, tol);
            if (pointCache.TryGetValue(pk, out var p)) return p;
            // Create a single Point instance per unique position
            p = new Point(Entities, x, y, z);
            pointCache.Add(pk, p);
            return p;
        }

        /// <summary>
        /// Get an existing vertex or create a new one.
        /// </summary>
        /// <param name="vertexCache"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        private Vertex GetOrCreateVertex(Dictionary<PointKey, Vertex> vertexCache, double x, double y, double z,
            double tol)
        {
            var pk = new PointKey(x, y, z, tol);
            if (vertexCache.TryGetValue(pk, out var v)) return v;
            // Create a single Point instance per unique position
            var p = GetOrCreatePoint(_pointCache, x, y, z, tol);
            v = new Vertex(Entities, p);
            vertexCache.Add(pk, v);
            return v;
        }


        /// <summary>
        /// Get an existing edge curve or create a new one.
        /// </summary>
        /// <param name="edgeCache"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="tol"></param>
        /// <param name="edgeDir"></param>
        /// <param name="mergeCount"></param>
        /// <returns></returns>
        private EdgeCurve GetOrCreateEdge(Dictionary<EdgeKey, EdgeCurve> edgeCache, Vertex v1, Vertex v2, double tol,
            out bool edgeDir, ref int mergeCount)
        {
            var pk1 = new PointKey(v1.Point.X, v1.Point.Y, v1.Point.Z, tol);
            var pk2 = new PointKey(v2.Point.X, v2.Point.Y, v2.Point.Z, tol);

            var keyF = new EdgeKey(pk1, pk2); // forward
            if (edgeCache.TryGetValue(keyF, out var eF))
            {
                edgeDir = true; // traverse in stored direction
                mergeCount++;
                return eF;
            }

            var keyR = new EdgeKey(pk2, pk1); // reverse
            if (edgeCache.TryGetValue(keyR, out var eR))
            {
                edgeDir = false; // triangle edge is opposite to stored direction
                mergeCount++;
                return eR;
            }

            // Not found — create and store once (store as forward by convention)
            var ec = CreateEdgeCurve(v1, v2, dir: true, tol);
            edgeCache.Add(keyF, ec);
            edgeDir = true;
            return ec;
        }


        /// <summary>
        /// Write STEP file to disk
        /// </summary>
        /// <param name="fileName"></param>
        public void WriteStep(string fileName)
        {
            var isoTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            using var writer = new StreamWriter(fileName, false, new UTF8Encoding(false), 1 << 16);
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
        }
    }

   
    internal readonly struct PointKey : IEquatable<PointKey>
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _z;

        public PointKey(double x, double y, double z, double tol)
        {
            long Q(double v) => (long)Math.Round(v / tol);
            _x = Q(x);
            _y = Q(y);
            _z = Q(z);
        }

        public bool Equals(PointKey other) => _x == other._x && _y == other._y && _z == other._z;
        public override bool Equals(object o) => o is PointKey k && Equals(k);
        public override int GetHashCode() => HashCode.Combine(_x, _y, _z);
    }

    internal readonly struct EdgeKey : IEquatable<EdgeKey>
    {
        // Directed edge key; use unordered for lookups by trying reversed too
        private readonly PointKey _a;
        private readonly PointKey _b;

        public EdgeKey(in PointKey a, in PointKey b)
        {
            _a = a;
            _b = b;
        }

        public bool Equals(EdgeKey other) => _a.Equals(other._a) && _b.Equals(other._b);
        public override bool Equals(object o) => o is EdgeKey k && Equals(k);
        public override int GetHashCode() => HashCode.Combine(_a, _b);
    }

    internal readonly struct DirectionKey : IEquatable<DirectionKey>
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _z;

        public DirectionKey(double x, double y, double z, double tol)
        {
            long Q(double v) => (long)Math.Round(v / tol);
            _x = Q(x);
            _y = Q(y);
            _z = Q(z);
        }

        public bool Equals(DirectionKey other) => _x == other._x && _y == other._y && _z == other._z;
        public override bool Equals(object o) => o is DirectionKey k && Equals(k);
        public override int GetHashCode() => HashCode.Combine(_x, _y, _z);
    }
}