using System;
using UnityEngine;

public class BgmPlayer : MonoBehaviour
{
    private CAudio _cAudio;
    [SerializeField]
    private AudioClip[] _mainclips;
    [SerializeField]
    private AudioClip[] _gameclips;
    [SerializeField]
    private AudioSource _audio;

    public static Action<float> Pitch;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        _cAudio = new CAudio(_audio, Sound.Bgm);
        DontDestroyOnLoad(gameObject);
        GameManager.ActiveScene -= SelectBgm;
        GameManager.ActiveScene += SelectBgm;
        GameManager.ChangeScene -= BgmVolumeFade;
        GameManager.ChangeScene += BgmVolumeFade;
        Pitch -= _cAudio.SetPitch;
        Pitch += _cAudio.SetPitch;
        _cAudio.PlaySound(_mainclips[(short)Scenename.StartScene], Sound.Bgm);
    }

    private void BgmVolumeFade(string prev, Scenename next)
    {
        if (next == Scenename.MainScene || next == Scenename.StartScene)
            _cAudio.StopSoundFade();
        else if (prev.Contains(Scenename.MainScene.ToString()) && next == Scenename.ScriptScene)
            _cAudio.StopSoundFade();
    }

    private void SelectBgm(Scenename next, short stage)
    {
        switch (next)
        {
            case Scenename.StartScene:
                _cAudio.PlaySound(_mainclips[(short)next], Sound.Bgm);
                break;

            case Scenename.MainScene:
                _cAudio.PlaySound(_mainclips[(short)next], Sound.Bgm);
                break;

            case Scenename.ScriptScene:
                _cAudio.PlaySound(_gameclips[stage % 2], Sound.Bgm);
                break;
        }
    }
}

