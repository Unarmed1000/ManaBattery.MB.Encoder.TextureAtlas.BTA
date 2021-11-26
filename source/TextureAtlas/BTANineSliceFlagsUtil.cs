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

using MB.Graphics2.TextureAtlas.Basic;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace MB.Encoder.TextureAtlas.BTA
{
  public static class BTANineSliceFlagsUtil
  {
    public static BTANineSliceFlags FromAtlasNineSliceFlags(AtlasNineSliceFlags flags)
    {
      return (flags.IsFlagged(AtlasNineSliceFlags.Slice0Transparent) ? BTANineSliceFlags.Slice0Transparent : BTANineSliceFlags.None) |
             (flags.IsFlagged(AtlasNineSliceFlags.Slice1Transparent) ? BTANineSliceFlags.Slice1Transparent : BTANineSliceFlags.None) |
             (flags.IsFlagged(AtlasNineSliceFlags.Slice2Transparent) ? BTANineSliceFlags.Slice2Transparent : BTANineSliceFlags.None) |
             (flags.IsFlagged(AtlasNineSliceFlags.Slice3Transparent) ? BTANineSliceFlags.Slice3Transparent : BTANineSliceFlags.None) |
             (flags.IsFlagged(AtlasNineSliceFlags.Slice4Transparent) ? BTANineSliceFlags.Slice4Transparent : BTANineSliceFlags.None) |
             (flags.IsFlagged(AtlasNineSliceFlags.Slice5Transparent) ? BTANineSliceFlags.Slice5Transparent : BTANineSliceFlags.None) |
             (flags.IsFlagged(AtlasNineSliceFlags.Slice6Transparent) ? BTANineSliceFlags.Slice6Transparent : BTANineSliceFlags.None) |
             (flags.IsFlagged(AtlasNineSliceFlags.Slice7Transparent) ? BTANineSliceFlags.Slice7Transparent : BTANineSliceFlags.None) |
             (flags.IsFlagged(AtlasNineSliceFlags.Slice8Transparent) ? BTANineSliceFlags.Slice8Transparent : BTANineSliceFlags.None);
    }
  }
}

//****************************************************************************************************************************************************
