namespace Synchro {
  public class ElapsedManualTimer : IElapsedTimer {
    public float ElapsedTime;

    public float Update() => ElapsedTime;

    public override string ToString(){
      return $"{Update():F3}";
    }
  }
}