namespace Synchro {
  public abstract class Synchronizer {
    public class Categories {
      public const byte Frame = 0;
      public const byte First = 1;
    }

    public abstract byte SynchronizerType { get; }

    public abstract bool Skippable { get; }

    private Frame m_RecordFrame = new Frame();

    private FrameRecycler m_ReplayFrameRecycler = new FrameRecycler();
    public FrameRecycler ReplayFrameRecycler => m_ReplayFrameRecycler;

    public Synchronizer(){
      m_RecordFrame.SynchronizerType = SynchronizerType;
    }

    public virtual void StartRecording(){}

    protected void Record(float elapsedTime, DastBytes.Buffer.BufferEvent onBuffer){
      lock (m_RecordFrame){
        m_RecordFrame.ElapsedTime = elapsedTime;
        m_RecordFrame.Write(buffer => {
          onBuffer?.Invoke(buffer);
          RecordingManager.Instance.Record(buffer.AsBytes());
        });
      }
    }

    protected void Record(DastBytes.Buffer.BufferEvent onBuffer){
      Record(RecordingManager.Instance.ElapsedTimer.Update(), onBuffer);
    }

    public virtual void UpdateRecording(float updateTime){}

    public virtual void StopRecording(){}

    public virtual void StartReplaying(){}

    public abstract void UpdateReplaying(float updateTime);

    public virtual void StopReplaying(){}

    public override string ToString(){
      return $"{GetType().Name}(SynchronizerType={SynchronizerType} Skippable={Skippable})";
    }
  }
}