using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Stage
{
    public short stage;
    public ushort[] failcnt;

    public Stage(short stage)
    {
        this.stage = stage;
        failcnt = new ushort[5] { 0, 0, 0, 0, 0 };
    }
}

public class MainScene : AllScene
{
    [SerializeField]
    private Button[] _stage;
    [SerializeField]
    private Material _backm;
    private CancellationTokenSource _cts;
    [SerializeField]
    private GameObject _info;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _cts = new CancellationTokenSource();
        FlowBackground(0.005f).Forget();
        short stage = GameManager.Instance.Stage.stage;
        short max = stage < (short)4 ? stage : (short)4;
        for (short i = 0; i <= max; ++i)
        {
            _stage[i].gameObject.SetActive(true);
        }
        GameManager.ChangeScene -= Clear;
        GameManager.ChangeScene += Clear;
    }

    private void Clear(string s, Scenename scene)
    {
        GameManager.ChangeScene -= Clear;
        _cts.Cancel();
    }

    private async UniTaskVoid FlowBackground(float speed)
    {
        try
        {
            Vector2 offset = new Vector2(0, 0);
            Vector2 addspeed = new Vector2(speed, speed);
            while (true)
            {
                offset += addspeed;
                _backm.SetTextureOffset("_MainTex", offset);
                await UniTask.Delay(TimeSpan.FromMilliseconds(10) * Time.deltaTime, cancellationToken: _cts.Token);
            }
        }

        catch
        {
            _cts.Dispose();
            return;
        }
    }

    public void SetInfoPanel(bool bactive)
    {
        _info.SetActive(bactive);
    }

    public void BackToStart()
    {
        GameManager.Instance.SceneChangeAsync(Scenename.StartScene).Forget();
    }
}
