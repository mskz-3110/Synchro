using System.Collections.Generic;

namespace Synchro {
  public class SynchronizerManager {
    static private SynchronizerManager s_Instance;
    static public SynchronizerManager Instance => s_Instance ??= new SynchronizerManager();

    private Dictionary<byte, Synchronizer> m_Synchronizers = new Dictionary<byte, Synchronizer>();
    public Dictionary<byte, Synchronizer>.ValueCollection Synchronizers => m_Synchronizers.Values;

    public void Set(Synchronizer synchronizer){
      m_Synchronizers[synchronizer.SynchronizerType] = synchronizer;
    }

    public bool TryGet(byte synchronizerType, out Synchronizer synchronizer){
      return m_Synchronizers.TryGetValue(synchronizerType, out synchronizer);
    }

    public Synchronizer Get(byte synchronizerType){
      return TryGet(synchronizerType, out Synchronizer synchronizer) ? synchronizer : null;
    }

    public T Get<T>(byte synchronizerType) where T : Synchronizer {
      return Get(synchronizerType) as T;
    }
  }
}