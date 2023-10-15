using System;

namespace Synchro {
  public class FrameRecycler {
    public delegate void UpdateEvent(Frame frame);

    private Recycler<Frame> m_Recycler = new Recycler<Frame>();

    public void Clear(){
      m_Recycler.Clear();
    }

    public Frame Enqueue(ReadOnlySpan<byte> bytes){
      Frame frame = m_Recycler.Create();
      frame.Read(bytes);
      return m_Recycler.Enqueue(frame);
    }

    public void Update(float updateTime, UpdateEvent onUpdate){
      while (m_Recycler.TryPeek(out Frame frame)){
        if (!Time.Passed(frame.ElapsedTime, updateTime)) break;

        onUpdate(m_Recycler.Dequeue());
        m_Recycler.Recycle(frame);
      }
    }

    public void Update(UpdateEvent onUpdate){
      while (m_Recycler.TryPeek(out Frame frame)){
        onUpdate(m_Recycler.Dequeue());
        m_Recycler.Recycle(frame);
      }
    }
  }
}