using NUnit.Framework;
using UnityEngine;
using System.Threading;
using System.IO;

namespace Synchro {
  public class SynchroTestEditor {
    [Test]
    public void SkyIdxTest(){
      var skyIdx = new SkyIdx<string>();
      Assert.That(skyIdx.TryAdd("b", "5"));
      Assert.That(skyIdx.TryAdd("ab", "4"));
      Assert.That(skyIdx.TryAdd("aa", "2"));
      Assert.That(skyIdx.TryAdd("a", "1"));
      Assert.That(skyIdx.TryAdd("aaa", "3"));

      Assert.That(skyIdx.Count == 5);
      Log(skyIdx);
      Check(skyIdx, 0, "a", "1");
      Check(skyIdx, 1, "aa", "2");
      Check(skyIdx, 2, "aaa", "3");
      Check(skyIdx, 3, "ab", "4");
      Check(skyIdx, 4, "b", "5");
      skyIdx.Clear();
      Assert.That(skyIdx.Count == 0);
      Log(skyIdx);
    }

    private void Log(SkyIdx<string> skyIdx){
      Debug.Log($"----- Count={skyIdx.Count} -----");
      var index = 0;
      while (skyIdx.TryGet(index, out var pair)){
        Debug.Log($"[{index}] key={pair.Key} value={pair.Value}");
        ++index;
      }
    }

    private void Check(SkyIdx<string> skyIdx, int index, string checkKey, string checkValue){
      Assert.That(skyIdx.FindIndex(checkKey) == index);
      Assert.That(skyIdx.TryGet(index, out var pair));
      Assert.That(checkKey == pair.Key);
      Assert.That(checkValue == pair.Value);
    }

    [Test]
    public void ElapsedTimerTest(){
      var elapsedManualTimer = new ElapsedManualTimer();
      Debug.Log($"[ElapsedManualTimer] {elapsedManualTimer}");
      elapsedManualTimer.ElapsedTime = 0.5f;
      Debug.Log($"[ElapsedManualTimer] {elapsedManualTimer}");
      elapsedManualTimer.ElapsedTime = 1f;
      Debug.Log($"[ElapsedManualTimer] {elapsedManualTimer}");

      var elapsedRealTimer = new ElapsedRealTimer();
      Debug.Log($"[ElapsedRealTimer] {elapsedRealTimer}");
      Thread.Sleep(500);
      Debug.Log($"[ElapsedRealTimer] {elapsedRealTimer}");
      Thread.Sleep(500);
      Debug.Log($"[ElapsedRealTimer] {elapsedRealTimer}");
    }

    private class RecycleTestValue {
      public int Value = 100;
    }

    [Test]
    public void RecyclerTest(){
      var recycler = new Recycler<RecycleTestValue>();
      Assert.That(recycler.Count == 0);
      Assert.That(recycler.RecycledCount == 0);

      recycler.Enqueue(x => x.Value = 1);
      Assert.That(recycler.Count == 1);
      Assert.That(recycler.RecycledCount == 0);

      recycler.Enqueue(x => x.Value = 2);
      Assert.That(recycler.Count == 2);
      Assert.That(recycler.RecycledCount == 0);

      recycler.Once(x => {
        Assert.That(x.Value == 100);
        Assert.That(recycler.Count == 2);
        Assert.That(recycler.RecycledCount == 0);
        x.Value = 99;
      });
      Assert.That(recycler.Count == 2);
      Assert.That(recycler.RecycledCount == 1);

      recycler.Recycle(recycler.Dequeue());
      Assert.That(recycler.Count == 1);
      Assert.That(recycler.RecycledCount == 2);

      recycler.Once(x => {
        Assert.That(x.Value == 99);
        Assert.That(recycler.Count == 1);
        Assert.That(recycler.RecycledCount == 1);
      });
      Assert.That(recycler.Count == 1);
      Assert.That(recycler.RecycledCount == 2);

      recycler.Clear();
      Assert.That(recycler.Count == 0);
      Assert.That(recycler.RecycledCount == 3);
    }

    [Test]
    public void FrameTest(){
      var writeFrame = new Frame(5, 0.5f);
      writeFrame.Write(buffer => {
        buffer.Write("Frame");
      });

      var readFrame = new Frame();
      readFrame.Read(writeFrame.Buffer.AsReadOnlySpan());
      Assert.That(readFrame.SynchronizerType == 5);
      Assert.That(readFrame.ElapsedTime == 0.5f);
      Assert.That(readFrame.Buffer.Read() == "Frame");

      writeFrame.SynchronizerType = 15;
      writeFrame.ElapsedTime = 1.5f;
      writeFrame.Rewrite();
      readFrame.Read(writeFrame.Buffer.AsReadOnlySpan());
      Assert.That(readFrame.SynchronizerType == 15);
      Assert.That(readFrame.ElapsedTime == 1.5f);
      Assert.That(readFrame.Buffer.Read() == "Frame");
    }

    [Test]
    public void StreamTest(){
      var writeCount = 10;
      var bufferWriter = new Stream.BufferWriter(new DastBytes.Buffer());
      WriteStream(bufferWriter, writeCount);
      var bufferReader = new Stream.BufferReader(bufferWriter.Buffer);
      var cancelCount = 5;
      bufferReader.Buffer.Reread(_ => {
        Assert.That(ReadStream(bufferReader, new CancellationTokenSource(), cancelCount) == cancelCount);
      });
      Assert.That(ReadStream(bufferReader, null, cancelCount) == writeCount);

      var binaryWriter = new Stream.BinaryWriter(new BinaryWriter(new MemoryStream()));
      WriteStream(binaryWriter, writeCount);
      var binaryReader = new Stream.BinaryReader(new BinaryReader(binaryWriter.Stream));
      binaryReader.Stream.Seek(0, SeekOrigin.Begin);
      Assert.That(ReadStream(binaryReader, new CancellationTokenSource(), cancelCount) == cancelCount);
      binaryReader.Stream.Seek(0, SeekOrigin.Begin);
      Assert.That(ReadStream(binaryReader, null, cancelCount) == writeCount);
    }

    private void WriteStream(Stream.IWriter writer, int writeCount){
      var buffer = new DastBytes.Buffer();
      for (var i = 0; i < writeCount; ++i){
        buffer.Clear();
        buffer.Write(i);
        Stream.Write(writer, buffer.AsReadOnlySpan());
      }
    }

    private int ReadStream(Stream.IReader reader, CancellationTokenSource cts, int cancelCount){
      var buffer = new DastBytes.Buffer();
      var readCount = 0;
      Stream.ReadEach(reader, cts, bytes => {
        buffer.Clear();
        buffer.Write(bytes);
        Assert.That(buffer.Read<int>() == readCount);
        ++readCount;
        if (readCount == cancelCount) cts?.Cancel();
      });
      return readCount;
    }
  }
}