using Cysharp.Threading.Tasks;
using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Scenename
{
    StartScene,
    MainScene,
    ScriptScene,
    WhoamIScene,
    TempScene,
}

public enum DialogType
{
    None,
    Dialog,
    Success,
    Fail
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static Action<string, Scenename> ChangeScene;
    public static Action<Scenename, short> ActiveScene;
    public Stage Stage { get; private set; }

    [SerializeField]
    private Image _fade;
    [SerializeField]
    private GameObject _scriptScene;
    [SerializeField]
    private GameObject _waiScene;
    [SerializeField]
    private GameObject _tScene;

    private string _savepath;

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _savepath = System.IO.Path.Combine(Application.persistentDataPath, "stage");
        Stage = LoadStage();
    }

    public static void Log<T>(T message)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log(message);
#endif
    }

    public async UniTaskVoid SceneChangeAsync(Scenename next, short stage = -1, DialogType type = DialogType.None)
    {
        _fade.gameObject.SetActive(true);
        float fadeTime = 0;

        while(fadeTime < 0.99f)
        {
            _fade.color = Color.Lerp(Color.clear, Color.black, fadeTime);
            await UniTask.Delay(TimeSpan.FromMilliseconds(10) * Time.deltaTime);
            fadeTime += 0.0167f;
        }
        ChangeScene?.Invoke(SceneManager.GetActiveScene().name, next);
        await UniTask.Delay(TimeSpan.FromMilliseconds(500));
        SceneManager.LoadScene((int)Scenename.TempScene);
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().buildIndex == (int)Scenename.TempScene);
        
        AsyncOperation ao = SceneManager.LoadSceneAsync((int)next);
        ao.allowSceneActivation = false;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        await UniTask.WhenAll(UniTask.WaitUntil(() => ao.progress >= 0.89), UniTask.Delay(TimeSpan.FromSeconds(2)));
        ao.allowSceneActivation = true;
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().buildIndex == (int)next);
        ActiveScene?.Invoke(next, stage);
        if (stage != -1)
        {
            switch (next)
            {
                case Scenename.ScriptScene:
                    ScriptScene ss = Instantiate(_scriptScene).GetComponent<ScriptScene>();
                    ss.Init(type, stage);
                    break;

                case Scenename.WhoamIScene:
                    if (stage != 0)
                    {
                        WhoamIScene wai = Instantiate(_waiScene).GetComponent<WhoamIScene>();
                        wai.Init(stage);
                    }
                    else
                    {
                        TutorialScene ts = Instantiate(_tScene).GetComponent<TutorialScene>();
                        ts.Init(stage);
                    }
                    break;
            }
        }

        fadeTime = 0;
        while (fadeTime < 0.99f)
        {
            _fade.color = Color.Lerp(Color.black, Color.clear, fadeTime);
            await UniTask.Delay(TimeSpan.FromMilliseconds(10) * Time.deltaTime);
            fadeTime += 0.0334f;
        }
        _fade.gameObject.SetActive(false);
    }

    public void SuccessSave(short stage)
    {
        if (Stage.stage < stage)
        {
            ++Stage.stage;
            string stagedata = JsonUtility.ToJson(Stage);
            File.WriteAllText(_savepath, stagedata);
        }
    }

    public void FailSave(short stage)
    {
        ++Stage.failcnt[stage];
        string stagedata = JsonUtility.ToJson(Stage);
        File.WriteAllText(_savepath, stagedata);
    }

    private Stage LoadStage()
    {
        if (File.Exists(_savepath))
        {
            string voldata = File.ReadAllText(_savepath);
            Stage stage = JsonUtility.FromJson<Stage>(voldata);
            if (stage.failcnt == null)
                return new Stage(stage.stage);
            else
                return stage;
        }

        else
            return new Stage(0);
    }
}
