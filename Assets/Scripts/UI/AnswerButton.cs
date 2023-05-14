using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour
{
    private Action<AnswerButton> _sendthis;
    private Action<string> _sendQ;
    private short _ans;

    [SerializeField]
    private RectTransform _t;
    [SerializeField]
    private TextMeshProUGUI _qna;
    [SerializeField]
    private TextMeshProUGUI _check;
    [SerializeField]
    private Button[] _checkButton;

    public void Init(ref Action<AnswerButton> action, ref Action<string> SendQ, float size)
    {
        _sendthis = action;
        _sendQ = SendQ;
        gameObject.SetActive(false);
        _t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }

    public void OnButton(string qna, float posy, short num)
    {
        _ans = num;
        if (num != 2)
        {
            _t.anchoredPosition = new Vector2(0, -posy);
            _qna.text = qna;
            gameObject.SetActive(true);
        }
        else
        {
            _qna.text = qna;
            _qna.color = Color.green;
        }
    }

    public void SetPos(float pos)
    {
        _t.anchoredPosition += new Vector2(0, pos);
    }

    public void CilckAnswerButton(bool b)
    {
        _qna.gameObject.SetActive(!b);
        _check.gameObject.SetActive(b);
        _checkButton[0].gameObject.SetActive(b);
        _checkButton[1].gameObject.SetActive(b);
    }

    public void ClickDelButton()
    {
        CilckAnswerButton(false);
        _sendQ.Invoke(_qna.text.Substring(0, _qna.text.IndexOf('\n')));
        _sendthis.Invoke(this);
        gameObject.SetActive(false);
    }

    public void Clear()
    {
        if(_ans == 1)
            _qna.color = Color.red;
    }
}
