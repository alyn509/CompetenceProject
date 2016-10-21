using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour {

    public AudioClip musicClip1;                   //Drag a reference to the audio source which will play the sound effects.
    public AudioClip musicClip2;                 //Drag a reference to the audio source which will play the music.

    public List<AudioClip> fightingSounds = new List<AudioClip>();
    public static MusicManager instance = null;     //Allows other scripts to call functions from SoundManager.             
    public float lowPitchRange = .95f;              //The lowest a sound effect will be randomly pitched.
    public float highPitchRange = 1.05f;            //The highest a sound effect will be randomly pitched.
    bool fade = false;
    bool changingMusic = false;
    public float volume  = 0.1f;

   AudioSource audioSource;
    //mostly so the volume doesn't lower at bg music fadeout (and to keep you from pressing the button constantly!)
    AudioSource soundEffectSource; 

    void Awake()
    {
        //Check if there is already an instance of SoundManager
        if (instance == null)
            //if not, set it to this.
            instance = this;
        //If instance already exists:
        else if (instance != this)
            //Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
            Destroy(gameObject);

        //Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        DontDestroyOnLoad(gameObject);
        //PlaySingle(musicSource.GetComponent<AudioClip>());

        
        audioSource = GetComponent<AudioSource>();
        soundEffectSource = gameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(musicClip1, 0.7f);
    }

    void Update()
    {
        if (Input.GetKeyDown("a") && !soundEffectSource.isPlaying)
        {
            soundEffectSource.PlayOneShot(fightingSounds[3], 0.7f);
        }
        volume = audioSource.volume;
        if (Input.GetKeyDown("space") || fade)
        {
            FadeOut(musicClip2);
        } else if (changingMusic)
        {
            FadeIn();
        }

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(musicClip1, 1f);
        }
    }

    public void FadeIn()
    {
        audioSource.volume += 0.2f * Time.deltaTime;
        if (audioSource.volume >= 0.7f)
        {
            changingMusic = false;
        }
    }

    public void FadeOut(AudioClip audioClip)
    {
        fade = true;

        if (audioSource.volume > 0.1f)// && !changingMusic)
        {
            //fade out
            audioSource.volume -= 0.2f * Time.deltaTime;
        }
        else 
        {
            audioSource.Stop();
            audioSource.PlayOneShot(audioClip, 1f);
            fade = false;
            changingMusic = true;
        }
            
    }


    //Used to play single sound clips.
    public void PlaySingle(AudioClip clip)
    {
        //Set the clip of our efxSource audio source to the clip passed in as a parameter.
        /*efxSource.clip = clip;

        //Play the clip.
        efxSource.Play();
        efxSource.volume = .7f;*/
    }


    //RandomizeSfx chooses randomly between various audio clips and slightly changes their pitch.
    public void RandomizeSfx(params AudioClip[] clips)
    {
        /*//Generate a random number between 0 and the length of our array of clips passed in.
        int randomIndex = Random.Range(0, clips.Length);

        //Choose a random pitch to play back our clip at between our high and low pitch ranges.
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);

        //Set the pitch of the audio source to the randomly chosen pitch.
        efxSource.pitch = randomPitch;

        //Set the clip to the clip at our randomly chosen index.
        efxSource.clip = clips[randomIndex];

        //Play the clip.
        efxSource.Play();*/
    }
}
