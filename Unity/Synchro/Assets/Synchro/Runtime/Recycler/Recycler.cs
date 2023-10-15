using System.Collections.Generic;

namespace Synchro {
  public class Recycler<Value> where Value : class, new() {
    public delegate void ValueEvent(Value value);

    public delegate T ResultValueEvent<T>(Value value);

    private Queue<Value> m_Values = new Queue<Value>();

    private Queue<Value> m_Recycles = new Queue<Value>();

    public int Count => m_Values.Count;

    public int RecycledCount => m_Recycles.Count;

    public void Clear(){
      lock (this){
        if (m_Values.Count == 0) return;

        while (m_Values.TryDequeue(out Value value)){
          m_Recycles.Enqueue(value);
        }
      }
    }

    public Value Create(){
      lock (this){
        return m_Recycles.TryDequeue(out Value value) ? value : new();
      }
    }

    public Value Enqueue(Value value){
      lock (this){
        m_Values.Enqueue(value);
        return value;
      }
    }

    public Value Enqueue(ValueEvent onValue){
      Value value = Create();
      lock (this){
        onValue(value);
        m_Values.Enqueue(value);
        return value;
      }
    }

    public bool TryPeek(out Value value){
      lock (this){
        return m_Values.TryPeek(out value);
      }
    }

    public Value Dequeue(){
      lock (this){
        return m_Values.Dequeue();
      }
    }

    public void Recycle(Value value){
      lock (this){
        m_Recycles.Enqueue(value);
      }
    }

    public void Once(ValueEvent onValue){
      Value value = Create();
      onValue(value);
      lock (this){
        m_Recycles.Enqueue(value);
      }
    }

    public T Once<T>(ResultValueEvent<T> onResultValue){
      Value value = Create();
      T result = onResultValue(value);
      lock (this){
        m_Recycles.Enqueue(value);
      }
      return result;
    }
  }
}