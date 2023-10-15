using UnityEngine;

namespace Synchro {
  public class SyncPosition : MonoBehaviour {
    private PositionSynchronizer m_PositionSynchronizer;

    private void OnStartRecording(){
      m_PositionSynchronizer.AddTransform(transform.ToStringFullPath(), transform);
    }

    private void Awake(){
      m_PositionSynchronizer = SynchronizerManager.Instance.Get<PositionSynchronizer>(SynchronizerTypes.Position);
      RecordingManager.Instance.OnStartRecording += OnStartRecording;
    }

    private void OnDestroy(){
      RecordingManager.Instance.OnStartRecording -= OnStartRecording;
    }
  }
}