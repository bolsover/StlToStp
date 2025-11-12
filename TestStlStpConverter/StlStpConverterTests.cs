using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using Bolsover.Converter;

namespace TestStlToStp
{
    [TestFixture]
    public class StlStpConverterTests
    {
//         [Test]
//         public async Task ReadStlAsync_ShouldReturnEmptyList_WhenFileDoesNotExist()
//         {
//             var result = await StlReader.ReadStlAsync("nonexistent.stl");
//             Assert.That(result, Is.Empty);
//         }
//
//         [Test]
//         public async Task ReadStlAsync_ShouldReturnEmptyList_WhenFileTooSmall()
//         {
//             string tempFile = Path.GetTempFileName();
//             await FileHelpers.WriteAllTextAsync(tempFile, "tiny");
//
//             var result = await StlReader.ReadStlAsync(tempFile);
//             Assert.That(result, Is.Empty);
//
//             File.Delete(tempFile);
//         }
//
//         [Test]
//         public async Task ReadStlAsciiAsync_ShouldParseVerticesCorrectly()
//         {
//             string tempFile = Path.GetTempFileName();
//             string asciiContent = @"solid ascii
// facet normal 0 0 0
// outer loop
// vertex 1.0 2.0 3.0
// vertex 4.0 5.0 6.0
// vertex 7.0 8.0 9.0
// endloop
// endfacet
// endsolid";
//             await FileHelpers.WriteAllTextAsync(tempFile, asciiContent);
//
//             var result = await StlReader.ReadStlAsciiAsync(tempFile);
//             Assert.That(result.Count, Is.EqualTo(9));
//             Assert.That(result[0], Is.EqualTo(1.0));
//             Assert.That(result[8], Is.EqualTo(9.0));
//
//             File.Delete(tempFile);
//         }
//
//         [Test]
//         public async Task ReadStlBinaryAsync_ShouldParseVerticesCorrectly()
//         {
//             string tempFile = Path.GetTempFileName();
//
//             // Create a minimal binary STL with 1 triangle
//             using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
//             using (var bw = new BinaryWriter(fs))
//             {
//                 bw.Write(new byte[80]); // header
//                 bw.Write((uint)1); // number of triangles
//
//                 // Normal vector (3 floats)
//                 bw.Write(0f);
//                 bw.Write(0f);
//                 bw.Write(0f);
//
//                 // 3 vertices (9 floats)
//                 float[] vertices = { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f };
//                 foreach (var v in vertices) bw.Write(v);
//
//                 bw.Write((ushort)0); // attribute byte count
//             }
//
//             var result = await StlReader.ReadStlBinaryAsync(tempFile);
//             Assert.That(result.Count, Is.EqualTo(9));
//             Assert.That(result[0], Is.EqualTo(1.0));
//             Assert.That(result[8], Is.EqualTo(9.0));
//
//             File.Delete(tempFile);
//         }
//
//         [Test]
//         public async Task ReadStlAsync_ShouldDetectAsciiAndParseCorrectly()
//         {
//             string tempFile = Path.GetTempFileName();
//             string asciiContent = @"solid ascii
// facet normal 0 0 0
// outer loop
// vertex 1.0 2.0 3.0
// vertex 4.0 5.0 6.0
// vertex 7.0 8.0 9.0
// endloop
// endfacet
// endsolid";
//             await FileHelpers.WriteAllTextAsync(tempFile, asciiContent);
//
//             var result = await StlReader.ReadStlAsync(tempFile);
//             Assert.That(result.Count, Is.EqualTo(9));
//             Assert.That(result[0], Is.EqualTo(1.0));
//             Assert.That(result[8], Is.EqualTo(9.0));
//
//             File.Delete(tempFile);
//         }
//

        [Test]
        public async Task TestConvert()
        {
            var result = await StlReader.ConvertToStp("Pencil Case.stl", "Pencil Case.stp", 0.0000001);
            Assert.AreEqual(0, result);
        }
     }  
}