using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using SourceScribe.CodeModel.Abstractions;

namespace SourceScribe.CodeModel.Roslyn
{
    internal sealed class Location : ILocation 
    {
        private readonly Microsoft.CodeAnalysis.Location location;
        private readonly FileLinePositionSpan fileLinePositionSpan;

        public bool IsInSource => location.IsInSource;
        public string Path => fileLinePositionSpan.Path;

        public int StartLinePosition => fileLinePositionSpan.StartLinePosition.Line + 1;
        public int EndLinePosition => fileLinePositionSpan.EndLinePosition.Line + 1;

        public Location(Microsoft.CodeAnalysis.Location location)
        {
            this.location = location;
            this.fileLinePositionSpan = location.GetLineSpan();
        }
        

        public static Location Create(Microsoft.CodeAnalysis.Location location)
        {            
            return new Location(location);
        }
    }
}