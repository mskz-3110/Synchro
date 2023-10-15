namespace Synchro {
  public class ElapsedRealTimer : IElapsedTimer {
    public double StartTime;

    public ElapsedRealTimer(){
      StartTime = Time.Now();
    }

    public ElapsedRealTimer(double startTime){
      StartTime = startTime;
    }

    public float Update() => (float)(Time.Now() - StartTime);

    public override string ToString(){
      return $"{Update():F3}";
    }
  }
}