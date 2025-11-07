using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bolsover.StlToStp.Converter
{
    public class STLReader
    {
        public static List<double> ReadStlAscii(string fileName)
        {
            var nodes = new List<double>();

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Failed to open ASCII STL file: {fileName}");
                return nodes;
            }

            foreach (var line in File.ReadLines(fileName))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4 && parts[0] == "vertex")
                {
                    if (double.TryParse(parts[1], out double x) &&
                        double.TryParse(parts[2], out double y) &&
                        double.TryParse(parts[3], out double z))
                    {
                        nodes.Add(x);
                        nodes.Add(y);
                        nodes.Add(z);
                    }
                }
            }

            return nodes;
        }


        public static async Task<List<double>> ReadStlAsciiAsync(string fileName)
        {
            var nodes = new List<double>();

            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($"File not found: {fileName}");
                    return nodes;
                }

                using (var reader = new StreamReader(fileName))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 4 && parts[0] == "vertex")
                        {
                            if (double.TryParse(parts[1], out double x) &&
                                double.TryParse(parts[2], out double y) &&
                                double.TryParse(parts[3], out double z))
                            {
                                nodes.Add(x);
                                nodes.Add(y);
                                nodes.Add(z);
                            }
                        }
                    }
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error while reading file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($"Access denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return nodes;
        }

        public static List<double> ReadStlBinary(string fileName)
        {
            var nodes = new List<double>();

            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($"Failed to open binary STL file: {fileName}");
                    return nodes;
                }

                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    // Read 80-byte header
                    br.ReadBytes(80);

                    // Read number of triangles (uint32)
                    uint tris = br.ReadUInt32();

                    // Pre-allocate list for performance
                    nodes.Capacity = (int)tris * 9;

                    for (int i = 0; i < tris; i++)
                    {
                        // Read normal vector (3 floats)
                        br.ReadBytes(sizeof(float) * 3);

                        // Read 3 vertices (9 floats)
                        for (int j = 0; j < 9; j++)
                        {
                            float value = br.ReadSingle();
                            nodes.Add(value);
                        }

                        // Read attribute byte count (uint16)
                        br.ReadUInt16();
                    }
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error while reading file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($"Access denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return nodes;
        }

        public static async Task<List<double>> ReadStlBinaryAsync(string fileName)
        {
            var nodes = new List<double>();

            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($"Failed to open binary STL file: {fileName}");
                    return nodes;
                }

                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read,
                           bufferSize: 4096, useAsync: true))
                using (var br = new BinaryReader(fs))
                {
                    // Read 80-byte header
                    br.ReadBytes(80);

                    // Read number of triangles (uint32)
                    uint tris = br.ReadUInt32();

                    // Pre-allocate list for performance
                    nodes.Capacity = (int)tris * 9;

                    // Buffer for one triangle (normal + 3 vertices + attribute)
                    byte[] buffer = new byte[(3 + 9) * sizeof(float) + sizeof(ushort)];

                    for (int i = 0; i < tris; i++)
                    {
                        int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead != buffer.Length)
                            throw new EndOfStreamException("Unexpected end of STL file.");

                        // Skip normal (first 3 floats)
                        int offset = 3 * sizeof(float);

                        // Read 9 floats for vertices
                        for (int j = 0; j < 9; j++)
                        {
                            float value = BitConverter.ToSingle(buffer, offset);
                            nodes.Add(value);
                            offset += sizeof(float);
                        }
                        // Skip attribute (last 2 bytes)
                    }
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error while reading file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($"Access denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return nodes;
        }

        public static List<double> ReadStl(string fileName)
        {
            var nodes = new List<double>();

            try
            {
                // Check if file exists
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($"Failed to open STL file: {fileName}");
                    return nodes;
                }

                // Get file size
                var fileInfo = new FileInfo(fileName);
                long fileSize = fileInfo.Length;

                // Minimum size of an empty ASCII STL file is 15 bytes
                if (fileSize < 15)
                {
                    Console.WriteLine($"Invalid STL file: {fileName}");
                    return nodes;
                }

                // Read first 5 characters to check if it's ASCII ("solid")
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    byte[] firstBytes = new byte[5];
                    int bytesRead = fs.Read(firstBytes, 0, 5);

                    if (bytesRead < 5)
                    {
                        Console.WriteLine($"Invalid STL file: {fileName}");
                        return nodes;
                    }

                    string firstWord = System.Text.Encoding.ASCII.GetString(firstBytes);

                    if (firstWord == "solid")
                    {
                        nodes = ReadStlAscii(fileName);
                    }
                    else
                    {
                        nodes = ReadStlBinary(fileName);
                    }
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($"Access denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return nodes;
        }


        public static async Task<List<double>> ReadStlAsync(string fileName)
        {
            var nodes = new List<double>();

            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($"Failed to open STL file: {fileName}");
                    return nodes;
                }

                var fileInfo = new FileInfo(fileName);
                long fileSize = fileInfo.Length;

                // Minimum size of an empty ASCII STL file is 15 bytes
                if (fileSize < 15)
                {
                    Console.WriteLine($"Invalid STL file: {fileName}");
                    return nodes;
                }

                // Read first 5 bytes to check for "solid"
                byte[] firstBytes = new byte[5];
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read,
                           bufferSize: 4096, useAsync: true))
                {
                    int bytesRead = await fs.ReadAsync(firstBytes, 0, 5);
                    if (bytesRead < 5)
                    {
                        Console.WriteLine($"Invalid STL file: {fileName}");
                        return nodes;
                    }
                }

                string firstWord = Encoding.ASCII.GetString(firstBytes);

                if (firstWord == "solid")
                {
                    // Possible ASCII STL, but could be binary with "solid" in header
                    // Check further: read more content and look for "facet" keyword
                    bool looksAscii = await LooksLikeAsciiAsync(fileName);

                    if (looksAscii)
                    {
                        nodes = await ReadStlAsciiAsync(fileName);
                    }
                    else
                    {
                        nodes = await ReadStlBinaryAsync(fileName);
                    }
                }
                else
                {
                    nodes = await ReadStlBinaryAsync(fileName);
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($"Access denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return nodes;
        }

        private static async Task<bool> LooksLikeAsciiAsync(string fileName)
        {
            // Heuristic: ASCII STL should contain "facet" somewhere in the first few KB
            const int checkSize = 1024; // 1 KB
            byte[] buffer = new byte[checkSize];

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096,
                       useAsync: true))
            {
                int bytesRead = await fs.ReadAsync(buffer, 0, checkSize);
                string content = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                return content.Contains("facet");
            }
        }

        // Assume these async methods exist:
        // private static Task<List<double>> ReadStlAsciiAsync(string fileName) => Task.FromResult(new List<double>());
        // private static Task<List<double>> ReadStlBinaryAsync(string fileName) => Task.FromResult(new List<double>());

        public static async Task<int> Convert(string inputFile, string outputFile, double tol = 1e-6)
        {
            bool mergePlanar = false;


            // Read STL file (async)
            List<double> nodes = await ReadStlAsync(inputFile);
            if (nodes.Count / 9 == 0)
            {
                Console.WriteLine($"No triangles found in STL file: {inputFile}");
                return 1;
            }

            Console.WriteLine($"Read {nodes.Count / 9} triangles from {inputFile}");

            // Build STEP body and write output
            StepKernel se = new StepKernel();
            int mergedEdgeCount = 0;
            se.BuildTriBody(nodes, tol, ref mergedEdgeCount);
            se.WriteStep(outputFile);

            Console.WriteLine($"Merged {mergedEdgeCount} edges");
            Console.WriteLine($"Exported STEP file: {outputFile}");

            return 0;
        }
    }
}