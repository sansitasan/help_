using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TempScene : MonoBehaviour
{
    [SerializeField]
    private Sprite[] _stelive;
    [SerializeField]
    private Image _img;
    [SerializeField]
    private TextMeshProUGUI _text;

    void Start()
    {
        _img.sprite = _stelive[Random.Range(0, _stelive.Length)];
    }
}
