using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types.Events;

namespace ReplayOverlay
{
    public sealed class ObsConnector
    {
        private readonly OBSWebsocket _obs = new();
        private readonly int _port;
        private readonly string _password;

        public event Action<bool>? ReplayBufferStateChanged;
        public event Action? ReplayBufferSaved;
        public event Action? Connected;

        public ObsConnector(int port, string password)
        {
            _port = port;
            _password = password;
            _obs.Connected += (_, _) => Connected?.Invoke();
            _obs.ReplayBufferStateChanged += (_, args) => ReplayBufferStateChanged?.Invoke(args.OutputState.IsActive);
            _obs.ReplayBufferSaved += (_, _) => ReplayBufferSaved?.Invoke();
        }

        public async Task ConnectAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            void Handler(object? sender, EventArgs e)
            {
                _obs.Connected -= Handler;
                tcs.TrySetResult(true);
            }
            _obs.Connected += Handler;
            _obs.ConnectAsync($"ws://127.0.0.1:{_port}", _password);
            await tcs.Task;
        }

        public bool GetReplayBufferStatus() =>
            _obs.GetReplayBufferStatus();

        public Task EnsureReplayBufferRunningAsync() => Task.Run(() =>
        {
            try
            {
                if (!_obs.GetReplayBufferStatus())
                    _obs.StartReplayBuffer();
            }
            catch (ErrorResponseException ex) when (ex.ErrorCode == 500)
            {
                Debug.WriteLine($"StartReplayBuffer ignored (500): {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting replay buffer: {ex}");
            }
        });

        public void StopReplayBuffer()
        {
            try
            {
                _obs.StopReplayBuffer();
            }
            catch { }
        }
    }
}
