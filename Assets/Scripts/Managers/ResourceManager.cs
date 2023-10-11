using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Backgrounds
{
    Stage0 = 1000,
    Stage0_STS = 1001,
    Stage1 = 2000,
    Stage1_CIB = 2001,
    Stage1Win = 2100,
    Stage1Fail = 2200,
    Stage1Fail_1 = 2201,
    Stage1Fail_2 = 2202,
    Stage2 = 3000,
    Stage2_Skull = 3001,
    Stage2Win = 3100,
    Stage2Win_1 = 3101,
    Stage2Win_2 = 3102,
    Stage2Win_3 = 3103,
    Stage2Fail = 3200,
    Stage3 = 4000,
    Stage3_OnlyUp = 4001,
    Stage3Win = 4100,
    Stage3Win_1 = 4101,
    Stage3Fail = 4200,
    Stage3Fail_1 = 4201,
    Stage4 = 5000,
    Stage5 = 6000
}

public enum Stages
{
    Stage0 = 0,
    Stage1 = 1,
    Stage2 = 2,
    Stage3 = 3,
    Stage4 = 4,
    Stage5 = 5,
}

public class ResourceManager
{
    public Dictionary<int, Sprite> BackGroundSprites { get => _backGroundSprites; }
    private Dictionary<int, Sprite> _backGroundSprites = new Dictionary<int, Sprite>();

    public Dictionary<int, AudioClip> EffectClips { get => _effectClips; }
    private Dictionary<int, AudioClip> _effectClips = new Dictionary<int, AudioClip>();

    public Dictionary<string,TextAsset> Dialog { get => _dialog; }
    private Dictionary<string, TextAsset> _dialog = new Dictionary<string, TextAsset>();

    public Dictionary<string, TextAsset> WhoAmI { get => _whoAmI; }
    private Dictionary<string, TextAsset> _whoAmI = new Dictionary<string, TextAsset>();

    public ResourceManager()
    {
        var backGroundsEnum = Enum.GetValues(typeof(Backgrounds));

        foreach (Backgrounds backGround in backGroundsEnum) {
            if ((int)backGround % 100 != 0)
            {
                string path = $"BackGrounds/{(int)backGround}";
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                    _backGroundSprites.Add((int)backGround, sprite);

                path = $"Sounds/Effects/{(int)backGround}";
                AudioClip clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                    _effectClips.Add((int)backGround, clip);
            }
        }

        var dialogs = Enum.GetValues(typeof(DialogType));
        var stages = Enum.GetValues(typeof(Stages));

        foreach (Stages stage in stages)
        {
            foreach (DialogType dialog in dialogs)
            {
                string path = $"Data/{dialog}/Tabii/{stage}";
                TextAsset text = Resources.Load<TextAsset>(path);
                if (text != null)
                    _dialog.Add(path, text);
            }
            string whopath = $"Data/WhoAmI/Tabii/{(int)stage}";
            TextAsset whotext = Resources.Load<TextAsset>(whopath);
            if (whotext != null)
                _whoAmI.Add(whopath, whotext);
        }
    }
}
