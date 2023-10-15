using System;
using System.Threading;

namespace Synchro {
  public class ReplayingManager {
    static private ReplayingManager s_Instance;
    static public ReplayingManager Instance => s_Instance ??= new ReplayingManager();

    public delegate void StartReplayingEvent();
    public StartReplayingEvent OnStartReplaying;

    public delegate void StopReplayingEvent();
    public StopReplayingEvent OnStopReplaying;

    private IElapsedTimer m_ElapsedTimer = new ElapsedManualTimer();
    public IElapsedTimer ElapsedTimer => m_ElapsedTimer;

    private bool m_IsReplaying;
    public bool IsReplaying => m_IsReplaying;

    private float m_LastElapsedTime;
    public float LastElapsedTime => m_LastElapsedTime;

    public void StartReplaying(IElapsedTimer elapsedTimer){
      StopReplaying();
      m_IsReplaying = true;
      m_ElapsedTimer = elapsedTimer;
      m_LastElapsedTime = 0;
      OnStartReplaying?.Invoke();
      foreach (Synchronizer synchronizer in SynchronizerManager.Instance.Synchronizers){
        synchronizer.StartReplaying();
      }
    }

    public void StartReplaying(){
      StartReplaying(new ElapsedRealTimer());
    }

    public bool TryAddFrame(ReadOnlySpan<byte> bytes, out Frame frame){
      Synchronizer synchronizer = SynchronizerManager.Instance.Get(bytes[0]);
      if (synchronizer == null){
        frame = null;
        return false;
      }

      frame = synchronizer.ReplayFrameRecycler.Enqueue(bytes);
      if (m_LastElapsedTime < frame.ElapsedTime) m_LastElapsedTime = frame.ElapsedTime;
      return true;
    }

    public AsyncHandler ReadEachFrameAsync(Stream.IReader reader, float readAheadTime, TimeSpan interval){
      return new AsyncHandler(cts => {
        Stream.ReadEach(reader, cts, bytes => {
          if (TryAddFrame(bytes, out Frame frame)){
            if (m_ElapsedTimer.Update() + readAheadTime < frame.ElapsedTime) Thread.Sleep(interval);
          }
        });
      });
    }

    public void Update(){
      if (!m_IsReplaying) return;

      float updateTime = m_ElapsedTimer.Update();
      foreach (Synchronizer synchronizer in SynchronizerManager.Instance.Synchronizers){
        synchronizer.UpdateReplaying(updateTime);
      }
    }

    public void StopReplaying(){
      foreach (Synchronizer synchronizer in SynchronizerManager.Instance.Synchronizers){
        synchronizer.ReplayFrameRecycler.Clear();
      }
      if (!m_IsReplaying) return;

      foreach (Synchronizer synchronizer in SynchronizerManager.Instance.Synchronizers){
        synchronizer.StopReplaying();
      }
      m_IsReplaying = false;
      OnStopReplaying?.Invoke();
    }
  }
}