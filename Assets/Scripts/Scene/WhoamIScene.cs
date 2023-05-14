using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public enum Stelive
{
    Yuni,
    Kanna
}

public class WhoamIScene : MonoBehaviour
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
    private int _time = 180;
    [SerializeField]
    private UnityEngine.UI.Slider _slider;

    [SerializeField]
    private TextMeshProUGUI _failnote;
    [SerializeField]
    private TextMeshProUGUI _correctAns;
    [SerializeField]
    private GameObject _gameOverPanel;
    [SerializeField]
    private GameObject[] _dialogbuttons;

    [SerializeField]
    private BindQA _qa;
    [SerializeField]
    private UnityEngine.UI.Image[] _imgs;
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

    private short _stage;
    private ushort _failcnt;
    private bool _bTalk;
    private bool _bstop;
    private short _who;

    private Action<string, bool, short> _setAnswerCon;
    private Action<string> _removeDict;

    private CancellationTokenSource _cts;

    public void Init(short stage)
    {
        _stage = stage;
        _back.Init(DialogType.None, _stage);
        _failcnt = GameManager.Instance.Stage.failcnt[_stage];
        LoadString();
        _skill.Init(_stage);
        _imgs[0].sprite = _stelivesprites[4 * _who];
        _removeDict -= ReturnToQA;
        _removeDict += ReturnToQA;
        _question.SendQ -= SearchAns;
        _question.SendQ += SearchAns;
        _answer.BAnswer -= BAnswer;
        _answer.BAnswer += BAnswer;
        _answer.Init(_failcnt, _qa.anslist);
        _answerContents.Init(ref _setAnswerCon, ref _removeDict);
        SetQ();
        _bTalk = false;
        _bstop = false;
        _cts = new CancellationTokenSource();
        _slider.maxValue = _time;
        _slider.value = _time;
        TimeTick().Forget();
        _cAudio = new CAudio(_audioSource, Sound.Effect);
        var observ = this.FixedUpdateAsObservable().Where(_ => _slider.value <= 60);
        //pitch에 effect사운드 시간이 없다는 것을 알려주기
        var pitch = observ.Subscribe(_ => 
        { 
            BgmPlayer.Pitch.Invoke(1.15f);
            _cAudio.PlaySound(_audioClips[4], Sound.Effect);
        });

        observ.Subscribe(_ => pitch.Dispose());
        _reactions = new List<string>()
        {
            "절대 아냐!",
            "아냐",
            "중간이야",
            "맞아",
            "진짜 맞아!",
            "몰?루"
        };
    }

    private void ReturnToQA(string q)
    {
        _qa.q.Add(q);
        _qa.ans.answers.Add(_ansDict[q]);
        _ansDict.Remove(q);
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
        for (int i = 0; i < _dialogbuttons.Length; ++i)
            _dialogbuttons[i].gameObject.SetActive(true);
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

            int val = skill ? 0 : UnityEngine.Random.Range(0, 100);
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
                _setAnswerCon?.Invoke(string.Concat(q, "\n\t->", _reactions[ansval]), true, _preva == ansval ? (short)0 : (short)1);
                _preva = (short)ansval;
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
        SaveStage(true);
        MyAnswer(false);
        _dialog.SetTextAsync(_talkname[0], "헉! 어떻게 알았지?? 신기해!!").Forget();
        _imgs[0].sprite = _stelivesprites[_who * 4 + 1];
        Clear();
        --_stage;
        await UniTask.Delay(TimeSpan.FromSeconds(2));
        GameManager.Instance.SceneChangeAsync(Scenename.ScriptScene, _stage, DialogType.Success).Forget();
    }

    private void BAnswer(bool bans, string input)
    {
        if (bans)
        {
            if (_qa.ans.solutions.Contains(input))
            {
                _bstop = true;
                WinAsync().Forget();
            }
            else
            {
                MyAnswer(false);
                _imgs[0].sprite = _stelivesprites[4 * _who + 2];
                _dialog.SetTextAsync(_talkname[0], _ans.reactions[_ans.reactions.Count - 1]).Forget();
                _slider.value -= 20;
                ShakeSlider().Forget();
            }
        }

        else
            MyAnswer(false);
    }

    private async UniTaskVoid ShakeSlider(float time = 0.5f, float amount = 10.0f)
    {
        Transform t = _slider.gameObject.transform;
        Vector2 startpos = t.position;
        while (time > 0.0f)
        {
            t.position = startpos + new Vector2(UnityEngine.Random.Range(-amount, amount), 
                UnityEngine.Random.Range(-amount, amount));
            await UniTask.Delay(TimeSpan.FromMilliseconds(10) * Time.deltaTime);
            t.position = startpos;
            time -= 0.025f;
        }
    }

    public void UseSkill()
    {
        Skill().Forget();
    }

    private async UniTaskVoid Skill()
    {
        if (_dialog.gameObject.activeSelf)
        {
            //타이무 스토뿌
            _bstop = true;
            for (int i = 0; i < _dialogbuttons.Length; ++i)
                _dialogbuttons[i].gameObject.SetActive(false);
            _imgs[1].sprite = _stelivesprites[8 + 2];
            _skill.SetDialog.Invoke(false);
            bool b = await _skill.UseSkill();
            if (b)
            {
                await _dialog.SetTextAsync(_talkname[1], "잘 생각해봐 아가씨~ 그게 맞아?");
                _dialog.AddTextAsync("...", 4.5f).Forget();
                await UniTask.Delay(TimeSpan.FromMilliseconds(2000), cancellationToken: _cts.Token);
                SearchAns(_prevq, true);
            }
            else
                _skill.SetDialog.Invoke(true);
            _imgs[1].sprite = _stelivesprites[8];
            for (int i = 0; i < _dialogbuttons.Length; ++i)
                _dialogbuttons[i].gameObject.SetActive(true);
            _bstop = false;
        }
    }

    public async void SetQ()
    {
        _imgs[0].sprite = _stelivesprites[4 * _who];
        string[] qs = new string[3];
        
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

        else if(!_bTalk)
        {
            _bTalk = true;
            _bTalk = await _dialog.SetTextAsync(_talkname[1], "(더 이상 물어볼 질문이 없는데... 다시 질문하거나 정답을 맞춰볼까?)");
        }
    }

    public void MyAnswer(bool banswer = true)
    {
        _skill.SetDialog.Invoke(!banswer);
        _answer.ActiveObject(banswer);
    }

    private async UniTaskVoid TimeTick()
    {
        try
        {
            while (_slider.value > 0)
            {
                if(!_bstop && !OptionPanel.instance.gameObject.activeSelf)
                    _slider.value--;
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
            //유칸 슬픈 표정
            _imgs[0].sprite = _stelivesprites[_who * 4 + 2];
            SaveStage(false);
            _skill.gameObject.SetActive(false);
            _slider.gameObject.SetActive(false);
            _answer.gameObject.SetActive(false);
            _question.gameObject.SetActive(false);
            _gameOverPanel.SetActive(true);
            for(int i = 0; i < _qa.ans.solutions.Count - 1; ++i)
                _correctAns.text += _qa.ans.solutions[i] + ", ";
            _correctAns.text += _qa.ans.solutions[_qa.ans.solutions.Count - 1];
            if (2 > _failcnt)
                _failnote.text = $"※앞으로 {2 - _failcnt}번 실패할 경우 정답리스트가 공개됩니다.";
            else
                _failnote.text = $"※이후 같은 스테이지의 정답 입력창에 정답리스트가 공개됩니다.";
            Clear();
        }
        catch(Exception ex)
        {
            if (ex.IsOperationCanceledException())
                return;
        }
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
        _stelivesprites.Initialize();
        _question.SendQ -= SearchAns;
        _answer.BAnswer -= BAnswer;
        _removeDict -= ReturnToQA;
        _answerContents.Clear();
        _cAudio.Clear();
    }

    private void SaveStage(bool bsuc)
    {
        if (bsuc)
        {
            ++_stage;
            GameManager.Instance.SuccessSave(_stage);
        }
        else
            GameManager.Instance.FailSave(_stage);
    }

    public void GameOverButton()
    {
        GameManager.Instance.SceneChangeAsync(Scenename.ScriptScene, _stage, DialogType.Fail).Forget();
    }
}
