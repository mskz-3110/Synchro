using System.Diagnostics;
using UnityEngine;

namespace Synchro {
  public class SynchronizerUpdater : MonoBehaviour {
    [SerializeField] private float m_Interval = 0.01f;

    private Stopwatch m_Stopwatch = new Stopwatch();

    private void Start(){
      m_Stopwatch.Start();
    }

    private void Update(){
      m_Stopwatch.Stop();
      if (m_Stopwatch.ElapsedMilliseconds < m_Interval * 1000){
        m_Stopwatch.Start();
        return;
      }

      m_Stopwatch.Restart();
      ReplayingManager.Instance.Update();
      RecordingManager.Instance.Update();
    }
  }
}