using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

public class DialogPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _name;
    [SerializeField]
    private TextMeshProUGUI _dialog;

    public async UniTask<bool> SetTextAsync(string name, string dialog, float speed = 0.5f)
    {
        try
        {
            _dialog.text = null;
            speed *= Time.deltaTime;
            _name.text = name;

            for (int i = 0; i < dialog.Length; ++i)
            {
                if (dialog[i].Equals('<'))
                    while (true)
                    {
                        _dialog.text += dialog[i];
                        if (dialog[i].Equals('>'))
                            break;
                        ++i;
                    }
                else
                    _dialog.text += dialog[i];
                await UniTask.Delay(TimeSpan.FromSeconds(speed));
            }

            return false;
        }

        catch
        {
            return false;
        }
    }

    public async UniTaskVoid AddTextAsync(string dialog, float speed = 1.5f)
    {
        try
        {
            speed *= Time.deltaTime;

            for (int i = 0; i < dialog.Length; ++i)
            {
                _dialog.text += dialog[i];
                await UniTask.Delay(TimeSpan.FromSeconds(speed));
            }
        }
        catch
        {
            return;
        }
    }
}
