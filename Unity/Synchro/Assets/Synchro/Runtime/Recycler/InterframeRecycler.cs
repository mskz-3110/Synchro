namespace Synchro {
  public class InterframeRecycler<Value> where Value : class, new() {
    public delegate void UpdateEvent(Value prev, Value next, float rate);

    private class InterframeValue {
      public float ElapsedTime;

      public Value Value = new();
    }

    private Recycler<InterframeValue> m_Recycler = new Recycler<InterframeValue>();

    private InterframeValue m_Prev = null;

    private void SetPrev(InterframeValue prev){
      if (m_Prev != null) m_Recycler.Recycle(m_Prev);
      m_Prev = prev;
    }

    public void Clear(){
      SetPrev(null);
      m_Recycler.Clear();
    }

    public Value Enqueue(float elapsedTime){
      return m_Recycler.Enqueue(interframeValue => interframeValue.ElapsedTime = elapsedTime).Value;
    }

    public void Update(float updateTime, UpdateEvent onUpdate){
      InterframeValue next = null;
      while (m_Recycler.TryPeek(out next)){
        if (Time.Passed(next.ElapsedTime, updateTime)){
          SetPrev(m_Recycler.Dequeue());
        }else{
          break;
        }
      }
      if (m_Prev == null) return;

      if (next == null){
        onUpdate(m_Prev.Value, m_Prev.Value, 0);
      }else{
        onUpdate(m_Prev.Value, next.Value, UnityEngine.Mathf.InverseLerp(m_Prev.ElapsedTime, next.ElapsedTime, updateTime));
      }
    }
  }
}