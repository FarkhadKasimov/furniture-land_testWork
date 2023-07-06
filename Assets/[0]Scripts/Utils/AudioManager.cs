using System.Collections;
using System.Collections.Generic;
using Project.Internal;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [System.Serializable]
    public class AudioClipPresset
    {
        public string key;
        public AudioClip audioClip;
        public bool loop;
        public float volume;

        public AudioClipPresset(string _key = "", AudioClip _audioClip = null, bool _loop = false, float _volume = 1)
        {
            key = _key;
            audioClip = _audioClip;
            loop = _loop;
            volume = Mathf.Clamp(_volume, 0, 1);
        }
    }

    private AudioSource audioSource = null;
    [SerializeField] private List<AudioClipPresset> audioClips = new List<AudioClipPresset>();
    [SerializeField] private AudioSource cassaSource;
    
    private bool audioEnabled = true;

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null)
        {
            Debug.LogError("There is no 'Audio Source' component attached on " + gameObject.name + " object!");
        }

        DontDestroyOnLoad(this);
    }

    public void PlayAudioByKey(string key)
    {
        StopAllAudioClips();

        if(audioEnabled == false || audioSource == null)
        {
            return;
        }

        AudioClipPresset presset = FindAudioClipByKey(key);
        audioSource.loop = presset.loop;
        audioSource.clip = presset.audioClip;
        audioSource.volume = presset.volume;
        audioSource.Play();
    }

    public void PlayBuySound(string key)
    {
        if(audioEnabled == false || audioSource == null)
        {
            return;
        }
        
        AudioClipPresset presset = FindAudioClipByKey(key);
        cassaSource.loop = presset.loop;
        cassaSource.clip = presset.audioClip;
        cassaSource.volume = presset.volume;
        cassaSource.Play();
    }

    public void StopAllAudioClips()
    {
        if(audioSource != null)
        {
            audioSource.loop = false;
            audioSource.UnPause();
            audioSource.Stop();
        }
    }

    public void PouseAudio()
    {
        audioSource.Pause();
    }

    public void ChangeSoundState(bool state)
    {
        audioEnabled = state;
    }

    public AudioClipPresset FindAudioClipByKey(string key)
    {
        return audioClips.Find(x => x.key.Contains(key));
    }
}
