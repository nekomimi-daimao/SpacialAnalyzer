using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

namespace SpacialAnalyzer.Scripts.Record
{
    public sealed class BehaviourPlayBack : MonoBehaviour
    {
        private readonly RecordAndPlayBack _recordAndPlayBack = new RecordAndPlayBack();

        [SerializeField]
        private bool AutoStart = true;

        private void Start()
        {
            if (AutoStart)
            {
                StartPlayBack().Forget();
            }
        }

        [SerializeField]
        private string FileName = null;

        public async UniTask StartPlayBack()
        {
            await UniTask.Yield();
            var cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
            await UniTask.WaitUntil(() => ARSession.state == ARSessionState.Ready, cancellationToken: cancellationTokenOnDestroy);
            await UniTask.Yield();
            var path = Path.Combine(Application.persistentDataPath, "record", FileName) + ".mp4";
            var status = await _recordAndPlayBack.StartPlayback(path);
            Debug.Log($"{nameof(BehaviourPlayBack)} {nameof(StartPlayBack)} {status}");

            if (status == ArStatus.Success)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = null;
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenOnDestroy);
                RepeatLoop(path, _cancellationTokenSource.Token).Forget();
            }
        }

        private CancellationTokenSource _cancellationTokenSource = null;

        private async UniTask RepeatLoop(string path, CancellationToken token)
        {
            while (true)
            {
                await UniTask.Yield();
                if (token.IsCancellationRequested)
                {
                    break;
                }

                if (_recordAndPlayBack.PlaybackStatus == ArPlaybackStatus.Ok)
                {
                    continue;
                }
                if (_recordAndPlayBack.PlaybackStatus == ArPlaybackStatus.Finished)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                    var status = await _recordAndPlayBack.StartPlayback(path);
                    if (status != ArStatus.Success)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public async UniTask StopPlayBack()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            var status = await _recordAndPlayBack.StopPlayBack();
        }
    }
}
