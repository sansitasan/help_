using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SkillEffectPanel : MonoBehaviour
{
    private Material _m;
    [SerializeField]
    private Image _effectImg;

    private float _duration;
    private CancellationTokenSource _cts = null;

    public void Init()
    {
        _m = gameObject.GetComponent<Image>().material;
        _duration = 0;
        _cts = new CancellationTokenSource();
    }

    public async UniTask<bool> Active()
    {
        try
        {
            gameObject.SetActive(true);
            _effectImg.gameObject.SetActive(true);
            while (_duration < 1.01f)
            {
                _effectImg.color = Color.Lerp(Color.clear, Color.black, _duration);
                _m.SetFloat("_CoolTime", _duration);
                await UniTask.Delay(TimeSpan.FromMilliseconds(10));
                _duration += 0.02f;
            }

            await UniTask.Delay(TimeSpan.FromMilliseconds(500));

            while (_duration > 0)
            {
                _effectImg.color = Color.Lerp(Color.clear, Color.black, _duration);
                _m.SetFloat("_CoolTime", _duration);
                await UniTask.Delay(TimeSpan.FromMilliseconds(10));
                _duration -= 0.04f;
            }

            gameObject.SetActive(false);
            _effectImg.gameObject.SetActive(false);
            return true;
        }
        catch
        {
            return true;
        }
    }

    public void Clear()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
