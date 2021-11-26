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

using MB.Graphics2.TextureAtlas.Basic;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace MB.Encoder.TextureAtlas.BTA
{
  public readonly struct NamedAtlasNineSlice : IEquatable<NamedAtlasNineSlice>
  {
    public readonly string Name;
    public readonly AtlasNineSliceInfo NineSliceInfo;

    //------------------------------------------------------------------------------------------------------------------------------------------------

    public NamedAtlasNineSlice(string name, AtlasNineSliceInfo nineSliceInfo)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      if (name.Length <= 0)
        throw new Exception($"Entry name '{name}' can not have a length of zero");
      if (name.Contains('\\', StringComparison.Ordinal))
        throw new ArgumentException($"Name '{name}' can not contain backslashes", nameof(name));
      if (name.StartsWith('/'))
        throw new ArgumentException($"Name  '{name}'can not start with a slash", nameof(name));
      if (name.EndsWith('/'))
        throw new ArgumentException($"Name '{name}' can not end with a slash", nameof(name));
      if (name.Contains("//", StringComparison.Ordinal))
        throw new Exception($"Name '{name}' can not contain two consecutive slashes '//'");

      NineSliceInfo = nineSliceInfo;
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    public bool IsValid => Name != null;

    //------------------------------------------------------------------------------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(NamedAtlasNineSlice lhs, NamedAtlasNineSlice rhs)
      => (lhs.Name == rhs.Name && lhs.NineSliceInfo == rhs.NineSliceInfo);

    //------------------------------------------------------------------------------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(NamedAtlasNineSlice lhs, NamedAtlasNineSlice rhs) => !(lhs == rhs);


    //------------------------------------------------------------------------------------------------------------------------------------------------

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is NamedAtlasNineSlice other && (this == other);

    //------------------------------------------------------------------------------------------------------------------------------------------------

    public override int GetHashCode() => Name.GetHashCode(StringComparison.Ordinal) ^ NineSliceInfo.GetHashCode();

    //------------------------------------------------------------------------------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(NamedAtlasNineSlice other) => this == other;

    //------------------------------------------------------------------------------------------------------------------------------------------------

    public override string ToString() => $"Name:\"{Name}\" NineSliceInfo:{NineSliceInfo}";
  }
}

//****************************************************************************************************************************************************
