namespace Synchro {
  public class ElapsedManualTimer : IElapsedTimer {
    private float m_ElapsedTime;

    public float Update(){
      return m_ElapsedTime;
    }

    public void Set(float elapsedTime){
      m_ElapsedTime = elapsedTime;
    }
  }
}