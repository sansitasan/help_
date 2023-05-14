using System;
using UnityEngine;
using TMPro;

public class QuestionPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI[] _q;

    public Action<string, bool> SendQ;

    public void SetText(string[] texts)
    {
        for (int i = 0; i < _q.Length; ++i)
        {
            _q[i].text = texts[i];
            if (string.IsNullOrEmpty(texts[i]))
                _q[i].transform.parent.gameObject.SetActive(false);
        }
        gameObject.SetActive(true);
    }

    public void ClickQ(int num)
    {
        SendQ.Invoke(_q[num].text, false);
        gameObject.SetActive(false);
    }
}
