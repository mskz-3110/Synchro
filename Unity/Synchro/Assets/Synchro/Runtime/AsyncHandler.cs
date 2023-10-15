using System.Threading;
using System.Threading.Tasks;

namespace Synchro {
  public class AsyncHandler {
    static public bool IsRunning(Task task) => !task?.IsCompleted ?? false;
    static public bool IsRunning(AsyncHandler asyncHandler) => IsRunning(asyncHandler.m_Task);

    static public bool IsCanceled(CancellationTokenSource cts) => cts?.IsCancellationRequested ?? false;
    static public bool IsCanceled(AsyncHandler asyncHandler) => IsCanceled(asyncHandler.m_CancellationTokenSource);

    public delegate void TaskEvent(CancellationTokenSource cts);

    private Task m_Task;

    private CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    public AsyncHandler(TaskEvent onTask){
      m_Task = Task.Run(() => onTask.Invoke(m_CancellationTokenSource), m_CancellationTokenSource.Token);
    }

    public void Cancel(){
      m_CancellationTokenSource.Cancel();
    }
  }
}