using System;

namespace Synchro {
  public class Frame {
    static public int Compare(Frame a, Frame b) => (int)(a.ElapsedTime * 1000) - (int)(b.ElapsedTime * 1000);

    public byte SynchronizerType;

    public float ElapsedTime;

    private DastBytes.Buffer m_Buffer = new DastBytes.Buffer();
    public DastBytes.Buffer Buffer => m_Buffer;

    public Frame(){}

    public Frame(byte synchronizerType, float elapsedTime = 0){
      SynchronizerType = synchronizerType;
      ElapsedTime = elapsedTime;
    }

    public void Read(ReadOnlySpan<byte> bytes){
      m_Buffer.Clear();
      m_Buffer.Write(bytes);
      SynchronizerType = m_Buffer.Read<byte>();
      ElapsedTime = m_Buffer.Read<float>();
    }

    public void Write(DastBytes.Buffer.BufferEvent onBuffer){
      m_Buffer.Clear();
      m_Buffer.Write(SynchronizerType);
      m_Buffer.Write(ElapsedTime);
      onBuffer.Invoke(m_Buffer);
    }

    public void Rewrite(){
      m_Buffer.Rewrite(buffer => {
        buffer.Write(SynchronizerType);
        buffer.Write(ElapsedTime);
      });
    }

    public override string ToString(){
      return $"SynchronizerType={SynchronizerType} ElapsedTime={ElapsedTime:F3} Buffer.Length={m_Buffer.Length}";
    }

    public string ToString(string name){
      return $"{name}({ToString()})";
    }
  }
}