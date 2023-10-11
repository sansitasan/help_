using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LookBackGroundButton : MonoBehaviour
{
    private RectTransform _thisTransform;
    private TextMeshProUGUI _text;

    private Vector2[] _pos = new Vector2[2];

    private void Start()
    {
        _thisTransform = GetComponent<RectTransform>();
        _pos[0] = _thisTransform.anchoredPosition;
        _pos[1] = new Vector2(_thisTransform.anchoredPosition.x, -225 + _thisTransform.sizeDelta.y / 2);
        _text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetPos(bool bOpen)
    {
        if (bOpen)
        {
            _thisTransform.anchoredPosition = _pos[0];
            _text.text = "대화창 숨기기";
        }

        else
        {
            _thisTransform.anchoredPosition = _pos[1];
            _text.text = "대화창 보기";
        }
    }
}
