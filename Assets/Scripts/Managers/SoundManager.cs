using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SoundManager
{
    public SoundVolume Volume { get => _volume; }
    private SoundVolume _volume;

    public event Action<float> BGMAction;
    public event Action<float> EffectAction;

    public SoundManager()
    {
        _volume = LoadVolume(Path.Combine(Application.persistentDataPath, "volume"));
        BGMAction?.Invoke(_volume.BGM);
        EffectAction?.Invoke(_volume.Effect);
    }

    public void SaveSound(SoundVolume vol)
    {
        string voldata = JsonUtility.ToJson(vol);
        string path = Path.Combine(Application.persistentDataPath, "volume");
        File.WriteAllText(path, voldata);
        _volume = vol;
        BGMAction?.Invoke(_volume.BGM);
        EffectAction?.Invoke(_volume.Effect);
    }

    public void SetVolumeTemporary(float vol, Sound type)
    {
        if (type == Sound.Bgm)
            BGMAction?.Invoke(vol);
        else
            EffectAction?.Invoke(vol);
    }

    private SoundVolume LoadVolume(string path)
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
}
