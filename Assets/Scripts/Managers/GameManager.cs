using Cysharp.Threading.Tasks;
using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebGLSupport;

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
    public static SoundManager Sound { get; private set; }
    public static ResourceManager Resource { get; private set; }

    public static event Action<short, Scenename> ChangeScene;
    public static event Action<Scenename, short> ActiveScene;
    public Stage Stage { get; private set; }

    [SerializeField]
    private Image _fade;
    [SerializeField]
    private Canvas _fadeCanvas;
    [SerializeField]
    private GameObject _scriptScene;
    [SerializeField]
    private GameObject _waiScene;
    [SerializeField]
    private GameObject _tScene;

    private string _savepath;

    void Awake()
    {
        if (Instance == null)
            Init();
        else
            Destroy(gameObject);
    }

    private void Init()
    {
        Instance = this;
        Resolution();
        DontDestroyOnLoad(gameObject);
        _savepath = System.IO.Path.Combine(Application.persistentDataPath, "stage");
        //Debug.Log(_savepath);
        Stage = LoadStage();
        if (Application.platform == RuntimePlatform.WindowsPlayer)
            this.ObserveEveryValueChanged(_ => Screen.width * Screen.height).Subscribe(_ => Resolution());
        Sound = new SoundManager();
        Resource = new ResourceManager();
    }

    public static void Log<T>(T message)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log(message);
#endif
    }

    private static void Resolution(float w = 1920, float h = 1080)
    {
        int width = Screen.width;
        int height = Screen.height;

        if(w / h < (float)width / height)
        {
            float newidth = 
                (w / h) / 
                ((float)width / height);
            Camera.main.rect = new Rect((1 - newidth) / 2f, 0f, newidth, 1f);
        }
    
        else
        {
            float newheight =
                ((float)width / height) /
                (w / h);
            Camera.main.rect = new Rect(0f, (1 - newheight) / 2f, 1f, newheight);
        }
    }

    public async UniTask FadeAsync(Action act, float time)
    {
        int temp = _fadeCanvas.sortingOrder;
        _fadeCanvas.sortingOrder = 1;
        _fade.gameObject.SetActive(true);
        float fadeTime = 0;

        while (fadeTime < 0.99f)
        {
            _fade.color = Color.Lerp(Color.clear, Color.black, fadeTime);
            await UniTask.Delay(TimeSpan.FromMilliseconds(10));
            fadeTime += 0.0167f;
        }
        await UniTask.Delay(TimeSpan.FromMilliseconds(500));

        act?.Invoke();

        fadeTime = 0;
        while (fadeTime < time)
        {
            _fade.color = Color.Lerp(Color.black, Color.clear, fadeTime / time);
            await UniTask.DelayFrame(1);
            fadeTime += Time.deltaTime;
        }
        _fade.gameObject.SetActive(false);
        _fadeCanvas.sortingOrder = temp;
    }

    public async UniTaskVoid SceneChangeAsync(Scenename next, short stage = -1, DialogType type = DialogType.None)
    {
        _fadeCanvas.worldCamera = Camera.main;
        _fade.gameObject.SetActive(true);
        float fadeTime = 0;

        while(fadeTime < 0.99f)
        {
            _fade.color = Color.Lerp(Color.clear, Color.black, fadeTime);
            await UniTask.Delay(TimeSpan.FromMilliseconds(10));
            fadeTime += 0.0167f;
        }
        ChangeScene?.Invoke((short)SceneManager.GetActiveScene().buildIndex, next);
        await UniTask.Delay(TimeSpan.FromMilliseconds(500));
        SceneManager.LoadScene((int)Scenename.TempScene);
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().buildIndex == (int)Scenename.TempScene);
        _fadeCanvas.worldCamera = Camera.main;

        AsyncOperation ao = SceneManager.LoadSceneAsync((int)next);
        ao.allowSceneActivation = false;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        await UniTask.WhenAll(UniTask.WaitUntil(() => ao.progress >= 0.89), UniTask.Delay(TimeSpan.FromSeconds(2)));
        ao.allowSceneActivation = true;
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().buildIndex == (int)next);
        
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
        _fadeCanvas.worldCamera = Camera.main;
        Resolution();
        fadeTime = 0;
        while (fadeTime < 0.99f)
        {
            _fade.color = Color.Lerp(Color.black, Color.clear, fadeTime);
            await UniTask.Delay(TimeSpan.FromMilliseconds(10));
            fadeTime += 0.0334f;
        }
        _fade.gameObject.SetActive(false);
        ActiveScene?.Invoke(next, stage);
    }

    public void SuccessSave(short stage)
    {
        if (Stage.stage < stage)
        {
            ++Stage.stage;
        }
        ++Stage.successcnt[stage - 1];
        string stagedata = JsonUtility.ToJson(Stage);
        File.WriteAllText(_savepath, stagedata);
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
            if (stage.failcnt == null || stage.successcnt == null)
                return new Stage(stage.stage);
            else
                return stage;
        }

        else
            return new Stage(0);
    }
}
