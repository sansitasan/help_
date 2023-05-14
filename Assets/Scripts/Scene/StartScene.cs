using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class StartScene : AllScene
{
    private bool _bactive = false;
    [SerializeField]
    private TextMeshProUGUI _text;
    [SerializeField]
    private RectTransform[] _imgts;
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip[] _audioClip;
    private CancellationTokenSource _cts = null;
    private CancellationTokenSource _ctsimg = null;
    private float _a;
    private CAudio _cAudio;

    private void Start()
    {
        Init().Forget();
    }

    private async UniTaskVoid Init()
    {
        _cts = new CancellationTokenSource();
        _ctsimg = new CancellationTokenSource();
        _a = 1f;
        _cAudio = new CAudio(_audioSource, Sound.Effect);
        Blink(0.03f).Forget();
        this.FixedUpdateAsObservable().Where(_ => Input.anyKeyDown && !_bactive)
            .Subscribe(_ => GameStart());

        for (int i = 0; i < _imgts.Length; ++i)
        {
            ShakeImg(i).Forget();
            await UniTask.Delay(TimeSpan.FromMilliseconds(240));
        }
    }

    private async UniTaskVoid ShakeImg(int i, float speed = 0.2f)
    {
        float z = -2.5f;
        while (true)
        {
            while (z < 2.5)
            {
                z += speed;
                _imgts[i].Rotate(Vector3.forward * speed);
                await UniTask.Delay(TimeSpan.FromMilliseconds(20) * Time.deltaTime, cancellationToken: _ctsimg.Token);
            }

            while (z > -2.5)
            {
                z -= speed;
                _imgts[i].Rotate(Vector3.back * speed);
                await UniTask.Delay(TimeSpan.FromMilliseconds(20) * Time.deltaTime, cancellationToken: _ctsimg.Token);
            }
        }
    }

    private async UniTaskVoid Blink(float speed, short time = -1)
    {
        try
        {
            while (true)
            {
                while (_a > 0)
                {
                    _a -= speed;
                    _text.alpha = _a;
                    await UniTask.Delay(TimeSpan.FromMilliseconds(10) * Time.deltaTime, cancellationToken: _cts.Token);
                }

                while (_a < 1)
                {
                    _a += speed;
                    _text.alpha = _a;
                    await UniTask.Delay(TimeSpan.FromMilliseconds(10) * Time.deltaTime, cancellationToken: _cts.Token);
                }

                if (time > 0)
                {
                    --time;
                    if (time == 0)
                    {
                        _ctsimg.Cancel();
                        _ctsimg.Dispose();
                        break;
                    }
                }
            }
        }

        catch
        {
            return;
        }
    }

    private void GameStart()
    {
        _bactive = true;
        _cAudio.PlaySound(_audioClip[UnityEngine.Random.Range(0, _audioClip.Length)], Sound.Effect);
        
        _audioClip.Initialize();
        _cAudio.Clear();
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        Blink(0.25f, 4).Forget();
        OnNextScene(Scenename.MainScene);
    }
}
