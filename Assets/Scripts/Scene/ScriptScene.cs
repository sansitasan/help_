using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScriptScene : AllScene
{
    [SerializeField]
    private BackGroundPanel _back;
    [SerializeField]
    private DialogPanel _dialog;
    [SerializeField]
    private Image[] _imgs;
    [SerializeField]
    private Sprite[] _stelivesprites;


    private IObservable<Unit> _click = null;
    private IDisposable _talk = null;

    private bool _btalk = false;

    private List<string> _nametext = new List<string>();
    private List<string> _dialogtext = new List<string>();
    private List<string> _talkname = new List<string>();
    private List<short[]> _nameNstate = new List<short[]>();

    private int cnt = 0;
    private short _stage;

    private DialogType _dialogType;
    private short _who;

    public void Init(DialogType type, short stage)
    {
        _stage = stage;
        _back.Init(type, _stage);
        _dialogType = type;
        LoadString(type, _stage);
        _who = _talkname[0].Contains("유니") ? (short)Stelive.Yuni : (short)Stelive.Kanna;
        _imgs[0].sprite = _stelivesprites[4 * _who];
        _click = this.FixedUpdateAsObservable()
            .Where(_ => 
            Input.anyKeyDown && !_btalk && !OptionPanel.instance.gameObject.activeSelf);

        _talk = _click
            .Subscribe(_ =>Script());
        Script();
    }

    private async void Script()
    {
        if (cnt >= _nametext.Count)
        {
            if (cnt == _nametext.Count)
                ++cnt;
            else
                Clear();
        }

        else
        {
            _btalk = true;
            SetTexture(_nameNstate[cnt][0], _nameNstate[cnt][1]);
            _btalk = await _dialog.SetTextAsync(_nametext[cnt], _dialogtext[cnt]);
            ++cnt;
        }
    }

    private void SetTexture(short name, short state)
    {

        switch (name)
        {
            case 0:
                _imgs[1].color = Color.gray;
                _imgs[0].color = Color.white;
                _imgs[0].sprite = _stelivesprites[4 * _who + state];
                break;
            case 1:
                _imgs[0].color = Color.gray;
                _imgs[1].color = Color.white;
                _imgs[1].sprite = _stelivesprites[8 + state];
                break;
            case 2:
                _imgs[1].color = Color.white;
                _imgs[0].color = Color.white;
                _imgs[0].sprite = _stelivesprites[4 * _who + state];
                _imgs[1].sprite = _stelivesprites[8 + state];
                break;
            case 3:
                _imgs[0].color = Color.gray;
                _imgs[1].color = Color.gray;
                _imgs[0].sprite = _stelivesprites[4 * _who + state];
                _imgs[1].sprite = _stelivesprites[8 + state];
                break;
        }
    }

    private void Clear()
    {
        _talk.Dispose();
        _talk = null;
        _click = null;
        _nametext.Clear();
        _dialogtext.Clear();
        _talkname.Clear();
        _nameNstate.Clear();
        if (_dialogType == DialogType.Success || _dialogType == DialogType.Fail)
            OnNextScene(Scenename.MainScene);
        else
            OnNextScene(Scenename.WhoamIScene, _stage);
    }

    private void LoadString(DialogType type, int stage)
    {
        TextAsset data = Resources.Load<TextAsset>($"Data/{type}/Stage{stage}");

        JsonUtility.FromJson<DataLoad>(data.text).Load(_nametext, _dialogtext, ref _talkname, _nameNstate);

        Resources.UnloadAsset(data);
    }
}
