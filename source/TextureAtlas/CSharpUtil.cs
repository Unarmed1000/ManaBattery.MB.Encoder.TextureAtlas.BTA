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
using System.CodeDom.Compiler;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace MB.Encoder.TextureAtlas.BTA
{
  public static class CSharpUtil
  {
    public static void AddHeader(IndentedTextWriter writer, UInt32 creationYear, string companyName)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof(writer));

      writer.WriteLine("//****************************************************************************************************************************************************");
      writer.WriteLine("//* File Description");
      writer.WriteLine("//* ----------------");
      writer.WriteLine("//*");
      writer.WriteLine($"//* (c) {creationYear} {companyName}");
      writer.WriteLine("//****************************************************************************************************************************************************");
      writer.WriteLineNoTabs("");
    }

    public static void AddFooter(IndentedTextWriter writer)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof(writer));

      writer.WriteLineNoTabs("");
      writer.WriteLine("//****************************************************************************************************************************************************");
      //      writer.WriteLineNoTabs("");
    }

    public static void AddNamespaceBegin(IndentedTextWriter writer, string namespaceName)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof(writer));

      writer.WriteLine($"namespace {namespaceName}");
      writer.WriteLine($"{{");
      ++writer.Indent;
    }

    public static void AddNamespaceEnd(IndentedTextWriter writer)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof(writer));

      --writer.Indent;
      writer.WriteLine($"}}");
    }

    public static void AddStaticClassBegin(IndentedTextWriter writer, string className)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof(writer));

      writer.WriteLine($"static class {className}");
      writer.WriteLine($"{{");
      ++writer.Indent;
    }


    public static void AddClassBegin(IndentedTextWriter writer, string className, string parentClassName)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof(writer));

      writer.WriteLine($"class {className} : {parentClassName}");
      writer.WriteLine($"{{");
      ++writer.Indent;
    }

    public static void AddClassEnd(IndentedTextWriter writer)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof(writer));

      --writer.Indent;
      writer.WriteLine($"}}");
    }

  }
}

//****************************************************************************************************************************************************
