using DG.Tweening;
using UnityEngine;

public enum Sound
{
    Effect,
    Bgm
}

public class CAudio
{
    private AudioSource _audioSource;
    private Sound _type;

    private void InitVolume(Scenename next, short s)
    {
        if (_audioSource != null)
            switch (_type)
            {
                case Sound.Effect:
                    //볼륨 초기화
                    OptionPanel.RequestAction.Invoke(Sound.Effect);
                    break;

                case Sound.Bgm:
                    //볼륨 초기화
                    OptionPanel.RequestAction.Invoke(Sound.Bgm);
                    break;
            }
        else
            Clear();
    }

    private void SetVolume(float value)
    {
        _audioSource.volume = value;
        _audioSource.pitch = 1;
    }

    public void SetPitch(float value)
    {
        _audioSource.pitch = value;
    }

    public CAudio(AudioSource a, Sound type)
    {
        _audioSource = a;
        _type = type;
        switch (_type)
        {
            case Sound.Effect:
                OptionPanel.EffectAction -= SetVolume;
                OptionPanel.EffectAction += SetVolume;
                OptionPanel.RequestAction.Invoke(Sound.Effect);
                break;

            case Sound.Bgm:
                OptionPanel.BgmAction -= SetVolume;
                OptionPanel.BgmAction += SetVolume;
                OptionPanel.RequestAction.Invoke(Sound.Bgm);
                break;
        }

        GameManager.ActiveScene -= InitVolume;
        GameManager.ActiveScene += InitVolume;
    }

    public void PlaySound(AudioClip clip, Sound type, float pitch = 1f)
    {

        if (_audioSource.isPlaying)
        {
            if (_audioSource.clip != clip)
                _audioSource.Stop();
            else
                return;
        }

        _audioSource.pitch = pitch;

        if (type == Sound.Bgm)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        else
            _audioSource.PlayOneShot(clip);
    }

    public void StopSoundFade(float time = 1.5f)
    {
        _audioSource.DOFade(0, time);
    }

    public void StopSound()
    {
        if (_audioSource.isPlaying)
            _audioSource.Stop();
    }

    public void Clear()
    {
        GameManager.ActiveScene -= InitVolume;
        OptionPanel.EffectAction -= SetVolume;
        OptionPanel.BgmAction -= SetVolume;
    }
}

