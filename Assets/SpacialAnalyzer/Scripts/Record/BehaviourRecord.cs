#if UNITY_ANDROID || UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;

namespace SpacialAnalyzer.Scripts.Record
{
    public sealed class BehaviourRecord : MonoBehaviour
    {
        [SerializeField]
        private Button _buttonStart = null;

        [SerializeField]
        private Button _buttonStop = null;

        [SerializeField]
        private Text _textStatus = null;

        private readonly RecordAndPlayBack _recordAndPlayBack = new RecordAndPlayBack();

        private void Start()
        {
            if (_buttonStart != null)
            {
                _buttonStart.onClick.AddListener(OnStart);
            }
            if (_buttonStop != null)
            {
                _buttonStop.onClick.AddListener(OnStop);
            }
        }

        private async void OnStart()
        {
            var status = await _recordAndPlayBack.StartRecord();

            if (_textStatus != null)
            {
                _textStatus.text = $"start : {status}";
            }
        }

        private async void OnStop()
        {
            var status = await _recordAndPlayBack.StopRecord();

            if (_textStatus != null)
            {
                _textStatus.text = $"stop : {status}";
            }
        }
    }
}

#endif
