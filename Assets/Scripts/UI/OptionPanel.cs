using System;
using System.IO;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct SoundVolume
{
    public float BGM;
    public float Effect;

    public SoundVolume(float b = 1, float e = 1)
    {
        BGM = b;
        Effect = e;
    }
}

public class OptionPanel : MonoBehaviour
{
    private CAudio _cAudio;

    [SerializeField]
    private AudioSource _audio;
    [SerializeField]
    private AudioClip[] _clips;

    public static OptionPanel instance { get; private set; }

    [SerializeField]
    private Slider _Bgm;
    [SerializeField]
    private Slider _Effect;

    private float _prevbgm;
    private float _preveffect;

    public static Action<float> BgmAction;
    public static Action<float> EffectAction;
    public static Action<Sound> RequestAction { get; private set; }

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        _prevbgm = _Bgm.value;
        _preveffect = _Effect.value;
    }

    private void Init()
    {
        instance = this;
        SetOption();
        RequestAction -= RequestSound;
        RequestAction += RequestSound;
        _cAudio = new CAudio(_audio, Sound.Effect);
        this.FixedUpdateAsObservable().Where(_ => 
        (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) && gameObject.activeSelf)
            .Subscribe(_ => CancelClick());
        DontDestroyOnLoad(gameObject.transform.parent.gameObject);
        gameObject.SetActive(false);
    }

    private void RequestSound(Sound type)
    {
        if (type == Sound.Effect)
            EffectAction.Invoke(_Effect.value);
        else
            BgmAction.Invoke(_Bgm.value);
    }

    public void SetOption()
    {
        SoundVolume v = LoadVolume(Path.Combine(Application.persistentDataPath, "volume"));
        _Bgm.value = v.BGM;
        _Effect.value = v.Effect;
        _prevbgm = _Bgm.value;
        _preveffect = _Effect.value;
        //사운드 인보크로 알리기
        BgmAction?.Invoke(_Bgm.value);
        EffectAction?.Invoke(_Effect.value);
    }

    public void SaveClick()
    {
        SaveVolume(new SoundVolume(_Bgm.value, _Effect.value));
        gameObject.SetActive(false);
    }

    public void CancelClick()
    {
        _Bgm.value = _prevbgm;
        _Effect.value = _preveffect;
        BgmAction?.Invoke(_Bgm.value);
        EffectAction?.Invoke(_Effect.value);
        gameObject.SetActive(false);
    }

    public void SaveVolume(SoundVolume vol)
    {
        string voldata = JsonUtility.ToJson(vol);
        string path = Path.Combine(Application.persistentDataPath, "volume");
        File.WriteAllText(path, voldata);
    }

    public SoundVolume LoadVolume(string path)
    {
        if (File.Exists(path))
        {
            string voldata = File.ReadAllText(path);
            return JsonUtility.FromJson<SoundVolume>(voldata);
        }
        else
        {
            return new SoundVolume(0.15f, 0.5f);
        }
    }

    public void EffectSliderPointUp()
    {
        _cAudio.PlaySound(_clips[UnityEngine.Random.Range(0, _clips.Length)], Sound.Effect);
    }
}
