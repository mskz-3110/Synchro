using UnityEngine;

namespace Synchro {
  public class SyncColor : MonoBehaviour {
    private ColorSynchronizer m_ColorSynchronizer;

    private void OnStartRecording(){
      m_ColorSynchronizer.AddTransform(transform.ToStringFullPath(), transform);
    }

    private void Awake(){
      m_ColorSynchronizer = SynchronizerManager.Instance.Get<ColorSynchronizer>(SynchronizerTypes.Color);
      RecordingManager.Instance.OnStartRecording += OnStartRecording;
    }

    private void OnDestroy(){
      RecordingManager.Instance.OnStartRecording -= OnStartRecording;
    }
  }
}