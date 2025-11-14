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
        private List<IEntity> Entities { get; } = new();

        // ID assignment and registration
        private int _nextId = 1;
        private int NextId() => _nextId++;

        private T Register<T>(T e) where T : IEntity
        {
            Entities.Add(e);
            return e;
        }

        // Number of coordinate values per triangle (3 vertices × 3 coordinates).
        private const int ValuesPerTriangle = 9;


        // Caches for STEP entities.
        private Dictionary<DirectionKey, Direction> _directionCache = new();
        private Dictionary<CartesianPointKey, CartesianPoint> _pointCache = new();
        private Dictionary<CartesianPointKey, Vertex> _vertexCache = new();
        private Dictionary<EdgeKey, EdgeCurve> _edgeCache = new();

        /// <summary>
        /// Calculate the direction vector from two points.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private static (double unitX, double unitY, double unitZ, double distance) CalculateUnitDirectionAndDistance(
            double[] from, double[] to)
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

       /// <summary>
       /// Calculate the cross product of two vectors.
       /// </summary>
       /// <param name="vector1"></param>
       /// <param name="vector2"></param>
       /// <returns></returns>
        private static double[] CrossProduct(double[] vector1, double[] vector2)
        {
            return new[]
            {
                vector1[1] * vector2[2] - vector1[2] * vector2[1],
                vector1[2] * vector2[0] - vector1[0] * vector2[2],
                vector1[0] * vector2[1] - vector1[1] * vector2[0]
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
        /// <param name="isForward"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private EdgeCurve CreateEdgeCurve(Vertex vertex1, Vertex vertex2, bool isForward, double tolerance)
        {
            var linePoint1 = GetOrCreatePoint(_pointCache, vertex1.CartesianPoint.X, vertex1.CartesianPoint.Y,
                vertex1.CartesianPoint.Z, tolerance);

            var (vx, vy, vz, _) = CalculateUnitDirectionAndDistance(
                new[] { vertex1.CartesianPoint.X, vertex1.CartesianPoint.Y, vertex1.CartesianPoint.Z },
                new[] { vertex2.CartesianPoint.X, vertex2.CartesianPoint.Y, vertex2.CartesianPoint.Z }
            );

            var lineDir1 = GetOrCreateDirection(_directionCache, vx, vy, vz, tolerance);
            var lineVector1 = Register(new Vector(NextId(), lineDir1, 1.0));
            var line1 = Register(new Line(NextId(), linePoint1, lineVector1));
            var surfCurve1 = Register(new SurfaceCurve(NextId(), line1));
            return Register(new EdgeCurve(NextId(), vertex1, vertex2, surfCurve1, isForward));
        }

        
        /// <summary>
        /// Try to compute a valid normalized triangle and reference direction.
        /// Returns false when the triangle is degenerate w.r.t. the given tolerance.
        /// </summary>
        private static (bool hasValidNormal, double[] normal, double[] referenceDirection) TryComputeTriangleNormal(
            double[] p0,
            double[] p1,
            double[] p2,
            double tolerance)
        {
            var (d0X, d0Y, d0Z, dist0) = CalculateUnitDirectionAndDistance(p0, p1);
            if (dist0 < tolerance)
            {
                return (false, Array.Empty<double>(), Array.Empty<double>());
            }

            var (d1X, d1Y, d1Z, dist1) = CalculateUnitDirectionAndDistance(p0, p2);
            if (dist1 < tolerance)
            {
                return (false, Array.Empty<double>(), Array.Empty<double>());
            }

            double[] referenceDirection = { d0X, d0Y, d0Z };
            double[] d1 = { d1X, d1Y, d1Z };

            var normal = CrossProduct(referenceDirection, d1);
            NormalizeVector(normal);

            return (true, normal, referenceDirection);
        }

        /// <summary>
        /// Build a body from a list of triangles
        /// </summary>
        /// <param name="triangleList"></param>
        /// <param name="tolerance"></param>
        /// <param name="mergedEdgeCount"></param>
        public void BuildTriangularBody(List<double> triangleList, double tolerance, ref int mergedEdgeCount)
        {
            var triangleCount = triangleList.Count / ValuesPerTriangle;

            _directionCache = new Dictionary<DirectionKey, Direction>();
            _pointCache = new Dictionary<CartesianPointKey, CartesianPoint>();
            _vertexCache = new Dictionary<CartesianPointKey, Vertex>(capacity: triangleCount * 3);
            _edgeCache = new Dictionary<EdgeKey, EdgeCurve>(capacity: triangleCount * 3);

            var originPoint = GetOrCreatePoint(_pointCache, 0.0, 0.0, 0.0, tolerance);
            var direction1 = GetOrCreateDirection(_directionCache, 0.0, 0.0, 1.0, tolerance);
            var direction2 = GetOrCreateDirection(_directionCache, 1.0, 0.0, 0.0, tolerance);
            var axisPlacement3D = Register(new AxisPlacement3D(NextId(), direction1, direction2, originPoint));
            var faces = new List<Face>();

            for (var i = 0; i < triangleCount; i++)
            {
                GetTrianglePoints(triangleList, i, out var p0, out var p1, out var p2);

                var (hasValidNormal, normal, referenceDirection) =
                    TryComputeTriangleNormal(p0, p1, p2, tolerance);

                if (!hasValidNormal)
                    continue;

                var vert1 = GetOrCreateVertex(_vertexCache, p0[0], p0[1], p0[2], tolerance);
                var vert2 = GetOrCreateVertex(_vertexCache, p1[0], p1[1], p1[2], tolerance);
                var vert3 = GetOrCreateVertex(_vertexCache, p2[0], p2[1], p2[2], tolerance);

                var edgeCurve1 = GetOrCreateEdge(_edgeCache, vert1, vert2, tolerance, out var edgeDir1,
                    ref mergedEdgeCount);
                var edgeCurve2 = GetOrCreateEdge(_edgeCache, vert2, vert3, tolerance, out var edgeDir2,
                    ref mergedEdgeCount);
                var edgeCurve3 = GetOrCreateEdge(_edgeCache, vert3, vert1, tolerance, out var edgeDir3,
                    ref mergedEdgeCount);

                var orientedEdges = new List<OrientedEdge>
                {
                    Register(new OrientedEdge(NextId(), edgeCurve1, edgeDir1)),
                    Register(new OrientedEdge(NextId(), edgeCurve2, edgeDir2)),
                    Register(new OrientedEdge(NextId(), edgeCurve3, edgeDir3))
                };

                
                var plane = CreatePlaneForTriangle(p0, normal, referenceDirection, tolerance);
                var edgeLoop = Register(new EdgeLoop(NextId(), orientedEdges));
                var faceBounds = new List<FaceBound> { Register(new FaceBound(NextId(), edgeLoop, true)) };
                faces.Add(Register(new Face(NextId(), faceBounds, plane, true)));
            }

            var shell = Register(new Shell(NextId(), faces));
            var shells = new List<Shell> { shell };
            var shellModel = Register(new ShellModel(NextId(), shells));
            // ReSharper disable once ObjectCreationAsStatement
            Register(new ManifoldShape(NextId(), axisPlacement3D, shellModel));
        }
        
        /// <summary>
        /// Create a plane for a triangle.
        /// </summary>
        /// <param name="triangleOrigin"></param>
        /// <param name="triangleNormal"></param>
        /// <param name="referenceDirection"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private Plane CreatePlaneForTriangle(double[] triangleOrigin, double[] triangleNormal, double[] referenceDirection, double tolerance)
        {
            var planePoint = GetOrCreatePoint(
                _pointCache,
                triangleOrigin[0],
                triangleOrigin[1],
                triangleOrigin[2],
                tolerance);

            var planeDirNormal = GetOrCreateDirection(
                _directionCache,
                triangleNormal[0],
                triangleNormal[1],
                triangleNormal[2],
                tolerance);

            var planeDirReference = GetOrCreateDirection(
                _directionCache,
                referenceDirection[0],
                referenceDirection[1],
                referenceDirection[2],
                tolerance);

            var axisPlacementIn = Register(new AxisPlacement3D(NextId(), planeDirNormal, planeDirReference, planePoint));
            return Register(new Plane(NextId(), axisPlacementIn));
        }


        /// <summary>
        /// Helper to read three points (a triangle) from a flat coordinate list.
        /// </summary>
        private static void GetTrianglePoints(List<double> triangleList, int triangleIndex,
            out double[] p0, out double[] p1, out double[] p2)
        {
            var baseIndex = triangleIndex * ValuesPerTriangle;

            p0 = new[]
            {
                triangleList[baseIndex + 0],
                triangleList[baseIndex + 1],
                triangleList[baseIndex + 2]
            };
            p1 = new[]
            {
                triangleList[baseIndex + 3],
                triangleList[baseIndex + 4],
                triangleList[baseIndex + 5]
            };
            p2 = new[]
            {
                triangleList[baseIndex + 6],
                triangleList[baseIndex + 7],
                triangleList[baseIndex + 8]
            };
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
            d = Register(new Direction(NextId(), x, y, z));
            directionCache.Add(pk, d);

            return d;
        }

        /// <summary>
        /// Get an existing cartesianPoint or create a new one.
        /// </summary>
        /// <param name="pointCache"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        private CartesianPoint GetOrCreatePoint(Dictionary<CartesianPointKey, CartesianPoint> pointCache, double x,
            double y, double z, double tol)
        {
            var pk = new CartesianPointKey(x, y, z, tol);
            if (pointCache.TryGetValue(pk, out var p)) return p;
            // Create a single CartesianPoint instance per unique position
            p = Register(new CartesianPoint(NextId(), x, y, z));
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
        private Vertex GetOrCreateVertex(Dictionary<CartesianPointKey, Vertex> vertexCache, double x, double y,
            double z,
            double tol)
        {
            var pk = new CartesianPointKey(x, y, z, tol);
            if (vertexCache.TryGetValue(pk, out var v)) return v;
            // Create a single CartesianPoint instance per unique position
            var p = GetOrCreatePoint(_pointCache, x, y, z, tol);
            v = Register(new Vertex(NextId(), p));
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
            var pk1 = new CartesianPointKey(v1.CartesianPoint.X, v1.CartesianPoint.Y, v1.CartesianPoint.Z, tol);
            var pk2 = new CartesianPointKey(v2.CartesianPoint.X, v2.CartesianPoint.Y, v2.CartesianPoint.Z, tol);

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
            var ec = CreateEdgeCurve(v1, v2, isForward: true, tol);
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


    internal readonly struct CartesianPointKey : IEquatable<CartesianPointKey>
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _z;

        public CartesianPointKey(double x, double y, double z, double tol)
        {
            long Q(double v) => (long)Math.Round(v / tol);
            _x = Q(x);
            _y = Q(y);
            _z = Q(z);
        }

        public bool Equals(CartesianPointKey other) => _x == other._x && _y == other._y && _z == other._z;
        public override bool Equals(object o) => o is CartesianPointKey k && Equals(k);
        public override int GetHashCode() => HashCode.Combine(_x, _y, _z);
    }

    internal readonly struct EdgeKey : IEquatable<EdgeKey>
    {
        // Directed edge key; use unordered for lookups by trying reversed too
        private readonly CartesianPointKey _a;
        private readonly CartesianPointKey _b;

        public EdgeKey(in CartesianPointKey a, in CartesianPointKey b)
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