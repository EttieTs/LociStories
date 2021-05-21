using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.Video;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EtVoiceRecognition : MonoBehaviour
{
    [SerializeField]
    private KeywordRecognizer m_Recognizer;

    public VideoPlayer menuVideoPlayer = new VideoPlayer();
    public VideoPlayer[] storyVideoPlayer = new VideoPlayer[6];

    public AudioSource backgroundSound = new AudioSource();
    public AudioSource backSoundAB = new AudioSource();
    public AudioSource backSoundCD = new AudioSource();
    public AudioSource backSoundEF = new AudioSource();

    // look for ones that sound like the phonetic alphabet for accuracy
    // alpha brave charlie delta echo foxtrot golf hotel indigo juliet kilo lima mike 
    // november oscar papa quebec romeo siera tango uniform victor whisky xray yankee zulu
    const string KeyWord_Morning = "Morning";           // A
    const string KeyWord_Discovery = "Discovery";       // B
    const string KeyWord_Fire = "Fire";                 // C
    const string KeyWord_Muses = "Muses";               // D
    const string KeyWord_Change = "Change";             // E
    const string KeyWord_Memory = "Memory";             // F
    const string KeyWord_Elephant = "Elephant";         

    const string KeyWord_Stop = "Stop";
    const string KeyWord_ShutDown = "Shut Down";

    bool elephant = false;

    int playVideo = -1;

    private string[] m_Keywords = new String[] { KeyWord_Morning,
                                                 KeyWord_Discovery,
                                                 KeyWord_Fire,
                                                 KeyWord_Muses,
                                                 KeyWord_Change,
                                                 KeyWord_Memory,
                                                 KeyWord_Stop,
                                                 KeyWord_ShutDown,
                                                 KeyWord_Elephant,
    };

    static public EtVoiceRecognition instance;
    static string thisMachineName;
    static GameObject[] canvas = new GameObject[3];

    void Awake()
    {
        instance = this;
    }

    private void SetVideoParams(VideoPlayer videoPlayer)
    {
        videoPlayer.frame = 0;
        videoPlayer.targetCamera = GetComponent<Camera>();
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
    }

    void Start()
    {
        Debug.Log("Waiting for voice command");
        
        thisMachineName = BWNetworking.GetDeviceName();

        // Make sure our sound never loops
        backgroundSound.loop = false;
        backSoundAB.loop = false;
        backSoundCD.loop = false;
        backSoundEF.loop = false;

        // Set the video pre conditions
        menuVideoPlayer.frame = 0;
        menuVideoPlayer.targetCamera = GetComponent<Camera>();
        menuVideoPlayer.isLooping = true;
        menuVideoPlayer.renderMode = VideoRenderMode.CameraFarPlane;

        if (thisMachineName == BWNetworking.machineNameAB)
        {
            backgroundSound = backSoundAB;
        }
        if (thisMachineName == BWNetworking.machineNameCD)
        {
            backgroundSound = backSoundCD;
        }
        if (thisMachineName == BWNetworking.machineNameEF)
        {
            backgroundSound = backSoundEF;
        }

        SetVideoParams(storyVideoPlayer[0]);
        SetVideoParams(storyVideoPlayer[1]);
        SetVideoParams(storyVideoPlayer[2]);
        SetVideoParams(storyVideoPlayer[3]);
        SetVideoParams(storyVideoPlayer[4]);
        SetVideoParams(storyVideoPlayer[5]);

        // Setup the speech recognition
        m_Recognizer = new KeywordRecognizer(m_Keywords);
        m_Recognizer.OnPhraseRecognized += OnPhraseRecognized;
        m_Recognizer.Start();

        canvas[0] = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
        canvas[1] = GameObject.Find("Canvas").transform.GetChild(1).gameObject;
        canvas[2] = GameObject.Find("Canvas").transform.GetChild(2).gameObject;

        // Hide the mouse 
        Cursor.visible = false;

        // Start the menu video
        StartMenu();

        EtLog.Log("Log started for version 1.0");
        EtLog.Log("Machine 1:" + BWNetworking.machineNameAB);
        EtLog.Log("Machine 2:" + BWNetworking.machineNameCD);
        EtLog.Log("Machine 3:" + BWNetworking.machineNameEF);
        EtLog.Log("This machine:" + BWNetworking.GetDeviceName() );
    }

    //VIDEO AND SOUND SETUP FOR KEYWORDS
    //=====================================================================================================================================================================

    void StartMenu()
    {
        menuVideoPlayer.frame = 0;
        menuVideoPlayer.Play();

        if (thisMachineName == BWNetworking.machineNameAB)
        {
            canvas[0].SetActive(true);
            canvas[1].SetActive(false);
            canvas[2].SetActive(false);
        }
        if (thisMachineName == BWNetworking.machineNameCD)
        {
            canvas[0].SetActive(false);
            canvas[1].SetActive(true);
            canvas[2].SetActive(false);
        }
        if (thisMachineName == BWNetworking.machineNameEF)
        {
            canvas[0].SetActive(false);
            canvas[1].SetActive(false);
            canvas[2].SetActive(true);
        }
    }

    void TurnOffAnyPlayingVideo()
    {
        if (menuVideoPlayer.isPlaying)
        {
            menuVideoPlayer.Stop();
        }

        for (int i = 0; i < 6; i++)
        {
            if (storyVideoPlayer[i].isPlaying)
            {
                storyVideoPlayer[i].Stop();
            }
        }       
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        TurnOffAnyPlayingVideo();
        StartMenu();
    }

    private void TurnVideoOn(int videoIndex)
    {
        playVideo = videoIndex;
        elephant = false;
    }

    private void Stop()
    {
        TurnOffAnyPlayingVideo();
        StartMenu();                 
    }

    //CHAT BETWEEN MACHINES 
    //=====================================================================================================================================================================
   
    public void PlaySoundAB()
    {
        if (thisMachineName == BWNetworking.machineNameAB)
        {
            if (!backgroundSound.isPlaying)
            {
                backgroundSound.Play();
                Debug.Log("Started sound on "+ BWNetworking.GetDeviceName());
            }
        }        
    }
    public void PlaySoundCD()
    {
        if (thisMachineName == BWNetworking.machineNameCD)
        {
            if (!backgroundSound.isPlaying)
            {
                backgroundSound.Play();
                Debug.Log("Started sound on "+ BWNetworking.GetDeviceName());
            }
        }        
    }
    public void PlaySoundEF()
    {
        if (thisMachineName == BWNetworking.machineNameEF)
        {
            if (!backgroundSound.isPlaying)
            {
                backgroundSound.Play();
                Debug.Log("Started sound on "+ BWNetworking.GetDeviceName());
            } 
        }        
    }
    public void StopSoundAB()
    {
        if (thisMachineName == BWNetworking.machineNameAB)
        {
            if (backgroundSound.isPlaying)
            {
                backgroundSound.Stop();
                Debug.Log("Stopped sound on " + BWNetworking.GetDeviceName());
            }
        }         
    }
    public void StopSoundCD()
    {
        if (thisMachineName == BWNetworking.machineNameCD)
        {
            if (backgroundSound.isPlaying)
            {
                backgroundSound.Stop();
                Debug.Log("Stopped sound on " + BWNetworking.GetDeviceName());
            }
        }         
    }

    public void StopSoundEF()
    {
        if (thisMachineName == BWNetworking.machineNameEF)
        {
            if (backgroundSound.isPlaying)
            {
                backgroundSound.Stop();
                Debug.Log("Stopped sound on " + BWNetworking.GetDeviceName());
            }
        }         
    }


    //KEYWORDS AND UPDATE 
    //=====================================================================================================================================================================
    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        builder.AppendFormat("\tTimestamp: {0}{1}", args.phraseStartTime, Environment.NewLine);
        builder.AppendFormat("\tDuration: {0} seconds{1}", args.phraseDuration.TotalSeconds, Environment.NewLine);
        EtLog.Log(builder.ToString());

        string keywordRecognised = args.text;

        if (keywordRecognised == KeyWord_Elephant)
        {
            elephant = true;
        }

        if ( (thisMachineName == BWNetworking.machineNameAB) || elephant )
        {
            if (keywordRecognised == KeyWord_Morning)
            {
                TurnVideoOn(0);
            }
            if (keywordRecognised == KeyWord_Discovery)
            {
                TurnVideoOn(1);
            }
        }
        if( (thisMachineName == BWNetworking.machineNameCD) || elephant )
        {
            if (keywordRecognised == KeyWord_Fire)
            {
                TurnVideoOn(2);
            }
            if (keywordRecognised == KeyWord_Muses)
            {
                TurnVideoOn(3);
            }
        }
        if( (thisMachineName == BWNetworking.machineNameEF) || elephant )
        {
            if (keywordRecognised == KeyWord_Change)
            {
                TurnVideoOn(4);
            }
            if (keywordRecognised == KeyWord_Memory)
            {
                TurnVideoOn(5);
            }
        }
        if (keywordRecognised == KeyWord_Stop)
        {
            Stop();
        }

        // Write the phrase we recognised
        EtLog.Log("Voice recognition heard \"" + args.text + "\"");

        if (keywordRecognised == KeyWord_ShutDown)
        {
            BWNetworking.SetShutdown();
        }
    }

    public void ForceTest(int index)
    {
        SetVideoParams(storyVideoPlayer[index]);
        TurnVideoOn(index);
    }

    public void Update()
    {
        // Update the networking state
        bool isPlayingSound = backgroundSound.isPlaying;
        bool isPlayingVideo = storyVideoPlayer[0].isPlaying | storyVideoPlayer[1].isPlaying | storyVideoPlayer[2].isPlaying |
                              storyVideoPlayer[3].isPlaying | storyVideoPlayer[4].isPlaying | storyVideoPlayer[5].isPlaying;

        BWNetworking.SendDatagram(isPlayingVideo, isPlayingSound);

        // P is for phrase
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ForceTest(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ForceTest(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ForceTest(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ForceTest(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ForceTest(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ForceTest(5);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            BWNetworking.SetShutdown(true);
        }
        if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }


        if (playVideo != -1)
        {
            Debug.Log("TurnVideoOn " + playVideo);

            TurnOffAnyPlayingVideo();

            if (thisMachineName == BWNetworking.machineNameAB)
            {
                canvas[0].SetActive(false);
            }
            if (thisMachineName == BWNetworking.machineNameCD)
            {
                canvas[1].SetActive(false);
            }
            if (thisMachineName == BWNetworking.machineNameEF)
            {
                canvas[2].SetActive(false);
            }

            if (backgroundSound.isPlaying)
            {
                backgroundSound.Stop();
            }

            storyVideoPlayer[playVideo].frame = 0;
            storyVideoPlayer[playVideo].Play();

            // Now set to -1 so we don't replay
            playVideo = -1;
        }
    }
}