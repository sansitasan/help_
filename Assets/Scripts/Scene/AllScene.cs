using UnityEngine;

public class AllScene : MonoBehaviour
{
    public void OnNextScene(Scenename Scenename, short stage = -1, DialogType type = DialogType.None)
    {
        GameManager.Instance.SceneChangeAsync(Scenename, stage, type).Forget();
    }
}
