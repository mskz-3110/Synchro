using System.Collections.Generic;

namespace Synchro {
  public class SkyIdx<Value> {
    public class Pair {
      private string m_Key;
      public string Key => m_Key;

      private Value m_Value;
      public Value Value => m_Value;

      public Pair(string key, Value value){
        m_Key = key;
        m_Value = value;
      }
    }

    public delegate void EachEvent(int index, Pair pair);

    private List<Pair> m_Pairs = new List<Pair>();

    public int Count => m_Pairs.Count;

    public void Clear(){
      m_Pairs.Clear();
    }

    public int FindIndex(string key){
      for (int i = 0; i < m_Pairs.Count; ++i){
        if (m_Pairs[i].Key.Equals(key)) return i;
      }
      return -1;
    }

    public bool TryAdd(string key, Value value){
      if (0 <= FindIndex(key)) return false;

      m_Pairs.Add(new Pair(key, value));
      m_Pairs.Sort((a, b) => a.Key.CompareTo(b.Key));
      return true;
    }

    public bool TryGet(int index, out Pair pair){
      if (index < m_Pairs.Count){
        pair = m_Pairs[index];
        return true;
      }

      pair = default;
      return false;
    }

    public void Each(EachEvent onEach){
      for (int i = 0; i < m_Pairs.Count; ++i){
        onEach(i, m_Pairs[i]);
      }
    }
  }
}