using System.Text;
using UnityEngine;
using Unity.Profiling;

namespace MyGame.Performance
{
    /// <summary>
    /// 运行时性能探针：统计 FPS、GC 分配、最低帧时间，便于自动化回归验证。
    /// 仅在 Development Build / Editor 下生效。
    /// </summary>
    public class PerfProbe : MonoBehaviour
    {
        [SerializeField] private bool showOverlay = true;
        [SerializeField] private int sampleWindow = 120;

        private ProfilerRecorder _gcAllocRecorder;
        private ProfilerRecorder _systemMemoryRecorder;
        private float[] _frameTimes;
        private int _index;
        private int _count;
        private readonly StringBuilder _sb = new StringBuilder(256);

        private void OnEnable()
        {
            _frameTimes = new float[sampleWindow];
            _index = 0;
            _count = 0;

            _gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            _systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        }

        private void OnDisable()
        {
            _gcAllocRecorder.Dispose();
            _systemMemoryRecorder.Dispose();
        }

        private void Update()
        {
            _frameTimes[_index] = Time.unscaledDeltaTime;
            _index = (_index + 1) % _frameTimes.Length;
            if (_count < _frameTimes.Length)
            {
                _count++;
            }
        }

        private void OnGUI()
        {
            if (!showOverlay || _count == 0)
            {
                return;
            }

            float sum = 0f;
            float min = float.MaxValue;
            float max = 0f;
            for (int i = 0; i < _count; i++)
            {
                sum += _frameTimes[i];
                if (_frameTimes[i] < min) min = _frameTimes[i];
                if (_frameTimes[i] > max) max = _frameTimes[i];
            }

            float avg = sum / _count;
            float avgFps = avg > 0f ? 1f / avg : 0f;

            long gcBytes = _gcAllocRecorder.LastValue;
            long usedMem = _systemMemoryRecorder.LastValue;

            _sb.Clear();
            _sb.Append($"FPS: {avgFps:F1}  (min {1f / min:F1} / max {1f / max:F1})\n");
            _sb.Append($"Frame: {avg * 1000f:F2} ms\n");
            _sb.Append($"GC Alloc/frame: {gcBytes} B\n");
            _sb.Append($"Used Memory: {usedMem / (1024 * 1024)} MB");

            GUI.Label(new Rect(10, 10, 420, 100), _sb.ToString());
        }
    }
}
