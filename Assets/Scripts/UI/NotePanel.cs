using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotePanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _stagetext;
    [SerializeField]
    private TextMeshProUGUI _failtext;
    [SerializeField]
    private Image _img;
    [SerializeField]
    private Sprite[] _sprites;
    private short _stage;

    public void Init(int stage)
    {
        _stage = (short)stage;
        _stagetext.text = $"Stage {stage}";
        _failtext.text = $"실패한 횟수: {GameManager.Instance.Stage.failcnt[stage]}";
        _img.sprite = _sprites[stage];
        gameObject.SetActive(true);
    }

    public void OnNextStage()
    {
        GameManager.Instance.SceneChangeAsync(Scenename.ScriptScene, _stage, DialogType.Dialog).Forget();
    }

    public void OffObject()
    {
        gameObject.SetActive(false);
    }
}
