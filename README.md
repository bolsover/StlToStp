# StlStpConverter

This software includes code derived from the slugdev/stltostp project, licensed under the BSD-4-Clause License.
Copyright (c) 2018, slugdev


Modifications Copyright (c) 2025 David Bolsover.  
Original slugdev code was written in C++.  
That original code informed the design and implementation of this converter in C#.

A C# Windows Forms application that converts STL (STereoLithography) files to STEP (ISO 10303) format.  
Additional classes are provided to permit implementation of an Alibre Add-on.  
This tool supports both ASCII and binary STL formats and generates ISO 10303-21 STEP files compatible with most CAD software.

## Features

- **Dual Format Support**: Reads both ASCII and binary STL files with automatic format detection
- **Asynchronous Processing**: Uses async/await for efficient file I/O operations
- **Edge Merging**: Intelligently merges coplanar edges during conversion
- **Tolerance Control**: Configurable tolerance for edge merging and geometric comparisons
- **Windows Forms GUI**: User-friendly interface for file selection and conversion
- **Robust Error Handling**: Comprehensive error handling for file operations

## Project Structure

```
StlStpConverter/
├── StlStpConverter/                    # Main application
│   ├── STLReader.cs        # STL file parser (ASCII & binary)
│   ├── StepWriter.cs       # STEP file generation engine
│   ├── IEntity.cs          # Base entity interface
│   ├── CartesianPoint.cs   # 3D point representation
│   ├── Vector.cs           # 3D vector operations
│   ├── Direction.cs        # Directional vectors
│   ├── Plane.cs            # Planar surface definitions
│   ├── Face.cs             # Face geometry
│   ├── Edge*.cs            # Edge-related classes
│   ├── Shell*.cs           # Shell model classes
│   └── ...                 # Additional geometric entities
│   ├── StlStpForm.cs       # Main application form
│   └── Program.cs          # Application entry point
└── TestStlToStp/           # Unit tests
    ├── StlStpConverterTests.cs   # STL reader writer tests
    └── SplitteratorTests.cs      # General tests for splitting .stl files to separate bodies.
    └── *.stl               # Test STL files
```

## Requirements

- **.NET Framework**: 4.8.1
- **Windows OS**: Windows Forms application
- **JetBrains Rider**: 2025 or later (recommended)
- **Visual Studio**: 2019 or later (optional)
- **NUnit**: 3.5.0 (for running tests)

## Building

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd StlStpConverter
   ```

2. Open the solution:
   ```bash
   StlStpConverter.sln
   ```

3. Build the solution:
   - In JetBrains Rider: `Build > Reuild Solution` 
   - In Visual Studio: `Build > Build Solution` or `Ctrl+Shift+B`
   

## Usage

### GUI Application

1. Launch `StlStpConverter.exe` from the build directory
2. Use the Windows Forms interface to:
   - Select an STL file for input
   - Choose output location for the STEP file
   - Configure conversion options (tolerance, etc.)
   - Click Convert to process the file

### Programmatic Usage

```csharp
using Bolsover.Converter;

// Convert STL to STEP with default tolerance (1e-6)
await STLReader.ConvertToStp("input.stl", "output.stp");

// Convert with custom tolerance
await STLReader.ConvertToStp("input.stl", "output.stp", tol: 1e-5);
```


## How It Works

1. **STL Parsing**: The `STLReader` class automatically detects whether an STL file is ASCII or binary format by examining the file header
2. **Triangle Extraction**: Extracts vertex coordinates from the triangular mesh
3. **STEP Generation**: The `StepWriter` class constructs a STEP boundary representation (B-Rep) model from the triangle mesh
4. **Edge Merging**: Coplanar edges are merged based on the specified tolerance to create cleaner geometry
5. **File Export**: Generates an ISO 10303-21 compliant STEP file

## Testing

The project includes unit tests in the `TestStlStpConverter` project:

## Technical Details

### Supported STL Formats

- **ASCII STL**: Text-based format starting with "solid"
- **Binary STL**: Binary format with 80-byte header + triangle data

### STEP Format

Generates ISO 10303-21 (STEP Physical File) format, specifically:
- Application Protocol: AP203 or AP214 compatible
- Boundary Representation (B-Rep) models
- Manifold solid brep shape representation

### Performance

- Uses buffered asynchronous I/O for large files
- Pre-allocates memory based on triangle count
- Efficient binary parsing with byte arrays

## License

BSD-4-Clause

## Author

David Bolsover

## Contributing

[Specify contribution guidelines if applicable]

## Known Limitations

- Output is a faceted STEP model (not smoothed surfaces)
- Very large STL files may require significant memory
- Edge merging is based on planarity tolerance
