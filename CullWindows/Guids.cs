// Guids.cs
// MUST match guids.h
using System;

namespace BrunoMLopes.CullWindows
{
    static class GuidList
    {
        public const string guidCullWindowsPkgString = "86b3430a-9294-4115-a3cd-907f077d8eaf";
        public const string guidCullWindowsCmdSetString = "81b30c38-2906-4ede-9a19-979c9e4cda61";

        public static readonly Guid guidCullWindowsCmdSet = new Guid(guidCullWindowsCmdSetString);
        public const string guidCullWindowsDesignerOptionsPage = "BE466BF8-97FC-11DD-B095-1D6456D89593";
    };
}