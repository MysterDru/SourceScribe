using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using SourceScribe.CodeModel.Abstractions;

namespace SourceScribe.CodeModel.Roslyn
{
    internal sealed class LocationCollection : List<ILocation>
    {
        private LocationCollection(IEnumerable<ILocation> locations) : base(locations)
        {

        }


        internal static LocationCollection Create(ImmutableArray<Microsoft.CodeAnalysis.Location> locations)
        {
            return new LocationCollection(locations.Select(x => Location.Create(x)));
        }
    }
}
