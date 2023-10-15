namespace Synchro {
  public class RecordingManager {
    static private RecordingManager s_Instance;
    static public RecordingManager Instance => s_Instance ??= new RecordingManager();

    public delegate void StartRecordingEvent();
    public StartRecordingEvent OnStartRecording;

    public delegate void RecordEvent(byte[] bytes);
    public RecordEvent OnRecord;

    public delegate void StopRecordingEvent();
    public StopRecordingEvent OnStopRecording;

    private IElapsedTimer m_ElapsedTimer = new ElapsedManualTimer();
    public IElapsedTimer ElapsedTimer => m_ElapsedTimer;

    private bool m_IsRecording;
    public bool IsRecording => m_IsRecording;

    public void StartRecording(IElapsedTimer elapsedTimer){
      StopRecording();
      m_IsRecording = true;
      m_ElapsedTimer = elapsedTimer;
      OnStartRecording?.Invoke();
      foreach (Synchronizer synchronizer in SynchronizerManager.Instance.Synchronizers){
        synchronizer.StartRecording();
      }
    }

    public void StartRecording(){
      StartRecording(new ElapsedRealTimer());
    }

    public void Record(byte[] bytes){
      if (!m_IsRecording) return;

      lock (this){
        OnRecord?.Invoke(bytes);
      }
    }

    public void Update(){
      float updateTime = m_ElapsedTimer.Update();
      foreach (Synchronizer synchronizer in SynchronizerManager.Instance.Synchronizers){
        synchronizer.UpdateRecording(updateTime);
      }
    }

    public void StopRecording(){
      if (!m_IsRecording) return;

      foreach (Synchronizer synchronizer in SynchronizerManager.Instance.Synchronizers){
        synchronizer.StopRecording();
      }
      m_IsRecording = false;
      OnStopRecording?.Invoke();
    }
  }
}