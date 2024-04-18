#nullable enable
//****************************************************************************************************************************************************
//* BSD 3-Clause License
//*
//* Copyright (c) 2020, Mana Battery
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
using System.Runtime.CompilerServices;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace MB.Encoder.TextureAtlas.BTA
{
  /// <summary>
  /// Nine patch flags
  /// 0|1|2
  /// -|-|-
  /// 3|4|5
  /// -|-|-
  /// 6|7|8
  /// </summary>
  [Flags]
  public enum BTANineSliceFlags : UInt32
  {
    /// <summary>
    /// No flags set
    /// </summary>
    None = 0,

    /// <summary>
    /// Slice0 contains transparent pixels
    /// </summary>
    Slice0Transparent = 0x1,
    /// <summary>
    /// Slice1 contains transparent pixels
    /// </summary>
    Slice1Transparent = 0x2,
    /// <summary>
    /// Slice2 contains transparent pixels
    /// </summary>
    Slice2Transparent = 0x4,
    /// <summary>
    /// Slice3 contains transparent pixels
    /// </summary>
    Slice3Transparent = 0x8,
    /// <summary>
    /// Slice4 contains transparent pixels
    /// </summary>
    Slice4Transparent = 0x10,
    /// <summary>
    /// Slice5 contains transparent pixels
    /// </summary>
    Slice5Transparent = 0x20,
    /// <summary>
    /// Slice6 contains transparent pixels
    /// </summary>
    Slice6Transparent = 0x40,
    /// <summary>
    /// Slice7 contains transparent pixels
    /// </summary>
    Slice7Transparent = 0x80,
    /// <summary>
    /// Slice8 contains transparent pixels
    /// </summary>
    Slice8Transparent = 0x100,
  }

  public static class BTANineSliceFlagsExt
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFlagged(this BTANineSliceFlags thisExt, BTANineSliceFlags flags) => (thisExt & flags) == flags;
  }
}

//****************************************************************************************************************************************************
