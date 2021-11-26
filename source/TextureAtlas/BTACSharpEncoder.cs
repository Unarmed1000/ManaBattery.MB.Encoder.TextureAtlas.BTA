

//****************************************************************************************************************************************************
//* BSD 3-Clause License
//*
//* Copyright (c) 2019, Mana Battery
//* All rights reserved.
//*
//* Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//*
//* 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//* 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the
//*    documentation and/or other materials provided with the distribution.
//* 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this
//*    software without specific prior written permission.
//*
//* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
//* THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
//* CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//* PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//* LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//* EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//****************************************************************************************************************************************************

using MB.Base.Container;
using MB.Base.MathEx.Pixel;
using MB.Graphics2.Patch.Advanced;
using MB.Graphics2.TextureAtlas.Basic;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace MB.Encoder.TextureAtlas.BTA
{
  public class BTACSharpEncoder
  {
    public readonly string DefaultExtension = "cs";

    private struct SortedNamedAtlasTexture
    {
      public int OriginalIndex;
      public NamedAtlasTexture Source;

      public SortedNamedAtlasTexture(int originalIndex, NamedAtlasTexture source)
      {
        OriginalIndex = originalIndex;
        Source = source;
      }
    }

    private struct SortedNineSliceInfo
    {
      public int TextureId;
      public AtlasNineSliceInfo Source;

      public SortedNineSliceInfo(int textureId, AtlasNineSliceInfo source)
      {
        TextureId = textureId;
        Source = source;
      }
    }

    private struct SortedComplexPatch
    {
      public int TextureId;
      public ImmutableComplexPatch Source;

      public SortedComplexPatch(int textureId, ImmutableComplexPatch source)
      {
        TextureId = textureId;
        Source = source;
      }
    }

    private struct SortedAnchorPoint
    {
      public int TextureId;
      public ImmutableArray<PxPoint2> AnchorPoints;

      public SortedAnchorPoint(int textureId, ImmutableArray<PxPoint2> anchorPoints)
      {
        TextureId = textureId;
        AnchorPoints = anchorPoints;
      }
    }

    private readonly struct Offsets
    {
      public readonly int SliceOffset;
      public readonly int SpanOffset;

      public Offsets(int sliceOffset, int spanOffset)
      {
        SliceOffset = sliceOffset;
        SpanOffset = spanOffset;
      }
    }

    public ByteList Encode(BasicTextureAtlas atlas, BtaVersion version, string atlasName, string companyName, string namespaceName, UInt32 creationYear)
    {
      if (atlas == null)
      {
        throw new ArgumentNullException(nameof(atlas));
      }
      if (version != BtaVersion.V4)
      {
        throw new NotSupportedException("Unsupported BTA version");
      }
      if (companyName == null)
      {
        throw new ArgumentNullException(nameof(companyName));
      }

      var sortedArray = BuildSortedEntriesArray(atlas);
      var newIndexLookupDict = new Dictionary<string, int>(sortedArray.Length);
      for (int i = 0; i < sortedArray.Length; ++i)
      {
        ref readonly var entry = ref sortedArray[i];
        newIndexLookupDict[entry.Source.Name] = i;
      }
      // Extract nine slices
      SortedNineSliceInfo[] sortedNineSlices = BuildSortedNineSliceArray(newIndexLookupDict, atlas.NineSliceEntries);
      // Extract complex patches
      SortedComplexPatch[] sortedPatches = BuildSortedPatchArray(newIndexLookupDict, atlas.PatchEntries);

      SortedAnchorPoint[] sortedAnchorPoints = BuildSortedAnchorPointsArray(newIndexLookupDict, atlas.AnchorPointEntries);

      using (var stringWriter = new System.IO.StringWriter())
      {
        using (var writer = new IndentedTextWriter(stringWriter, "  "))
        {
          string className = atlasName;
          string textureName = atlasName;

          CSharpUtil.AddHeader(writer, creationYear, companyName);
          AddUsings(writer);
          CSharpUtil.AddNamespaceBegin(writer, namespaceName);
          CSharpUtil.AddClassBegin(writer, className, "TextureAtlasBase");
          AddAtlasContent(writer, textureName, sortedArray, sortedNineSlices, sortedPatches, sortedAnchorPoints);
          AddConstructor(writer, className, atlas.DefaultDpi);
          CSharpUtil.AddClassEnd(writer);
          CSharpUtil.AddNamespaceEnd(writer);
          CSharpUtil.AddFooter(writer);
        }

        stringWriter.Flush();
        return new ByteList(System.Text.Encoding.UTF8.GetBytes(stringWriter.ToString()));
      }
    }

    private static SortedNamedAtlasTexture[] BuildSortedEntriesArray(BasicTextureAtlas atlas)
    {
      var sortedArray = new SortedNamedAtlasTexture[atlas.Count];

      { // Build a sorted array of the entries to ensure that our output is always in the same order
        for (int i = 0; i < sortedArray.Length; ++i)
        {
          sortedArray[i] = new SortedNamedAtlasTexture(i, atlas[i]);
        }
        // sort the filenames by the original path
        // - This should ensure that we always output the same file given the same input
        // - This is also the reason why we use the CompareOrdinal method (since it ignores locale).
        Array.Sort(sortedArray, (lhs, rhs) => string.CompareOrdinal(lhs.Source.Name, rhs.Source.Name));
      }
      return sortedArray;
    }


    private static SortedNineSliceInfo[] BuildSortedNineSliceArray(Dictionary<string, int> newIndexLookupDict,
                                                                   ImmutableArray<NamedAtlasNineSlice> nineSlices)
    {
      var sortedResult = new SortedNineSliceInfo[nineSlices.Length];
      for (int i = 0; i < nineSlices.Length; ++i)
      {
        var newTextureId = newIndexLookupDict[nineSlices[i].Name];
        sortedResult[i] = new SortedNineSliceInfo(newTextureId, nineSlices[i].NineSliceInfo);
      }
      // Sort based on the texture id to ensure we have a fixed output order
      Array.Sort(sortedResult, (lhs, rhs) => lhs.TextureId.CompareTo(rhs.TextureId));
      return sortedResult;
    }


    private static SortedComplexPatch[] BuildSortedPatchArray(Dictionary<string, int> newIndexLookupDict, ImmutableArray<NamedComplexPatch> patchEntries)
    {
      var sortedResult = new SortedComplexPatch[patchEntries.Length];
      for (int i = 0; i < patchEntries.Length; ++i)
      {
        var newTextureId = newIndexLookupDict[patchEntries[i].Name];
        sortedResult[i] = new SortedComplexPatch(newTextureId, patchEntries[i].ComplexPatch);
      }
      // Sort based on the texture id to ensure we have a fixed output order
      Array.Sort(sortedResult, (lhs, rhs) => lhs.TextureId.CompareTo(rhs.TextureId));
      return sortedResult;
    }

    private static SortedAnchorPoint[] BuildSortedAnchorPointsArray(Dictionary<string, int> newIndexLookupDict,
                                                                    ImmutableArray<NamedAnchorPoints> anchorPointEntries)
    {
      var sortedResult = new SortedAnchorPoint[anchorPointEntries.Length];
      for (int i = 0; i < anchorPointEntries.Length; ++i)
      {
        var newTextureId = newIndexLookupDict[anchorPointEntries[i].Name];
        sortedResult[i] = new SortedAnchorPoint(newTextureId, anchorPointEntries[i].AnchorPoints);
      }
      // Sort based on the texture id to ensure we have a fixed output order
      Array.Sort(sortedResult, (lhs, rhs) => lhs.TextureId.CompareTo(rhs.TextureId));
      return sortedResult;
    }

    private static void AddUsings(IndentedTextWriter writer)
    {
      writer.WriteLine($"using MB.Base.Container;");
      writer.WriteLine($"using MB.Base.MathEx;");
      writer.WriteLine($"using MB.Base.MathEx.Pixel;");
      writer.WriteLine($"using MB.Graphics2.Patch.Advanced;");
      writer.WriteLine($"using MB.Graphics2.TextureAtlas;");
      writer.WriteLine($"using MB.Graphics2.TextureAtlas.Basic;");
      writer.WriteLine($"using System;");
      writer.WriteLine($"using System.Collections.Generic;");
      writer.WriteLineNoTabs("");
    }



    private static void AddAtlasContent(IndentedTextWriter writer, string textureName, SortedNamedAtlasTexture[] sortedArray,
                                        SortedNineSliceInfo[] sortedNineSlices, SortedComplexPatch[] sortedPatches,
                                        SortedAnchorPoint[] sortedAnchorPoints)
    {
      writer.WriteLine($"private const string g_sourceTextureName = \"{textureName}\";");
      writer.WriteLineNoTabs("");

      // Write a "path" to array index lookup dict
      {
        writer.WriteLine($"public static readonly Dictionary<string, UInt32> g_textureLookup = new Dictionary<string, UInt32>()");
        writer.WriteLine($"{{");
        ++writer.Indent;

        for (int i = 0; i < sortedArray.Length; ++i)
        {
          writer.WriteLine($"{{ \"{sortedArray[i].Source.Name}\", {i} }},");
        }

        --writer.Indent;
        writer.WriteLine($"}};");
        writer.WriteLineNoTabs("");
      }

      writer.WriteLineNoTabs("");

      // Write the texture array
      {
        writer.WriteLine($"private static readonly TextureAtlasImageInfo[] g_imageInfoArray = new TextureAtlasImageInfo[{sortedArray.Length}]");
        writer.WriteLine($"{{");
        ++writer.Indent;

        for (int i = 0; i < sortedArray.Length; ++i)
        {
          ref readonly var entry = ref sortedArray[i];
          ref readonly var trimmedRectPx = ref entry.Source.TextureInfo.TrimmedRectPx;
          ref readonly var trimMarginPx = ref entry.Source.TextureInfo.TrimMarginPx;
          var dpi = entry.Source.TextureInfo.Dpi;
          writer.WriteLine($"new TextureAtlasImageInfo(new PxRectangleU({trimmedRectPx.Left}, {trimmedRectPx.Top}, {trimmedRectPx.Width}, {trimmedRectPx.Height}), new PxThicknessU({trimMarginPx.Left}, {trimMarginPx.Top}, {trimMarginPx.Right}, {trimMarginPx.Bottom}), {dpi}), // {i}");
        }

        --writer.Indent;
        writer.WriteLine($"}};");
      }

      writer.WriteLineNoTabs("");

      // Write the texture nineslice meta data
      {
        writer.WriteLine($"private static readonly Dictionary<UInt32, AtlasNineSliceInfo> g_nineSliceLookup = new Dictionary<UInt32, AtlasNineSliceInfo>({sortedNineSlices.Length})");
        writer.WriteLine($"{{");
        ++writer.Indent;

        for (int i = 0; i < sortedNineSlices.Length; ++i)
        {
          ref readonly var entry = ref sortedNineSlices[i];
          ref readonly var nineSlice = ref entry.Source.NineSlicePx;
          ref readonly var contentMargin = ref entry.Source.ContentMarginPx;
          string strFlags = ToFlagsString(entry.Source.Flags);
          writer.WriteLine($"{{ {entry.TextureId}, new AtlasNineSliceInfo(new PxThicknessU({nineSlice.Left}, {nineSlice.Top}, {nineSlice.Right}, {nineSlice.Bottom}), new PxThicknessU({contentMargin.Left}, {contentMargin.Top}, {contentMargin.Right}, {contentMargin.Bottom}), {strFlags} ) }},");
        }

        --writer.Indent;
        writer.WriteLine($"}};");
      }

      writer.WriteLineNoTabs("");

      // Write the texture patch meta data
      {
        var finalOffsets = new Offsets[sortedPatches.Length];
        {
          writer.WriteLine($"private static readonly ImmutableComplexPatchSlice[] g_patchSlices = new ImmutableComplexPatchSlice[]");
          writer.WriteLine($"{{");
          ++writer.Indent;

          int dstIndex = 0;
          for (int i = 0; i < sortedPatches.Length; ++i)
          {
            finalOffsets[i] = new Offsets(dstIndex, 0);

            ref readonly var entry = ref sortedPatches[i];
            var source = entry.Source;
            var sourceSpans = source.Slices.AsSpan();

            writer.WriteLine($"// Patch Id:{entry.TextureId} X:{source.Slices.CountX} Y:{source.Slices.CountY}");
            for (int spanIndex = 0; spanIndex < sourceSpans.Length; ++spanIndex)
            {
              writer.WriteLine($"new ImmutableComplexPatchSlice({sourceSpans[spanIndex].Position}, {ToString(sourceSpans[spanIndex].Flags)}),  // {dstIndex}");
              ++dstIndex;
            }
          }

          --writer.Indent;
          writer.WriteLine($"}};");
        }

        writer.WriteLineNoTabs("");

        {
          writer.WriteLine($"private static readonly ImmutableContentSpan[] g_patchContentSpans = new ImmutableContentSpan[]");
          writer.WriteLine($"{{");
          ++writer.Indent;

          int dstIndex = 0;
          for (int i = 0; i < sortedPatches.Length; ++i)
          {
            ref readonly var entry = ref sortedPatches[i];
            var source = entry.Source;
            var sourceSpans = source.ContentSpans.AsSpan();
            finalOffsets[i] = new Offsets(finalOffsets[i].SliceOffset, dstIndex);


            writer.WriteLine($"// Patch Id:{entry.TextureId} X:{source.ContentSpans.CountX} Y:{source.ContentSpans.CountY}");
            for (int spanIndex = 0; spanIndex < sourceSpans.Length; ++spanIndex)
            {
              writer.WriteLine($"new ImmutableContentSpan({sourceSpans[spanIndex].Start}, {sourceSpans[spanIndex].Length}), // {dstIndex}");
              ++dstIndex;
            }

          }
          --writer.Indent;
          writer.WriteLine($"}};");
        }

        writer.WriteLineNoTabs("");

        {
          writer.WriteLine($"private static readonly Dictionary<UInt32, ImmutableComplexPatch> g_patchLookup = new Dictionary<UInt32, ImmutableComplexPatch>({sortedPatches.Length})");
          writer.WriteLine($"{{");
          ++writer.Indent;


          for (int i = 0; i < sortedPatches.Length; ++i)
          {
            ref readonly var entry = ref sortedPatches[i];
            var source = entry.Source;

            // ReadOnlyArraySegment<ImmutableComplexPatchSlice>
            // ReadOnlyArraySegment<ImmutableContentSpan>
            int sliceCount = source.Slices.CountX + source.Slices.CountY;
            int spanCount = source.ContentSpans.CountX + source.ContentSpans.CountY;
            writer.WriteLine($"{{ {entry.TextureId}, new ImmutableComplexPatch(new ImmutableComplexPatchSlices(ReadOnlyArraySegment.Create(g_patchSlices, {finalOffsets[i].SliceOffset}, {sliceCount}), {source.Slices.CountX}, {source.Slices.CountY}, {ToString(source.Slices.Flags)}),");
            writer.WriteLine($"                                new ImmutablePatchContentSpans(ReadOnlyArraySegment.Create(g_patchContentSpans, {finalOffsets[i].SpanOffset}, {spanCount}), {source.ContentSpans.CountX}, {source.ContentSpans.CountY})) }},");
          }

          --writer.Indent;
          writer.WriteLine($"}};");
        }

        {
          writer.WriteLineNoTabs("");

          {
            writer.WriteLine($"private static readonly PxPoint2[] g_anchorPoints = new PxPoint2[]");
            writer.WriteLine($"{{");
            ++writer.Indent;

            int dstIndex = 0;
            for (int i = 0; i < sortedAnchorPoints.Length; ++i)
            {
              ref readonly var entry = ref sortedAnchorPoints[i];
              var points = entry.AnchorPoints;

              writer.WriteLine($"// Id:{entry.TextureId}");
              for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
              {
                writer.WriteLine($"new PxPoint2({points[pointIndex].X}, {points[pointIndex].Y}), // {dstIndex}");
                ++dstIndex;
              }
            }
            --writer.Indent;
            writer.WriteLine($"}};");
          }
          writer.WriteLineNoTabs("");

          {
            writer.WriteLine($"private static readonly Dictionary<UInt32, ReadOnlyArraySegment<PxPoint2>> g_anchorPointLookup = new Dictionary<UInt32, ReadOnlyArraySegment<PxPoint2>>()");
            writer.WriteLine($"{{");
            ++writer.Indent;

            int dstIndex = 0;
            for (int i = 0; i < sortedAnchorPoints.Length; ++i)
            {
              ref readonly var entry = ref sortedAnchorPoints[i];
              var points = entry.AnchorPoints;

              writer.WriteLine($"{{ {entry.TextureId}, new ReadOnlyArraySegment<PxPoint2>(g_anchorPoints, {dstIndex}, {points.Length}) }},");
              dstIndex += points.Length;
            }
            --writer.Indent;
            writer.WriteLine($"}};");
          }
        }
      }
    }

    private static string ToFlagsString(AtlasNineSliceFlags flags)
    {
      string result = string.Empty;
      if (flags.IsFlagged(AtlasNineSliceFlags.Slice0Transparent))
        result = "AtlasNineSliceFlags.Slice0Transparent";
      if (flags.IsFlagged(AtlasNineSliceFlags.Slice1Transparent))
        result = FlagStringOr(result, "AtlasNineSliceFlags.Slice1Transparent");
      if (flags.IsFlagged(AtlasNineSliceFlags.Slice2Transparent))
        result = FlagStringOr(result, "AtlasNineSliceFlags.Slice2Transparent");
      if (flags.IsFlagged(AtlasNineSliceFlags.Slice3Transparent))
        result = FlagStringOr(result, "AtlasNineSliceFlags.Slice3Transparent");
      if (flags.IsFlagged(AtlasNineSliceFlags.Slice4Transparent))
        result = FlagStringOr(result, "AtlasNineSliceFlags.Slice4Transparent");
      if (flags.IsFlagged(AtlasNineSliceFlags.Slice5Transparent))
        result = FlagStringOr(result, "AtlasNineSliceFlags.Slice5Transparent");
      if (flags.IsFlagged(AtlasNineSliceFlags.Slice6Transparent))
        result = FlagStringOr(result, "AtlasNineSliceFlags.Slice6Transparent");
      if (flags.IsFlagged(AtlasNineSliceFlags.Slice7Transparent))
        result = FlagStringOr(result, "AtlasNineSliceFlags.Slice7Transparent");
      if (flags.IsFlagged(AtlasNineSliceFlags.Slice8Transparent))
        result = FlagStringOr(result, "AtlasNineSliceFlags.Slice8Transparent");
      return result.Length <= 0 ? "AtlasNineSliceFlags.None" : result;
    }

    private static string FlagStringOr(string result, string value)
    {
      return result.Length <= 0 ? value : $"{result} | {value}";
    }

    private static string ToString(PatchFlags flags)
    {
      var result = new List<string>();
      if (flags.IsFlagged(PatchFlags.MirrorX))
      {
        result.Add("PatchFlags.MirrorX");
        flags &= ~PatchFlags.MirrorX;
      }
      if (flags.IsFlagged(PatchFlags.MirrorY))
      {
        result.Add("PatchFlags.MirrorY");
        flags &= ~PatchFlags.MirrorY;
      }
      if (flags != PatchFlags.None)
        throw new Exception($"Unsupported flags found: {flags}");
      return result.Count <= 0 ? "PatchFlags.None" : string.Join('|', result);
    }

    private static string ToString(ComplexPatchSliceFlags flags)
    {
      var result = new List<string>();
      if (flags.IsFlagged(ComplexPatchSliceFlags.Group0))
      {
        result.Add("ComplexPatchSliceFlags.Group0");
        flags &= ~ComplexPatchSliceFlags.Group0;
      }
      if (flags.IsFlagged(ComplexPatchSliceFlags.Group1))
      {
        result.Add("ComplexPatchSliceFlags.Group1");
        flags &= ~ComplexPatchSliceFlags.Group1;
      }
      if (flags.IsFlagged(ComplexPatchSliceFlags.Group2))
      {
        result.Add("ComplexPatchSliceFlags.Group2");
        flags &= ~ComplexPatchSliceFlags.Group2;
      }
      if (flags.IsFlagged(ComplexPatchSliceFlags.Group3))
      {
        result.Add("ComplexPatchSliceFlags.Group3");
        flags &= ~ComplexPatchSliceFlags.Group3;
      }
      if (flags.IsFlagged(ComplexPatchSliceFlags.Scale))
      {
        result.Add("ComplexPatchSliceFlags.Scale");
        flags &= ~ComplexPatchSliceFlags.Scale;
      }
      if (flags.IsFlagged(ComplexPatchSliceFlags.Transparent))
      {
        result.Add("ComplexPatchSliceFlags.Transparent");
        flags &= ~ComplexPatchSliceFlags.Transparent;
      }
      if (flags != ComplexPatchSliceFlags.None)
        throw new Exception($"Unsupported flags found: {flags}");
      return result.Count <= 0 ? "ComplexPatchSliceFlags.None" : string.Join('|', result);
    }

    private static void AddConstructor(IndentedTextWriter writer, string className, UInt16 dpi)
    {
      writer.WriteLineNoTabs("");
      writer.WriteLine($"public {className}()");
      ++writer.Indent;
      writer.WriteLine($": base(g_sourceTextureName, {dpi}, g_textureLookup, g_imageInfoArray, g_nineSliceLookup, g_patchLookup, g_anchorPointLookup)");
      --writer.Indent;
      writer.WriteLine($"{{");
      ++writer.Indent;
      --writer.Indent;
      writer.WriteLine($"}}");
    }
  }
}

//****************************************************************************************************************************************************
