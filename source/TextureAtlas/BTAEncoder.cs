//#define LOCAL_SANITY_CHECK
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

using MB.Base;
using MB.Base.Container;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace MB.Encoder.TextureAtlas.BTA
{
  public class BTAEncoder
  {
    private const UInt32 NineSliceVersion2 = 0x2;

    private const UInt32 ChunkMagicMarker = 0x6b6843;

    private class PathNode
    {
      public readonly Dictionary<String, PathNode> Dict = new Dictionary<string, PathNode>();
      public UInt32 IndexTag;

      public override string ToString()
      {
        return $"Dict: {Dict} Index: {IndexTag}";
      }
    }

    private struct FilenameInfo
    {
      public int AtlasIndex;
      public PathNode ParentNode;
      public string Filename;
      public string FullPathFilename;

      public FilenameInfo(int atlasIndex, PathNode parentNode, string filename, string fullPathFilename)
      {
        Debug.Assert(atlasIndex >= 0);
        AtlasIndex = atlasIndex;
        ParentNode = parentNode ?? throw new ArgumentNullException(nameof(parentNode));
        Filename = filename ?? throw new ArgumentNullException(nameof(filename));
        FullPathFilename = fullPathFilename ?? throw new ArgumentNullException(nameof(fullPathFilename));
      }
    }


    private readonly struct BTA3EncodedPath
    {
      public readonly UInt32 ParentPathIndex;
      public readonly string Path;

      public BTA3EncodedPath(UInt32 parentPathIndex, string path)
      {
        if (path.Length <= 0)
          throw new ArgumentException("Path length must be larger than zero", nameof(path));
        if (path.Contains('/', StringComparison.Ordinal))
          throw new ArgumentException("Path can not contain '/'", nameof(path));

        ParentPathIndex = parentPathIndex;
        Path = path;
      }
    };

    private struct ChunkInfo
    {
      public int OffsetStart;
      public int OffsetSize;

      public ChunkInfo(int offsetStart, int offsetSize)
      {
        OffsetStart = offsetStart;
        OffsetSize = offsetSize;
      }
    }

    private enum ChunkType
    {
      NineSlices = 0x1,
      // AnchorPoints = 0x2,
    }

    public readonly string DefaultExtension = "bta";

    public ByteList Encode(BasicTextureAtlas atlas, BtaVersion version)
    {
      if (atlas == null)
      {
        throw new ArgumentNullException(nameof(atlas));
      }
      if (version != BtaVersion.V3 && version != BtaVersion.V4)
      {
        throw new NotSupportedException("Unsupported BTA version");
      }

      var result = new ByteList(4096 * 4);
      List<FilenameInfo> filenameInfo;
      var encodedPaths = EncodePathTree(atlas, out filenameInfo);

#if LOCAL_SANITY_CHECK
      // Internal integrity check
      ValidateEncodedPaths(atlas, encodedPaths, filenameInfo);
#endif


      AddHeader(result, (UInt32)version);
      // make room for a number of bytes written entry and store the offset where it was written so we can pacth it later
      var offset = result.Count;
      result.AddUInt32(0);

      AddPathList(result, encodedPaths);

      Debug.Assert(filenameInfo.Count == atlas.Count);

      // sort the filenames by the original path
      // - This should ensure that we always output the same file given the same input
      // - This is also the reason why we use the CompareOrdinal method (since it ignores locale).
      filenameInfo.Sort((lhs, rhs) => string.CompareOrdinal(lhs.FullPathFilename, rhs.FullPathFilename));

      // Write the atlas entries
      result.AddEncodedUInt32(UncheckedNumericCast.ToUInt32(filenameInfo.Count));
      for (int i = 0; i < filenameInfo.Count; ++i)
      {
        var info = filenameInfo[i];
        AddEntry(result, atlas[info.AtlasIndex], info);
      }

      // Write the number of bytes that were written to the extended header (-4 because we dont count the 'size' entry)
      int bytesWritten = result.Count - offset - 4;
      result.SetUInt32(offset, UncheckedNumericCast.ToUInt32(bytesWritten));

      // V4 and above might contain a additional number of optional chunks
      if (atlas.NineSliceEntries.Length > 0)
      {
        if (version == BtaVersion.V3)
          throw new NotSupportedException("BTA3 can not contain nine slice information, please upgrade to BTA4 or newer");

        AddNineSliceChunk(result, atlas, filenameInfo);
      }
      return result;
    }

    static private void AddNineSliceChunk(ByteList dst, BasicTextureAtlas atlas, List<FilenameInfo> sortedFileNameInfo)
    {
      var nameToKeyDict = new Dictionary<string, int>(sortedFileNameInfo.Count);
      for (Int32 i = 0; i < sortedFileNameInfo.Count; ++i)
      {
        var entry = sortedFileNameInfo[i];
        nameToKeyDict[entry.FullPathFilename] = i;
      }

      ChunkInfo chunkInfo = BeginChunk(dst, ChunkType.NineSlices, NineSliceVersion2);
      dst.AddEncodedUInt32((UInt32)atlas.NineSliceEntries.Length);
      foreach (var entry in atlas.NineSliceEntries)
      {
        // Lookup the texture entry
        if (!nameToKeyDict.TryGetValue(entry.Name, out int sortedIndex))
          throw new NotSupportedException($"The nine slice '{entry.Name}' did not have a corresponding texture entry");
        //var filenameInfo = sortedFileNameInfo[sortedIndex];

        BTANineSliceFlags flags = BTANineSliceFlagsUtil.FromAtlasNineSliceFlags(entry.NineSliceInfo.Flags);
        UInt32 encodedFlags = (UInt32)flags;

        // Encode the nineslice entry by associating it with the texture filename entry
        dst.AddEncodedThicknessU(entry.NineSliceInfo.NineSlicePx);
        dst.AddEncodedThicknessU(entry.NineSliceInfo.ContentMarginPx);
        dst.AddEncodedUInt32((UInt32)sortedIndex);
        // v2 adds 32bits of flags
        dst.AddEncodedUInt32(encodedFlags);
      }
      EndChunk(dst, chunkInfo);
    }


    static private ChunkInfo BeginChunk(ByteList dst, ChunkType chunkType, UInt32 chunkVersion)
    {
      var offsetStart = dst.Count;
      dst.AddUInt32(ChunkMagicMarker);
      var offsetSize = dst.Count;
      dst.AddUInt32(0u);    // chunk length
      dst.AddEncodedUInt32((UInt32)chunkType);
      dst.AddEncodedUInt32(chunkVersion);
      // +4 because
      return new ChunkInfo(offsetStart, offsetSize);
    }

    static private void EndChunk(ByteList dst, ChunkInfo chunkInfo)
    {
      // We include the full size of everything in the chunk as part of the chunk length
      int chunkLength = dst.Count - chunkInfo.OffsetStart;
      Debug.Assert(chunkLength > 0);
      dst.SetUInt32(chunkInfo.OffsetSize, (UInt32)chunkLength);    // length
    }


    private string DebugReconstructFilename(BTA3EncodedPath[] encodedPaths, UInt32 parentIndex)
    {
      if (parentIndex <= 0)
        return string.Empty;
      var current = encodedPaths[parentIndex - 1];
      var parentPath = DebugReconstructFilename(encodedPaths, current.ParentPathIndex);
      return parentPath.Length > 0 ? $"{parentPath}/{current.Path}" : current.Path;
    }

#if LOCAL_SANITY_CHECK
    private string DebugReconstructFilename(BTA3EncodedPath[] encodedPaths, FilenameInfo fileInfo)
    {
      if (fileInfo.ParentNode.IndexTag <= 0)
        return fileInfo.Filename;
      var parentPath = DebugReconstructFilename(encodedPaths, fileInfo.ParentNode.IndexTag);
      return $"{parentPath}/{fileInfo.Filename}";
    }

    private void ValidateEncodedPaths(BasicTextureAtlas atlas, BTA3EncodedPath[] encodedPaths, List<FilenameInfo> files)
    {
      foreach (var entry in files)
      {
        var res = DebugReconstructFilename(encodedPaths, entry);
        if (res != entry.FullPathFilename)
          throw new Exception("Encoding error");
      }
    }
#endif

    private static void AddHeader(ByteList result, UInt32 version)
    {
      // Magic
      result.AddUInt32(0x00415442);
      // Version
      result.AddUInt32(version);
    }


    private static void AddPathList(ByteList result, BTA3EncodedPath[] encodedPaths)
    {
      result.AddEncodedUInt32(UncheckedNumericCast.ToUInt32(encodedPaths.Length));
      for (int i = 0; i < encodedPaths.Length; ++i)
      {
        var entry = encodedPaths[i];
        result.AddEncodedUInt32(entry.ParentPathIndex);
        result.AddString(entry.Path);
      }
    }

    private static void AddEntry(ByteList result, in NamedAtlasTexture namedAtlasTexture, FilenameInfo filenameInfo)
    {
      result.AddEncodedPxRectangleU(namedAtlasTexture.TextureInfo.TrimmedRectPx);
      result.AddEncodedThicknessU(namedAtlasTexture.TextureInfo.TrimMarginPx);
      result.AddEncodedUInt32(namedAtlasTexture.TextureInfo.Dpi);
      result.AddEncodedUInt32(filenameInfo.ParentNode.IndexTag);
      result.AddString(filenameInfo.Filename);
    }


    private BTA3EncodedPath[] EncodePathTree(BasicTextureAtlas atlas, out List<FilenameInfo> rFiles)
    {
      int entryCount;
      var pathDict = BuildPathDict(atlas, out rFiles, out entryCount);
      var result = EncodePathDict(pathDict, entryCount);
      return result;
    }

    private BTA3EncodedPath[] EncodePathDict(PathNode rootNode, int dirEntryCount)
    {
      var result = new BTA3EncodedPath[dirEntryCount];
      int dstIndex = 0;
      EncodePathDict(result, ref dstIndex, 0u, rootNode);
      if (dstIndex != dirEntryCount)
        throw new Exception("Encoding failed");
      return result;
    }

    private void EncodePathDict(BTA3EncodedPath[] dst, ref int rDstIndex, UInt32 parentPathIndex, PathNode pathNode)
    {
      Debug.Assert(dst != null);
      Debug.Assert(pathNode.Dict != null);

      // We sort the keys to ensure our dictionary get encoded the same way no matter how the dictionary enumerates it keys
      string[] sortedKeys = new string[pathNode.Dict.Keys.Count];
      {
        int index = 0;
        foreach (var entry in pathNode.Dict.Keys)
        {
          sortedKeys[index] = entry;
          ++index;
        }
        // - We use the CompareOrdinal method since it ignores locale and we want to produce the same file no matter the host settings.
        System.Array.Sort(sortedKeys, (lhs, rhs) => string.CompareOrdinal(lhs, rhs));
      }

      foreach (var sortedKey in sortedKeys)
      {
        var entryValue = pathNode.Dict[sortedKey];
        Debug.Assert(rDstIndex >= 0 && rDstIndex < dst.Length);

        // +1 because we use zero as (none)
        entryValue.IndexTag = UncheckedNumericCast.ToUInt32(rDstIndex + 1);
        dst[rDstIndex] = new BTA3EncodedPath(parentPathIndex, sortedKey);
        // +1 because we use zero as (none)
        UInt32 currentParentIndex = UncheckedNumericCast.ToUInt32(rDstIndex + 1);
        ++rDstIndex;

        if (entryValue.Dict.Count > 0)
        {
          EncodePathDict(dst, ref rDstIndex, currentParentIndex, entryValue);
        }
      }
    }

    private static PathNode BuildPathDict(BasicTextureAtlas atlas, out List<FilenameInfo> rFiles, out int rEntryCount)
    {
      int entryCount = 0;
      var pathNode = new PathNode();
      var files = new List<FilenameInfo>();
      for (int i = 0; i < atlas.Count; ++i)
      {
        var entry = atlas[i];
        var currentNode = pathNode;
        if (entry.Name.Length <= 0)
          throw new Exception($"Empty path is not allowed found at index {i}");

        var pathElements = entry.Name.Split('/');
        Debug.Assert(pathElements.Length > 0);
        for (int pathIndex = 0; pathIndex < pathElements.Length - 1; ++pathIndex)
        {
          var element = pathElements[pathIndex];
          if (element.Length <= 0)
            throw new Exception($"Empty path element is not allowed '{entry.Name}'");

          if (currentNode.Dict.TryGetValue(element, out PathNode? value))
          {
            currentNode = value;
            if (currentNode.Dict == null)
              throw new Exception($"Invalid path structure. Tried to add directory '{entry.Name}' with the same name as a file '{Join(pathElements, pathIndex)}'");
          }
          else
          {
            ++entryCount;
            var newNode = new PathNode();
            currentNode.Dict[element] = newNode;
            currentNode = newNode;
          }
        }
        files.Add(new FilenameInfo(i, currentNode, pathElements[pathElements.Length - 1], entry.Name));
      }
      rFiles = files;
      rEntryCount = entryCount;
      return pathNode;
    }

    private static string Join(string[] pathElements, int pathIndex)
    {
      string res = pathElements[0];
      for (int i = 1; i < pathIndex; ++i)
      {
        res += $"/{pathElements[i]}";
      }
      return res;
    }
  }
}

//****************************************************************************************************************************************************
