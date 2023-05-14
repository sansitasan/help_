using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerContents : MonoBehaviour
{
    [SerializeField]
    private RectTransform _t;
    [SerializeField]
    private AnswerButton[] _buttons;
    [SerializeField]
    private Stack<AnswerButton> _offButtons = new Stack<AnswerButton>();
    [SerializeField]
    private List<AnswerButton> _onButtons = new List<AnswerButton>();
    [SerializeField]
    private Scrollbar _slider;

    private int _basePosy = 20;
    [SerializeField]
    private int _buttonSize = 40;

    private Action<AnswerButton> _getCount;
    private Action<string, bool, short> _check;

    public void Init(ref Action<string, bool, short> Button, ref Action<string> sendq)
    {
        _check = Button;
        Button -= CheckAddorDel;
        Button += CheckAddorDel;
        _getCount -= OffButton;
        _getCount += OffButton;

        for (int i = 0; i < _buttons.Length; ++i)
        {
            _buttons[i].Init(ref _getCount, ref sendq, _buttonSize);
            _offButtons.Push(_buttons[i]);
        }

        _basePosy = _buttonSize / 2;
    }

    private void CheckAddorDel(string qna, bool b, short num)
    {
        if (b)
            OnButton(qna, num);
        else
            OnButton(qna, num, b);
    }

    private void OnButton(string qna, short num, bool b = true)
    {
        AnswerButton button;
        if (b)
        {
            button = _offButtons.Peek();
            _onButtons.Add(_offButtons.Pop());
            button.OnButton(qna, _basePosy, num);
            
            _basePosy += _buttonSize;
            if (_onButtons.Count * _buttonSize > _t.rect.height)
                _t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _t.rect.height + _buttonSize);
        }

        else
        {
            button = _onButtons[_onButtons.Count - 1];
            button.OnButton(qna, _basePosy, num);
        }
        _slider.value = 0;
    }

    public void OffButton(AnswerButton ab)
    {
        _basePosy -= _buttonSize;
        
        _offButtons.Push(ab);
        //링크드 리스트를 활용하면 더 빠를 것
        
        for(int i = _onButtons.IndexOf(ab); i < _onButtons.Count; ++i)
            _onButtons[i].SetPos(_buttonSize);
        _onButtons.Remove(ab);
        if (_onButtons.Count * _buttonSize > 240)
            _t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _t.rect.height - _buttonSize);
    }

    public void Clear()
    {
        _check -= CheckAddorDel;
        _getCount -= OffButton;
        for (int i = 0; i < _onButtons.Count; ++i)
            _onButtons[i].Clear();
        _onButtons.Clear();
        _offButtons.Clear();
        _buttons.Initialize();
    }
}
