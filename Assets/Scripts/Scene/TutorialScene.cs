using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScene : MonoBehaviour
{
    [SerializeField]
    private BackGroundPanel _back;
    [SerializeField]
    private DialogPanel _dialog;
    [SerializeField]
    private QuestionPanel _question;
    [SerializeField]
    private AnswerPanel _answer;
    [SerializeField]
    private SkillButton _skill;
    [SerializeField]
    private AnswerContents _answerContents;

    [SerializeField]
    private GameObject[] _dialogbuttons;
    [SerializeField]
    private GameObject _okButton;
    [SerializeField]
    private GameObject _blockPanel;

    [SerializeField]
    private BindQA _qa;
    [SerializeField]
    private Image[] _imgs;
    [SerializeField]
    private Sprite[] _stelivesprites;
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip[] _audioClips;
    private CAudio _cAudio;

    private List<string> _talkname = new List<string>();
    private List<string> _reactions = null;
    private Reaction _ans = null;

    private List<int> _cnt = new List<int>();
    private Dictionary<string, short> _ansDict = new Dictionary<string, short>();
    private string _prevq;
    private short _preva;
    private short _dialogcnt = 0;

    private short _stage;
    private bool _bTalk;
    private bool _flag;
    private short _who;

    private Action<string, bool, short> _setAnswerCon;
    private Action<string> _removeDict;

    private CancellationTokenSource _cts;

    public void Init(short stage)
    {
        _stage = stage;
        _back.Init(DialogType.None, _stage);
        LoadString();
        _skill.Init(_stage);
        _imgs[0].sprite = _stelivesprites[4 * (short)_who];
        _removeDict -= ReturnToQA;
        _removeDict += ReturnToQA;
        _question.SendQ -= SearchAns;
        _question.SendQ += SearchAns;
        _answer.BAnswer -= BAnswer;
        _answer.BAnswer += BAnswer;
        _answerContents.Init(ref _setAnswerCon, ref _removeDict);
        _bTalk = false;
        _cts = new CancellationTokenSource();
        _reactions = new List<string>()
        {
            "절대 아냐!",
            "아냐",
            "중간이야",
            "맞아",
            "진짜 맞아!",
            "몰?루"
        };
        _cAudio = new CAudio(_audioSource, Sound.Effect);
        for (int i = 0; i < _dialogbuttons.Length; ++i)
            _dialogbuttons[i].gameObject.SetActive(false);
        SetDialogAsync().Forget();
        _flag = false;
    }

    private void ReturnToQA(string q)
    {

        _qa.q.Add(q);
        _qa.ans.answers.Add(_ansDict[q]);
        _ansDict.Remove(q);
        if (_dialogcnt == 5 && !_flag)
        {
            _bTalk = false;
            SetDialogAsync().Forget();
            _flag = true;
        }
    }

    private void LoadString()
    {
        TextAsset data = Resources.Load<TextAsset>($"Data/WhoAmI/{_stage}");
        _qa = JsonUtility.FromJson<QuestionLoad>(data.text).Load(ref _talkname);
        _who = _talkname[0].Contains("유니") ? (short)Stelive.Yuni : (short)Stelive.Kanna;
        TextAsset reaction = Resources.Load<TextAsset>($"Data/Reaction/{(Stelive)_who}");
        _ans = JsonUtility.FromJson<Reaction>(reaction.text);
        
        Resources.UnloadAsset(data);
        Resources.UnloadAsset(reaction);
    }

    private async void SearchAns(string q, bool skill = false)
    {
        await WaitAnsAsync(q, skill);
        if(_dialogcnt != _qa.dialog.Count)
            await SetDialogAsync();
        _skill.SetDialog.Invoke(true);
    }

    private async UniTask<bool> WaitAnsAsync(string q, bool skill)
    {
        try
        {
            bool clear = false;
            if (!skill)
            {
                int idx = _qa.q.IndexOf(q);
                _preva = _qa.ans.answers[idx];
                _prevq = q;
                _ansDict.Add(q, _preva);
                _qa.q.Remove(q);
                _qa.ans.answers.RemoveAt(idx);
            }

            int val = skill ? 0 : _dialogcnt * 40;
            int ansval = 0;

            if (val > 74)
            {
                if (val > 94)
                {
                    ansval = _ans.reactions.Count - 4;
                    clear = await _dialog.SetTextAsync(_talkname[0], _ans.reactions[ansval]);
                }

                else
                {
                    if (_preva < 2)
                        ansval = UnityEngine.Random.Range(2, 5);
                    else if (_preva > 2)
                        ansval = UnityEngine.Random.Range(0, 3);
                    else
                    {
                        _cnt.Add(0);
                        _cnt.Add(1);
                        _cnt.Add(3);
                        _cnt.Add(4);
                        ansval = _cnt[UnityEngine.Random.Range(0, _cnt.Count)];
                    }
                    clear = await _dialog.SetTextAsync(_talkname[0], _ans.reactions[ansval]);
                    _cnt.Clear();
                }
                _preva = (short)ansval;
                _setAnswerCon?.Invoke(string.Concat(q, "\n\t->", _reactions[ansval]), true, _preva == ansval ? (short)0 : (short)1);
            }

            else
            {
                if (skill)
                {
                    clear = await _dialog.SetTextAsync(_talkname[0],
                        _ans.reactions[_preva == _ansDict[q] ? 7 : 6]);
                    _imgs[0].sprite = _stelivesprites[4 * _who + (_preva == _ansDict[q] ? 2 : 3)];
                    _cAudio.PlaySound(_audioClips[_preva == _ansDict[q] ? (short)_who * 2 : (short)_who * 2 + 1], Sound.Effect);
                    _setAnswerCon?.Invoke(string.Concat(q, "\n\t->", _reactions[_ansDict[q]]), false, 2);
                }

                else
                {
                    ansval = _preva;
                    clear = await _dialog.SetTextAsync(_talkname[0], _ans.reactions[ansval]);
                    _setAnswerCon?.Invoke(string.Concat(q, "\n\t->", _reactions[ansval]), true, _preva == ansval ? (short)0 : (short)1);
                }
            }

            return clear;
        }
        catch
        {
            return true;
        }
    }

    private async UniTaskVoid WinAsync()
    {
        for (int i = 0; i < _dialogbuttons.Length; ++i)
            _dialogbuttons[i].gameObject.SetActive(false);
        SaveStage();
        MyAnswer(false);
        _dialog.SetTextAsync(_talkname[0], "헉! 어떻게 알았지?? 신기해!!").Forget();
        _imgs[0].sprite = _stelivesprites[_who * 4 + 1];
        Clear();
        await UniTask.Delay(TimeSpan.FromSeconds(2));
        --_stage;
        GameManager.Instance.SceneChangeAsync(Scenename.ScriptScene, _stage, DialogType.Success).Forget();
    }

    private void BAnswer(bool bans, string input)
    {
        if (bans)
        {
            if (_qa.ans.solutions.Contains(input))
            {
                WinAsync().Forget();
            }
            else
            {
                MyAnswer(false);
                _imgs[0].sprite = _stelivesprites[4 * _who + 2];
                _dialog.SetTextAsync(_talkname[0], _ans.reactions[_ans.reactions.Count - 1]).Forget();
            }
        }

        else
            MyAnswer(false);
    }

    public void UseSkill()
    {
        Skill().Forget();
    }

    private async UniTaskVoid Skill()
    {
        if (_dialog.gameObject.activeSelf && _dialogcnt == 3)
        {
            _imgs[1].sprite = _stelivesprites[8 + 2];
            _skill.SetDialog.Invoke(false);
            bool b = await _skill.UseSkill();
            if (b)
            {
                await _dialog.SetTextAsync(_talkname[1], "잘 생각해봐 아가씨~ 그게 맞아?");
                _dialog.AddTextAsync("...", 4.5f).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: _cts.Token);
                SearchAns(_prevq, true);
            }
            else
                _skill.SetDialog.Invoke(true);
            _imgs[1].sprite = _stelivesprites[8];
        }
    }

    private async UniTask<bool> SetDialogAsync()
    {
        short cnt = 0;
        if(_dialogcnt == 0)
        {
            _bTalk = true;
            await _dialog.SetTextAsync(_talkname[1], _qa.dialog[_dialogcnt].name[cnt]);
            ++cnt;
        }
        if(_bTalk)
            _okButton.SetActive(true);
        int len = _qa.dialog[_dialogcnt].name.Count;
        while (len > cnt)
        {
            await UniTask.WaitUntil(() => !_bTalk);
            _bTalk = true;
            await _dialog.SetTextAsync(_talkname[1], _qa.dialog[_dialogcnt].name[cnt]);
            ++cnt;
            if (len > cnt)
                _okButton.SetActive(true);
        }
        ++_dialogcnt;
        if (_dialogcnt != 3 && _dialogcnt != 5)
        {
            for (int i = 0; i < _dialogbuttons.Length; ++i)
                _dialogbuttons[i].gameObject.SetActive(true);
            if(_dialogcnt == _qa.dialog.Count)
                _dialogbuttons[0].gameObject.SetActive(false);
        }
        if (_dialogcnt == 5)
            _blockPanel.SetActive(false);
        return true;
    }

    public void OnDialog()
    {
        _okButton.SetActive(false);
        _bTalk = false;
    }

    public async void OnDialog(short cnt)
    {
        _bTalk = true;
        gameObject.SetActive(false);
        _bTalk = await _dialog.SetTextAsync(_talkname[1], _qa.dialog[_dialogcnt].name[cnt]);
        if(_qa.dialog[_dialogcnt].name.Count > cnt)
            gameObject.SetActive(true);
    }

    public void SetQ()
    {
        string[] qs = new string[3];
        _imgs[0].sprite = _stelivesprites[4 * _who];
        for (short i = 0; i < _qa.q.Count; ++i)
            _cnt.Add(i);
        int qnum = 3 < _qa.q.Count ? 3 : _qa.q.Count;
        for(short i = 0; i < qnum; ++i)
        {
            int r = _cnt[UnityEngine.Random.Range(0, _cnt.Count)];
            _cnt.Remove(r);
            qs[i] = _qa.q[r];
        }

        if (qnum != 0)
        {
            _question.SetText(qs);
            _cnt.Clear();
            _skill.SetDialog.Invoke(false);
            for (int i = 0; i < _dialogbuttons.Length; ++i)
                _dialogbuttons[i].gameObject.SetActive(false);
        }
    }

    public void MyAnswer(bool banswer = true)
    {
        _skill.SetDialog.Invoke(!banswer);
        _answer.gameObject.SetActive(banswer);
    }

    private void Clear()
    {
        _cts.Cancel();
        _cts.Dispose();
        _skill.Clear();
        _talkname.Clear();
        _reactions.Clear();
        _cnt.Clear();
        _ansDict.Clear();
        _ans.reactions.Clear();
        _imgs.Initialize();
        _stelivesprites.Initialize();
        _qa.ans.answers.Clear();
        _qa.ans.solutions.Clear();
        _qa.q.Clear();
        _qa.dialog.Clear();
        _stelivesprites.Initialize();
        _question.SendQ -= SearchAns;
        _answer.BAnswer -= BAnswer;
        _removeDict -= ReturnToQA;
        _answerContents.Clear();
        _cAudio.Clear();
    }

    private void SaveStage()
    {
        ++_stage;
        GameManager.Instance.SuccessSave(_stage);
    }
}
