using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackGroundPanel : MonoBehaviour
{
    [SerializeField]
    private Image _back;
    [SerializeField]
    private Sprite[] _backSprite;

    public void Init(DialogType type, short stage)
    {
        if (stage != 3 || type != DialogType.Success)
            _back.sprite = _backSprite[stage % 2];
        else
            _back.sprite = _backSprite[2];
    }
}
