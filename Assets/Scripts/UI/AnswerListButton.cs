using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnswerListButton : MonoBehaviour
{
    private bool _onoff = true;
    [SerializeField]
    private RectTransform[] _ts;
    [SerializeField]
    private TextMeshProUGUI _text;

    public void OnOff()
    {
        _onoff = !_onoff;

        if (_onoff)
        {
            for (int i = 0; i < _ts.Length; ++i)
            {
                _ts[i].offsetMin += Vector2.right * 200;
                _ts[i].offsetMax += Vector2.right * 200;
            }
            _text.text = "<";
        }

        else
        {
            for (int i = 0; i < _ts.Length; ++i)
            {
                _ts[i].offsetMin -= Vector2.right * 200;
                _ts[i].offsetMax -= Vector2.right * 200;
            }
            _text.text = ">";
        }
    }
}
