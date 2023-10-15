using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

namespace Synchro {
  public class SynchronizerTypes {
    public const byte Color = 1;
    public const byte Position = 2;

    public const byte Finish = 255;
  }

  public class ColorSynchronizer : Synchronizer {
    public override byte SynchronizerType => SynchronizerTypes.Color;

    public override bool Skippable => false;

    private SkyIdx<Transform> m_TransformSkyIdx = new SkyIdx<Transform>();

    private int m_ColorPropertyId;

    private MaterialPropertyBlock m_MaterialPropertyBlock;

    private List<Renderer> m_Renderers = new List<Renderer>();

    public ColorSynchronizer(){
      m_ColorPropertyId = Shader.PropertyToID("_Color");
      m_MaterialPropertyBlock = new MaterialPropertyBlock();
    }

    public void AddTransform(string key, Transform transform){
      Assert.That(m_TransformSkyIdx.TryAdd(key, transform));
      m_Renderers.AddRange(transform.GetComponentsInChildren<Renderer>());
    }

    public override void StartRecording(){
      Record(0, buffer => {
        buffer.Write(Categories.First);
        buffer.Write(GetType().Name);
      });
    }

    public void Record(float elapsedTime, Color color){
      Record(elapsedTime, buffer => {
        buffer.Write(Categories.Frame);
        buffer.Write(color.r);
        buffer.Write(color.g);
        buffer.Write(color.b);
      });
    }

    private void Replay(float elapsedTime, DastBytes.Buffer buffer){
      switch (buffer.Read<byte>()){
        case Categories.First:{
          Assert.That(buffer.Read() == GetType().Name);
        }break;

        case Categories.Frame:{
          var color = new Color(buffer.Read<float>(), buffer.Read<float>(), buffer.Read<float>());
          Debug.Log($"{elapsedTime:F3} {color}");
          foreach (var renderer in m_Renderers){
            for (var i = 0; i < renderer.sharedMaterials.Length; ++i){
              renderer.GetPropertyBlock(m_MaterialPropertyBlock, i);
              m_MaterialPropertyBlock.SetColor(m_ColorPropertyId, color);
              renderer.SetPropertyBlock(m_MaterialPropertyBlock, i);
            }
          }
        }break;
      }
    }

    public override void UpdateReplaying(float updateTime){
      ReplayFrameRecycler.Update(updateTime, frame => Replay(frame.ElapsedTime, frame.Buffer));
    }
  }

  public class PositionSynchronizer : Synchronizer {
    private class InterframeValue {
      public Vector3[] Positions = new Vector3[0];
    }

    public override byte SynchronizerType => SynchronizerTypes.Position;

    public override bool Skippable => true;

    private SkyIdx<Transform> m_TransformSkyIdx = new SkyIdx<Transform>();

    private InterframeRecycler<InterframeValue> m_InterframeRecycler = new InterframeRecycler<InterframeValue>();

    public void AddTransform(string key, Transform transform){
      Assert.That(m_TransformSkyIdx.TryAdd(key, transform));
    }

    public override void StartRecording(){
      Record(0, buffer => {
        buffer.Write(Categories.First);
        buffer.Write(m_TransformSkyIdx.Count);
        m_TransformSkyIdx.Each((_, pair) => {
          buffer.Write(pair.Key);
        });
      });
    }

    public void Record(float elapsedTime, Vector3[] positions){
      Record(elapsedTime, buffer => {
        buffer.Write(Categories.Frame);
        buffer.Write(positions);
      });
    }

    public override void StartReplaying(){
      m_InterframeRecycler.Clear();
    }

    private void Replay(float elapsedTime, DastBytes.Buffer buffer){
      switch (buffer.Read<byte>()){
        case Categories.First:{
          Assert.That(m_TransformSkyIdx.Count == buffer.Read<int>());
          m_TransformSkyIdx.Each((_, pair) => {
            Assert.That(pair.Key == buffer.Read());
          });
        }break;

        case Categories.Frame:{
          var interframeValue = m_InterframeRecycler.Enqueue(elapsedTime);
          var count = m_TransformSkyIdx.Count;
          if (interframeValue.Positions.Length != count) Array.Resize(ref interframeValue.Positions, count);
          buffer.Read(interframeValue.Positions);
        }break;
      }
    }

    public override void UpdateReplaying(float updateTime){
      ReplayFrameRecycler.Update(frame => Replay(frame.ElapsedTime, frame.Buffer));
      m_InterframeRecycler.Update(updateTime, (prev, next, rate) => {
        m_TransformSkyIdx.Each((index, pair) => {
          pair.Value.position = Vector3.Lerp(prev.Positions[index], next.Positions[index], rate);
        });
      });
    }
  }

  public class FinishSynchronizer : Synchronizer {
    public override byte SynchronizerType => SynchronizerTypes.Finish;

    public override bool Skippable => false;

    public override void UpdateReplaying(float updateTime){
      ReplayFrameRecycler.Update(updateTime, _ => ReplayingManager.Instance.StopReplaying());
    }
  }

