using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    private Material _m;
    private IObservable<Unit> _addtime;
    private IDisposable _cooldown;
    private float _duration;
    private bool _bcool;
    public Action<bool> SetDialog;

    private CancellationTokenSource _cts = null;
    [SerializeField]
    private SkillEffectPanel _effectPanel;

    public void Init(short stage)
    {
        _bcool = true;
        _m = gameObject.GetComponent<Image>().material;
        _duration = 1;
        _m.SetFloat("_Stage", stage % 2);
        _addtime = this.FixedUpdateAsObservable().Where(_ => _duration <= 1.01f);
        _cooldown = _addtime.Subscribe(_ =>
            _m.SetFloat("_CoolTime", _duration)
        );
        _cts = new CancellationTokenSource();
        _effectPanel.Init();
        SetDialog -= CanSkill;
        SetDialog += CanSkill;
    }

    private async UniTaskVoid AddCoolTime()
    {
        while(_duration < 1.01f)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(100), cancellationToken: _cts.Token);
            _duration += 0.0035f;
        }
        _bcool = true;
    }

    public async UniTask<bool> UseSkill()
    {
        try
        {
            bool b = false;
            if (_duration > 0.99f && _bcool)
            {
                _duration = 0;
                _bcool = false;
                AddCoolTime().Forget();
                b = await _effectPanel.Active();
            }

            return b;
        }

        catch
        {
            return false;
        }
    }

    private void CanSkill(bool b)
    {
        if (_duration > 0.99f && b)
            _m.SetFloat("_BCan", 2);
        else
            _m.SetFloat("_BCan", 0);
    }

    public void Clear()
    {
        SetDialog -= CanSkill;
        _cooldown.Dispose();
        _cts.Cancel();
        _cts.Dispose();
        _effectPanel.Clear();
    }
}
