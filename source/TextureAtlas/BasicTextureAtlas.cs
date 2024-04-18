#nullable enable
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace MB.Encoder.TextureAtlas.BTA
{
  public class BasicTextureAtlas
  {
    public readonly ImmutableArray<NamedAtlasTexture> Entries;
    public readonly ImmutableArray<NamedAtlasNineSlice> NineSliceEntries;
    public readonly ImmutableArray<NamedComplexPatch> PatchEntries;
    public readonly ImmutableArray<NamedAnchorPoints> AnchorPointEntries;
    public readonly UInt16 DefaultDpi;

    public BasicTextureAtlas(NamedAtlasTexture[] entries, NamedAtlasNineSlice[] nineSliceEntries, NamedComplexPatch[] patchEntries,
                             NamedAnchorPoints[] anchorPoints, UInt16 defaultDpi)
      : this(ImmutableArray.Create(entries), ImmutableArray.Create(nineSliceEntries), ImmutableArray.Create(patchEntries),
             ImmutableArray.Create(anchorPoints), defaultDpi)
    {
    }

    public BasicTextureAtlas(ImmutableArray<NamedAtlasTexture> entries, UInt16 defaultDpi)
      : this(entries, ImmutableArray<NamedAtlasNineSlice>.Empty, ImmutableArray<NamedComplexPatch>.Empty, ImmutableArray<NamedAnchorPoints>.Empty, defaultDpi)
    {
    }

    public BasicTextureAtlas(ImmutableArray<NamedAtlasTexture> entries, ImmutableArray<NamedAtlasNineSlice> nineSliceEntries,
                             ImmutableArray<NamedComplexPatch> patchEntries, ImmutableArray<NamedAnchorPoints> anchorPointEntries,
                             UInt16 defaultDpi)
    {
      ValidateNamedAtlasTextureEntries(entries, nineSliceEntries, patchEntries);
      Entries = entries;
      NineSliceEntries = nineSliceEntries;
      PatchEntries = patchEntries;
      AnchorPointEntries = anchorPointEntries;
      DefaultDpi = defaultDpi;
    }


    public int Count => Entries.Length;

    public NamedAtlasTexture this[int index] => Entries[index];

    private static void ValidateNamedAtlasTextureEntries(ImmutableArray<NamedAtlasTexture> entries,
                                                         ImmutableArray<NamedAtlasNineSlice> nineSliceEntries,
                                                         ImmutableArray<NamedComplexPatch> patchEntries)
    {
      var uniqueNameDict = new Dictionary<string, NamedAtlasTexture>();
      for (int i = 0; i < entries.Length; ++i)
      {
        var entry = entries[i];
        var nameId = entry.Name.ToUpperInvariant();
        if (!entry.IsValid)
          throw new Exception($"Entry at index {i} is invalid");
        if (uniqueNameDict.TryGetValue(nameId, out NamedAtlasTexture previousEntry))
        {
          if (entry.Name == previousEntry.Name)
            throw new Exception($"Entry named {entry.Name} was already added");
          else
            throw new Exception($"Entry named {entry.Name} was already added as {previousEntry.Name} and we do not allow names where the only difference is the casing");
        }
        uniqueNameDict.Add(nameId, entry);
      }

      {
        var uniqueNineSliceNameDict = new Dictionary<string, NamedAtlasNineSlice>();
        foreach (var entry in nineSliceEntries)
        {
          var nameId = entry.Name.ToUpperInvariant();
          if (!uniqueNameDict.TryGetValue(nameId, out NamedAtlasTexture parent))
            throw new Exception($"NineSlice entry named {entry.Name} does not have a corrosponding texture entry");
          if (parent.Name != entry.Name)
            throw new Exception($"NineSlice entry named {entry.Name} does not match the casing of the corrosponding texture entry '{parent.Name}'");

          if (uniqueNineSliceNameDict.TryGetValue(nameId, out NamedAtlasNineSlice previousEntry))
          {
            if (entry.Name == previousEntry.Name)
              throw new Exception($"NineSlice entry named {entry.Name} was already added");
            else
              throw new Exception($"NineSlice entry named {entry.Name} was already added as {previousEntry.Name} and we do not allow names where the only difference is the casing");
          }
          uniqueNineSliceNameDict.Add(nameId, entry);
        }
      }

      {
        var uniquePatchNameDict = new Dictionary<string, NamedComplexPatch>();
        foreach (var entry in patchEntries)
        {
          var nameId = entry.Name.ToUpperInvariant();
          if (!uniqueNameDict.TryGetValue(nameId, out NamedAtlasTexture parent))
            throw new Exception($"NineSlice entry named {entry.Name} does not have a corrosponding texture entry");
          if (parent.Name != entry.Name)
            throw new Exception($"NineSlice entry named {entry.Name} does not match the casing of the corrosponding texture entry '{parent.Name}'");

          if (uniquePatchNameDict.TryGetValue(nameId, out NamedComplexPatch previousEntry))
          {
            if (entry.Name == previousEntry.Name)
              throw new Exception($"NineSlice entry named {entry.Name} was already added");
            else
              throw new Exception($"NineSlice entry named {entry.Name} was already added as {previousEntry.Name} and we do not allow names where the only difference is the casing");
          }
          uniquePatchNameDict.Add(nameId, entry);
        }

      }
    }
  }
}

//****************************************************************************************************************************************************
