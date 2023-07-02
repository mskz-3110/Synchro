using NUnit.Framework;
using UnityEngine;
using System.Threading;

namespace Synchro {
  public class SynchroTestEditor {
    [Test]
    public void ElapsedTimerTest(){
      var elapsedManualTimer = new ElapsedManualTimer();
      Debug.Log($"[ElapsedManualTimer] {elapsedManualTimer.Update():F3}");
      elapsedManualTimer.Set(0.5f);
      Debug.Log($"[ElapsedManualTimer] {elapsedManualTimer.Update():F3}");
      elapsedManualTimer.Set(1f);
      Debug.Log($"[ElapsedManualTimer] {elapsedManualTimer.Update():F3}");

      var elapsedRealTimer = new ElapsedRealTimer();
      Debug.Log($"[ElapsedRealTimer] {elapsedRealTimer.Update():F3}");
      Thread.Sleep(500);
      Debug.Log($"[ElapsedRealTimer] {elapsedRealTimer.Update():F3}");
      Thread.Sleep(500);
      Debug.Log($"[ElapsedRealTimer] {elapsedRealTimer.Update():F3}");
    }
  }
}