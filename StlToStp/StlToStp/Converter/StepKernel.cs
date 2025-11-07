using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bolsover.StlToStp.Converter
{
 
public class StepKernel
{
    public List<Entity> Entities { get; private set; } = new List<Entity>();

    // Constructor
    public StepKernel()
    {
    }
    

    // Create EdgeCurve
    public EdgeCurve CreateEdgeCurve(Vertex vert1, Vertex vert2, bool dir)
    {
        // Create starting point
        var linePoint1 = new Point(Entities, vert1.Point.X, vert1.Point.Y, vert1.Point.Z);

        // Compute direction vector
        double vx = vert2.Point.X - vert1.Point.X;
        double vy = vert2.Point.Y - vert1.Point.Y;
        double vz = vert2.Point.Z - vert1.Point.Z;
        double dist = Math.Sqrt(vx * vx + vy * vy + vz * vz);

        vx /= dist;
        vy /= dist;
        vz /= dist;

        var lineDir1 = new Direction(Entities, vx, vy, vz);
        var lineVector1 = new Vector(Entities, lineDir1, 1.0);
        var line1 = new Line(Entities, linePoint1, lineVector1);
        var surfCurve1 = new SurfaceCurve(Entities, line1);

        return new EdgeCurve(Entities, vert1, vert2, surfCurve1, dir);
    }

    // Build triangular body
    public void BuildTriBody(List<double> tris, double tol, ref int mergedEdgeCount)
    {
        var originPoint = new Point(Entities, 0.0, 0.0, 0.0);
        var dir1 = new Direction(Entities, 0.0, 0.0, 1.0);
        var dir2 = new Direction(Entities, 1.0, 0.0, 0.0);
        var baseCsys = new Csys3D(Entities, dir1, dir2, originPoint);

        var faces = new List<Face>();
        var edgeMap = new Dictionary<(double, double, double, double, double, double), EdgeCurve>();

        for (int i = 0; i < tris.Count / 9; i++)
        {
            double[] p0 = { tris[i * 9 + 0], tris[i * 9 + 1], tris[i * 9 + 2] };
            double[] p1 = { tris[i * 9 + 3], tris[i * 9 + 4], tris[i * 9 + 5] };
            double[] p2 = { tris[i * 9 + 6], tris[i * 9 + 7], tris[i * 9 + 8] };

            // Compute directions
            double[] d0 = { p1[0] - p0[0], p1[1] - p0[1], p1[2] - p0[2] };
            double dist0 = Math.Sqrt(d0[0] * d0[0] + d0[1] * d0[1] + d0[2] * d0[2]);
            if (dist0 < tol) continue;
            for (int j = 0; j < 3; j++) d0[j] /= dist0;

            double[] d1 = { p2[0] - p0[0], p2[1] - p0[1], p2[2] - p0[2] };
            double dist1 = Math.Sqrt(d1[0] * d1[0] + d1[1] * d1[1] + d1[2] * d1[2]);
            if (dist1 < tol) continue;
            for (int j = 0; j < 3; j++) d1[j] /= dist1;

            // Cross product for normal
            double[] d2 = {
                d0[1] * d1[2] - d0[2] * d1[1],
                d0[2] * d1[0] - d0[0] * d1[2],
                d0[0] * d1[1] - d0[1] * d1[0]
            };
            double dist2 = Math.Sqrt(d2[0] * d2[0] + d2[1] * d2[1] + d2[2] * d2[2]);
            if (dist2 < tol) continue;
            for (int j = 0; j < 3; j++) d2[j] /= dist2;

            // Correct d1
            double[] d1Cor = {
                d2[1] * d0[2] - d2[2] * d0[1],
                d2[2] * d0[0] - d2[0] * d0[2],
                d2[0] * d0[1] - d2[1] * d0[0]
            };
            double d1CorLen = Math.Sqrt(d1Cor[0] * d1Cor[0] + d1Cor[1] * d1Cor[1] + d1Cor[2] * d1Cor[2]);
            for (int j = 0; j < 3; j++) d1[j] = d1Cor[j] / d1CorLen;

            // Create vertices
            var vert1 = new Vertex(Entities, new Point(Entities, p0[0], p0[1], p0[2]));
            var vert2 = new Vertex(Entities, new Point(Entities, p1[0], p1[1], p1[2]));
            var vert3 = new Vertex(Entities, new Point(Entities, p2[0], p2[1], p2[2]));

            // Get edges
            EdgeCurve edgeCurve1, edgeCurve2, edgeCurve3;
            bool edgeDir1, edgeDir2, edgeDir3;
            GetEdgeFromMap(p0, p1, edgeMap, vert1, vert2, out edgeCurve1, out edgeDir1, ref mergedEdgeCount);
            GetEdgeFromMap(p1, p2, edgeMap, vert2, vert3, out edgeCurve2, out edgeDir2, ref mergedEdgeCount);
            GetEdgeFromMap(p2, p0, edgeMap, vert3, vert1, out edgeCurve3, out edgeDir3, ref mergedEdgeCount);

            var orientedEdges = new List<OrientedEdge>
            {
                new OrientedEdge(Entities, edgeCurve1, edgeDir1),
                new OrientedEdge(Entities, edgeCurve2, edgeDir2),
                new OrientedEdge(Entities, edgeCurve3, edgeDir3)
            };

            // Plane and csys
            var planePoint = new Point(Entities, p0[0], p0[1], p0[2]);
            var planeDir1 = new Direction(Entities, d2[0], d2[1], d2[2]);
            var planeDir2 = new Direction(Entities, d0[0], d0[1], d0[2]);
            var planeCsys = new Csys3D(Entities, planeDir1, planeDir2, planePoint);
            var plane = new Plane(Entities, planeCsys);

            var edgeLoop = new EdgeLoop(Entities, orientedEdges);
            var faceBounds = new List<FaceBound> { new FaceBound(Entities, edgeLoop, true) };
            faces.Add(new Face(Entities, faceBounds, plane, true));
        }

        var shell = new Shell(Entities, faces);
        var shells = new List<Shell> { shell };
        var shellModel = new ShellModel(Entities, shells);
        var manifoldShape = new ManifoldShape(Entities, baseCsys, shellModel);
    }

    // Get edge from map
    public void GetEdgeFromMap(
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

        if (edgeMap.ContainsKey(keyForward))
        {
            edgeCurve = edgeMap[keyForward];
            edgeDir = true;
            mergeCount++;
        }
        else if (edgeMap.ContainsKey(keyReverse))
        {
            edgeCurve = edgeMap[keyReverse];
            edgeDir = false;
            mergeCount++;
        }

        if (edgeCurve == null)
        {
            edgeCurve = CreateEdgeCurve(vert1, vert2, true);
            edgeMap[keyForward] = edgeCurve;
        }
    }

    // Write STEP file
    public void WriteStep(string fileName)
    {
        var isoTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine("ISO-10303-21;");
        writer.WriteLine("HEADER;");
        writer.WriteLine("FILE_DESCRIPTION(('STP203'),'2;1');");
        writer.WriteLine($"FILE_NAME('{fileName}','{isoTime}',('slugdev'),('org'),' ','stltostp',' ');");
        writer.WriteLine("FILE_SCHEMA(('CONFIG_CONTROL_DESIGN'));");
        writer.WriteLine("ENDSEC;");
        writer.WriteLine("DATA;");
        foreach (var e in Entities)
        {
            //  Console.WriteLine(e);
            e.Serialize(writer);
        }

        writer.WriteLine("ENDSEC;");
        writer.WriteLine("END-ISO-10303-21;");
        writer.Flush();
        writer.Close();
    }

    // Read line from STEP file
    public string ReadLine(StreamReader stpFile, bool skipAllSpace)
    {
        var lineStr = string.Empty;
        bool leadingSpace = true;
        while (!stpFile.EndOfStream)
        {
            char ch = (char)stpFile.Read();
            if (ch == ';') break;
            if (ch == '\n' || ch == '\r' || ch == '\t') continue;
            if (leadingSpace && (ch == ' ' || ch == '\t')) continue;
            if (!skipAllSpace) leadingSpace = false;
            lineStr += ch;
        }
        return lineStr;
    }

public void ReadStep(string fileName)
{
    if (!File.Exists(fileName))
        return;

    using var stpFile = new StreamReader(fileName);
    // Read first line (ISO header)
    string isoLine = ReadLine(stpFile, true);

    bool dataSection = false;
    var ents = new List<Entity>();
    var entMap = new Dictionary<int, Entity>();
    var args = new List<string>();

    while (!stpFile.EndOfStream)
    {
        string curStr = ReadLine(stpFile, false);

        if (curStr == "DATA")
        {
            dataSection = true;
            continue;
        }
        if (!dataSection)
            continue;

        if (curStr == "ENDSEC")
        {
            dataSection = false;
            break;
        }

        // Parse entity line
        int id = -1;
        if (curStr.Length > 0 && curStr[0] == '#' && curStr.Contains("="))
        {
            int equalPos = curStr.IndexOf('=');
            int parenPos = curStr.IndexOf('(');
            string idStr = curStr.Substring(1, equalPos - 1).Trim();
            id = int.TryParse(idStr, out var parsedId) ? parsedId : -1;

            // int funcStart = curStr.IndexOfAny(new[] { ' ', '\t' }, equalPos + 1);
            int funcEnd = curStr.IndexOfAny(new[] { ' ', '\t', '(' }, equalPos + 1);
            //    string funcName = curStr.Substring(funcStart, funcEnd - funcStart).Trim();

            int argEnd = curStr.LastIndexOf(')');
            string argStr = curStr.Substring(funcEnd + 1, argEnd - funcEnd - 1);
            EntityType entityType = ParseEntityType(curStr);

            Entity ent = entityType.ToString() switch
            {
                "CARTESIAN_POINT" => new Point(Entities),
                "DIRECTION" => new Direction(Entities),
                "AXIS2_PLACEMENT_3D" => new Csys3D(Entities),
                "PLANE" => new Plane(Entities),
                "EDGE_LOOP" => new EdgeLoop(Entities),
                "FACE_BOUND" or "FACE_OUTER_BOUND" => new FaceBound(Entities),
                "ADVANCED_FACE" or "FACE_SURFACE" => new Face(Entities),
                "OPEN_SHELL" or "CLOSED_SHELL" => new Shell(Entities),
                "SHELL_BASED_SURFACE_MODEL" => new ShellModel(Entities),
                "MANIFOLD_SURFACE_SHAPE_REPRESENTATION" => new ManifoldShape(Entities),
                "VERTEX_POINT" => new Vertex(Entities),
                "SURFACE_CURVE" => new SurfaceCurve(Entities),
                "EDGE_CURVE" => new EdgeCurve(Entities),
                "ORIENTED_EDGE" => new OrientedEdge(Entities),
                "VECTOR" => new Vector(Entities),
                "LINE" => new Line(Entities),
                _ => null
            };

            if (ent != null)
            {
                ent.Id = id;
                entMap[id] = ent;
                ents.Add(ent);
                args.Add(argStr);
            }
        }

        Console.WriteLine(curStr);
    }

    // Process arguments
    for (int i = 0; i < ents.Count; i++)
    {
        ents[i].ParseArgs(entMap, args[i]);
    }
}

// Parse the STEP ASCII entity type from a line and return corresponding EntityType
public static EntityType ParseEntityType(string line)
{
    if (string.IsNullOrWhiteSpace(line)) throw new ArgumentException("line is null or empty", nameof(line));

    // Expect format like:  #1 = CARTESIAN_POINT('', (0,0,0));
    int eq = line.IndexOf('=');
    int paren = line.IndexOf('(');
    if (eq < 0 || paren < 0 || paren < eq)
        throw new FormatException("Invalid STEP line format");

    // Extract token between '=' and '(' and normalize
    string token = line.Substring(eq + 1, paren - (eq + 1)).Trim();
    // Token might have trailing/leading spaces; ensure upper-case for mapping
    string t = token.ToUpperInvariant();

    switch (t)
    {
        case "CARTESIAN_POINT": return EntityType.CARTESIAN_POINT;
        case "DIRECTION": return EntityType.DIRECTION;
        case "AXIS2_PLACEMENT_3D": return EntityType.AXIS2_PLACEMENT_3D;
        case "PLANE": return EntityType.PLANE;
        case "EDGE_LOOP": return EntityType.EDGE_LOOP;
        case "FACE_BOUND": 
        case "FACE_OUTER_BOUND":return EntityType.FACE_BOUND;
        case "ADVANCED_FACE":
        case "FACE_SURFACE": return EntityType.FACE;
        case "OPEN_SHELL":
        case "CLOSED_SHELL": return EntityType.SHELL;
        case "SHELL_BASED_SURFACE_MODEL": return EntityType.SHELL_MODEL;
        case "MANIFOLD_SURFACE_SHAPE_REPRESENTATION": return EntityType.MANIFOLD_SHAPE;
        case "VERTEX_POINT": return EntityType.VERTEX_POINT;
        case "SURFACE_CURVE": return EntityType.SURFACE_CURVE;
        case "EDGE_CURVE": return EntityType.EDGE_CURVE;
        case "ORIENTED_EDGE": return EntityType.ORIENTED_EDGE;
        case "VECTOR": return EntityType.VECTOR;
        case "LINE": return EntityType.LINE;
        default:
            // Try direct enum parse as fallback if token matches an enum name
            if (Enum.TryParse<EntityType>(t, out var et)) return et;
            throw new NotSupportedException($"Unsupported STEP entity type: {token}");
    }
}

 
 
    // // Create EdgeCurve
    // public EdgeCurve CreateEdgeCurve(Vertex vert1, Vertex vert2, bool dir)
    // {
    //     var edgeCurve = new EdgeCurve(Entities, vert1, vert2, null, dir);
    //     return edgeCurve;
    // }
    //
    // // Build triangular body (placeholder logic)
    // public void BuildTriBody(List<double> tris, double tol, ref int mergedEdgeCount)
    // {
    //     // Implement triangulation and merging logic here
    //     // For now, just a placeholder
    //     mergedEdgeCount = 0;
    // }
    //
    // // Get edge from map
    // public void GetEdgeFromMap(
    //     double[] p0,
    //     double[] p1,
    //     Dictionary<(double, double, double, double, double, double), EdgeCurve> edgeMap,
    //     Vertex vert1,
    //     Vertex vert2,
    //     out EdgeCurve edgeCurve,
    //     out bool edgeDir,
    //     ref int mergeCount)
    // {
    //     var key = (p0[0], p0[1], p0[2], p1[0], p1[1], p1[2]);
    //     if (edgeMap.TryGetValue(key, out edgeCurve))
    //     {
    //         edgeDir = true;
    //         mergeCount++;
    //     }
    //     else
    //     {
    //         edgeCurve = CreateEdgeCurve(vert1, vert2, true);
    //         edgeMap[key] = edgeCurve;
    //         edgeDir = true;
    //     }
    // }
    //
    // // Write STEP file
    // public void WriteStep(string fileName)
    // {
    //     using (var writer = new StreamWriter(fileName))
    //     {
    //         foreach (var entity in Entities)
    //         {
    //             entity.Serialize(writer.BaseStream);
    //         }
    //     }
    // }
    //
    // // Read a line from STEP file
    // public string ReadLine(StreamReader stpFile, bool skipAllSpace)
    // {
    //     string line;
    //     do
    //     {
    //         line = stpFile.ReadLine();
    //     } while (skipAllSpace && string.IsNullOrWhiteSpace(line));
    //     return line;
    // }
    //
    // // Read STEP file (placeholder)
    // public void ReadStep(string fileName)
    // {
    //     using (var reader = new StreamReader(fileName))
    //     {
    //         while (!reader.EndOfStream)
    //         {
    //             var line = ReadLine(reader, true);
    //             // Parse STEP entities here
    //         }
    //     }
    // }
}

}