  public class SynchroTest {
    static SynchroTest(){
      SynchronizerManager.Instance.Set(new ColorSynchronizer());
      SynchronizerManager.Instance.Set(new PositionSynchronizer());
      SynchronizerManager.Instance.Set(new FinishSynchronizer());
      foreach (Synchronizer synchronizer in SynchronizerManager.Instance.Synchronizers){
        Debug.Log($"{synchronizer}");
      }
    }

    private struct LinePosition {
      public Vector3 Start;

      public Vector3 End;

      public LinePosition(Vector3 start, Vector3 end){
        Start = start;
        End = end;
      }
    }

    private List<Frame> m_Frames = new List<Frame>();

    private Stream.BufferWriter m_BufferWriter = new Stream.BufferWriter(new DastBytes.Buffer());

    private void OnRecord(byte[] bytes){
      var frame = new Frame();
      frame.Read(bytes.AsSpan());
      m_Frames.Add(frame);
    }

    [UnityTest]
    public IEnumerator RecordReplayBufferTest(){
      SceneManager.LoadScene("RecordReplayBufferTest");
      yield return new WaitForSeconds(1);
      yield return Record();
      yield return Replay();
    }

    private void RecordColorSynchronizer(){
      var colorSynchronizer = SynchronizerManager.Instance.Get<ColorSynchronizer>(SynchronizerTypes.Color);
      colorSynchronizer.Record(0, new Color(0, 0, 0));
      colorSynchronizer.Record(1, new Color(1, 0, 0));
      colorSynchronizer.Record(2, new Color(0, 1, 0));
      colorSynchronizer.Record(3, new Color(0, 0, 1));
      colorSynchronizer.Record(4, new Color(0, 1, 1));
      colorSynchronizer.Record(5, new Color(1, 0, 1));
      colorSynchronizer.Record(6, new Color(1, 1, 0));
      colorSynchronizer.Record(7, new Color(1, 1, 1));
      for (var i = 0; i < 7; ++i){
        colorSynchronizer.Record(i + 0.5f, new Color(0.5f, 0.5f, 0.5f));
      }
    }

    private void RecordPositionSynchronizer(){
      var positionSynchronizer = SynchronizerManager.Instance.Get<PositionSynchronizer>(SynchronizerTypes.Position);
      var linePositions = new LinePosition[]{
        new LinePosition(new Vector3(-2.5f, -1, 0), new Vector3(2.5f, 1, 0)), // ↗
        new LinePosition(new Vector3(2.5f, -1, 0), new Vector3(-2.5f, 1, 0)), // ↖
        new LinePosition(new Vector3(-2.5f, 0, 0), new Vector3(2.5f, 0, 0)), // →
        new LinePosition(new Vector3(2.5f, 0, 0), new Vector3(-2.5f, 0, 0)), // ←
        new LinePosition(new Vector3(0, -1, 0), new Vector3(0, 1, 0)), // ↑
        new LinePosition(new Vector3(0, 1, 0), new Vector3(0, -1, 0)), // ↓
        new LinePosition(new Vector3(2.5f, 1, 0), new Vector3(-2.5f, -1, 0)), // ↙
        new LinePosition(new Vector3(-2.5f, 1, 0), new Vector3(2.5f, -1, 0)), // ↘
      };
      var count = 700;
      for (var i = 0; i <= count; ++i){
        var rate = (float)i / count;
        positionSynchronizer.Record(0.01f * i, linePositions.Select(x => Vector3.Lerp(x.Start, x.End, rate)).ToArray());
      }
    }

    private IEnumerator Record(){
      m_BufferWriter.Buffer.Clear();
      m_Frames.Clear();
      RecordingManager.Instance.OnRecord += OnRecord;
      RecordingManager.Instance.StartRecording();
      RecordColorSynchronizer();
      RecordPositionSynchronizer();
      RecordingManager.Instance.StopRecording();
      RecordingManager.Instance.OnRecord -= OnRecord;
      m_Frames.Sort((a, b) => Frame.Compare(a, b));
      foreach (Frame frame in m_Frames){
        Stream.Write(m_BufferWriter, frame);
      }
      yield return null;
    }

    private IEnumerator Replay(){
      ReplayingManager.Instance.StartReplaying();
      var asyncHandler = ReplayingManager.Instance.ReadEachFrameAsync(new Stream.BufferReader(m_BufferWriter.Buffer), 0.2f, TimeSpan.FromSeconds(0.01f));
      while (AsyncHandler.IsRunning(asyncHandler)){
        yield return new WaitForFixedUpdate();
      }
      var frame = new Frame(SynchronizerTypes.Finish, ReplayingManager.Instance.LastElapsedTime + 1);
      frame.Write(buffer => ReplayingManager.Instance.TryAddFrame(buffer.AsReadOnlySpan(), out _));
      Debug.LogWarning($"{ReplayingManager.Instance.ElapsedTimer} {frame.ToString("Finish")}");
      while (ReplayingManager.Instance.IsReplaying){
        yield return new WaitForFixedUpdate();
      }
      yield return null;
    }
  }
}