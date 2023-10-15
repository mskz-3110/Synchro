using System;

namespace Synchro {
  public class Time {
    static public double Now(){
      DateTime nowTime = DateTime.UtcNow;
      return new DateTimeOffset(nowTime).ToUnixTimeSeconds() + (double)nowTime.Millisecond / 1000;
    }

    static public bool Passed(float targetTime, float checkTime){
      return targetTime <= checkTime;
    }
  }
}