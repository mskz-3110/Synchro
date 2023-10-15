using System;
using System.Threading;

namespace Synchro {
  public class Stream {
    public delegate void ReadEvent(ReadOnlySpan<byte> bytes);

    public interface IReader {
      bool IsReadable();

      int ReadLength();

      void ReadBytes(Span<byte> bytes);
    }

    public interface IWriter {
      void WriteLength(int length);

      void WriteBytes(ReadOnlySpan<byte> bytes);
    }

    public class BufferReader : IReader {
      private DastBytes.Buffer m_Buffer;
      public DastBytes.Buffer Buffer => m_Buffer;

      public BufferReader(DastBytes.Buffer buffer){
        m_Buffer = buffer;
      }

      public bool IsReadable() => 0 < m_Buffer.Length;

      public int ReadLength() => m_Buffer.Read<int>();

      public void ReadBytes(Span<byte> bytes){
        m_Buffer.Read(bytes);
      }
    }

    public class BufferWriter : IWriter {
      private DastBytes.Buffer m_Buffer;
      public DastBytes.Buffer Buffer => m_Buffer;

      public BufferWriter(DastBytes.Buffer buffer){
        m_Buffer = buffer;
      }

      public void WriteLength(int length){
        m_Buffer.Write(length);
      }

      public void WriteBytes(ReadOnlySpan<byte> bytes){
        m_Buffer.Write(bytes);
      }
    }

    public class BinaryReader : IReader {
      private System.IO.BinaryReader m_BinaryReader;
      public System.IO.Stream Stream => m_BinaryReader.BaseStream;

      public BinaryReader(System.IO.BinaryReader binaryReader){
        m_BinaryReader = binaryReader;
      }

      public bool IsReadable() => m_BinaryReader.BaseStream.Position < m_BinaryReader.BaseStream.Length;

      public int ReadLength() => m_BinaryReader.ReadInt32();

      public void ReadBytes(Span<byte> bytes){
        m_BinaryReader.BaseStream.Read(bytes);
      }
    }

    public class BinaryWriter : IWriter {
      private System.IO.BinaryWriter m_BinaryWriter;
      public System.IO.Stream Stream => m_BinaryWriter.BaseStream;

      public BinaryWriter(System.IO.BinaryWriter binaryWriter){
        m_BinaryWriter = binaryWriter;
      }

      public void WriteLength(int length){
        m_BinaryWriter.Write(length);
      }

      public void WriteBytes(ReadOnlySpan<byte> bytes){
        m_BinaryWriter.BaseStream.Write(bytes);
      }
    }

    static public void Read(IReader reader, ReadEvent onRead){
      Span<byte> bytes = stackalloc byte[reader.ReadLength()];
      reader.ReadBytes(bytes);
      onRead.Invoke(bytes);
    }

    static public void ReadEach(IReader reader, CancellationTokenSource cts, ReadEvent onRead){
      while (!AsyncHandler.IsCanceled(cts) && reader.IsReadable()){
        Read(reader, onRead);
      }
    }

    static public void Write(IWriter writer, ReadOnlySpan<byte> bytes){
      writer.WriteLength(bytes.Length);
      writer.WriteBytes(bytes);
    }

    static public void Write(IWriter writer, Frame frame){
      frame.Buffer.Reread(buffer => {
        writer.WriteLength(buffer.Length);
        writer.WriteBytes(buffer.AsReadOnlySpan());
      });
    }
  }
}