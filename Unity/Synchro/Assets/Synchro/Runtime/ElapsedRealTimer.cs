using System;

namespace Synchro {
  public class ElapsedRealTimer : IElapsedTimer {
    static public double Now(){
      DateTime nowTime = DateTime.UtcNow;
      return new DateTimeOffset(nowTime).ToUnixTimeSeconds() + (double)nowTime.Millisecond / 1000;
    }

    private double m_StartTime;

    public ElapsedRealTimer(){
      Set(Now());
    }

    public float Update(){
      return (float)(Now() - m_StartTime);
    }

    public void Set(double startTime){
      m_StartTime = startTime;
    }
  }
}