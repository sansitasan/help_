using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class AnswerPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _input;
    [SerializeField]
    private TextMeshProUGUI _notetext;
    [SerializeField]
    private GameObject _correctlistbutton;
    [SerializeField]
    private GameObject _correctlist;
    [SerializeField]
    private TextMeshProUGUI _correctlisttext;
    private bool _bactiveal;

    public Action<bool, string> BAnswer;

    public void Init(ushort cnt, List<string> cl)
    {
        _bactiveal = false;
        if (3 > cnt)
            _notetext.text = $"※앞으로 {3 - cnt}번 실패할 경우 정답리스트가 공개됩니다.";
        else
            _notetext.text = $"※정답리스트가 공개됩니다.";
        if (cnt > 2)
        {
            _correctlistbutton.SetActive(true);
            for (int i = 0; i < cl.Count; ++i)
                _correctlisttext.text += String.Concat(cl[i], "\n");
            RectTransform t = _correctlisttext.GetComponent<RectTransform>();
            t.sizeDelta = new Vector2(t.sizeDelta.x, cl.Count * 22.5f);
            t.localPosition = new Vector2(t.localPosition.x, cl.Count * -11.25f - 5);
            RectTransform pt = t.parent.GetComponent<RectTransform>();
            pt.sizeDelta = new Vector2(pt.sizeDelta.x, cl.Count * 22.5f);
        }
    }

    public void AnswerIs(bool bans)
    {
        BAnswer?.Invoke(bans, _input.text);
        _input.text = null;
    }

    public void ActiveAL()
    {
        _bactiveal = !_bactiveal;
        _correctlist.SetActive(_bactiveal);
    }

    public void ActiveObject(bool b)
    {
        gameObject.SetActive(b);
        if (!b)
        {
            _bactiveal = false;
            _correctlist.SetActive(_bactiveal);
        }
    }
}
