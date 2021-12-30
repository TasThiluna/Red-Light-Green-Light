using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class redLightGreenLight : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;
    public KMBossModule boss;

    public Transform moduleTransform;
    public GameObject statusLight;
    public Renderer[] eyes;
    public Texture[] eyeTextures;
    public TextMesh solveText;

    private int activationCount;
    private int totalActivations;
    private bool active;
    private Vector3 mousePos;
    private bool alreadyStruck;

    public static string[] ignoreList = null;
    private int solvesNeeded;
    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    #region ModSettings
    public KMGameCommands service;
    private bool bombExploded;
    redLightGreenLightSettings Settings = new redLightGreenLightSettings();
    private static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
      new Dictionary<string, object>
      {
        { "Filename", "Red Light Green Light Settings.json"},
        { "Name", "Red Light Green Light" },
        { "Listings", new List<Dictionary<string, object>>
        {
          new Dictionary<string, object>
          {
            { "Key", "HardMode" },
            { "Text", "Enable hard mode (bomb will be detonated instantly)?"}
          }
        }}
      }
    };

    private class redLightGreenLightSettings
    {
        public bool HardMode = false;
    }
    #endregion

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        statusLight.gameObject.SetActive(false);
        module.OnActivate += delegate () { audio.PlaySoundAtTransform("start", transform); StartCoroutine(Timer()); };
        bomb.OnBombExploded += delegate () { bombExploded = true; };
        eyes[0].material.mainTexture = eyeTextures[1];
        eyes[1].material.mainTexture = eyeTextures[0];

        var modConfig = new modConfig<redLightGreenLightSettings>("Red Light Green Light Settings");
        Settings = modConfig.read();
        var missionDesc = KTMissionGetter.Mission.Description;
        if (missionDesc != null)
        {
            var regex = new Regex(@"\[Red Light Green Light\] (true|false)");
            var match = regex.Match(missionDesc);
            if (match.Success)
            {
                string[] options = match.Value.Replace("[Red Light Green Light] ", "").Split(',');
                bool[] values = new bool[options.Length];
                for (int i = 0; i < options.Length; i++)
                    values[i] = options[i] == "true" ? true : false;
                Settings.HardMode = values[0];
            }
        }
        ignoreList = ignoreList ?? boss.GetIgnoredModules("Red Light Green Light", new string[]
        {
            "14",
            "42",
            "501",
            "A>N<D",
            "Bamboozling Time Keeper",
            "Black Arrows",
            "Brainf---",
            "Busy Beaver",
            "Concentration",
            "Duck Konundrum",
            "Don't Touch Anything",
            "Floor Lights",
            "Forget Any Color",
            "Forget Enigma",
            "Forget Everything",
            "Forget Infinity",
            "Forget It Not",
            "Forget Maze Not",
            "Forget Me Later",
            "Forget Me Not",
            "Forget Our Voices",
            "Forget Perspective",
            "Forget The Colors",
            "Forget Them All",
            "Forget This",
            "Forget Us Not",
            "ID Exchange",
            "Iconic",
            "Keypad Directionality",
            "Kugelblitz",
            "Multitask",
            "OmegaDestroyer",
            "OmegaForest",
            "Organization",
            "Password Destroyer",
            "Purgatory",
            "RPS Judging",
            "Security Council",
            "Shoddy Chess",
            "Simon Forgets",
            "Simon's Stages",
            "Souvenir",
            "Tallordered Keys",
            "The Time Keeper",
            "Timing is Everything",
            "The Troll",
            "Turn The Key",
            "The Twin",
            "Übermodule",
            "Ultimate Custom Night",
            "The Very Annoying Button",
            "Whiteout"
        });
    }

    private void Start()
    {
        solvesNeeded = bomb.GetSolvableModuleNames().Where(str => !ignoreList.Contains(str)).Count();
        Debug.LogFormat("<Red Light Green Light #{0}> {1}", moduleId, solvesNeeded);
        activationCount = Mathf.CeilToInt((float)bomb.GetModuleNames().Count() / 5f);
        Debug.LogFormat("[Red Light Green Light #{0}] The module count is {1}, so there will be {2} activation{3}.", moduleId, bomb.GetModuleNames().Count(), activationCount, activationCount != 1 ? "s" : "");
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(rnd.Range(60f, 151f));
        StartCoroutine(Activate());
    }

    private IEnumerator Activate()
    {
        audio.PlaySoundAtTransform("voice", transform);
        yield return new WaitForSeconds(4.515f);
        var elapsed = 0f;
        var duration = .7f;
        var start = Quaternion.identity;
        var end = Quaternion.Euler(0f, 180f, 180f);
        audio.PlaySoundAtTransform("turn", transform);
        while (elapsed < duration)
        {
            moduleTransform.localRotation = Quaternion.Slerp(start, end, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        moduleTransform.localRotation = end;
        active = true;
        alreadyStruck = false;
        mousePos = Input.mousePosition;
        Debug.LogFormat("[Red Light Green Light #{0}] Module activated! Don't move!", moduleId);
        yield return new WaitForSeconds(rnd.Range(15f, 20f));
        Debug.LogFormat("[Red Light Green Light #{0}] Activation passed.", moduleId);
        active = false;
        totalActivations++;
        if (totalActivations == activationCount)
        {
            module.HandlePass();
            moduleSolved = true;
            Debug.LogFormat("[Red Light Green Light #{0}] {1} activations have been completed. Module solved!", moduleId, totalActivations);
            eyes[0].material.mainTexture = eyeTextures[2];
            solveText.text = "GG";
        }
        if (bomb.GetSolvedModuleNames().Count() == solvesNeeded)
        {
            module.HandlePass();
            moduleSolved = true;
            Debug.LogFormat("[Red Light Green Light #{0}] Every other module has been solved. Module solved!", moduleId);
            eyes[0].material.mainTexture = eyeTextures[2];
            solveText.text = "GG";
        }
        start = Quaternion.Euler(0f, 180f, 180f);
        end = Quaternion.identity;
        elapsed = 0f;
        audio.PlaySoundAtTransform("turn", transform);
        while (elapsed < duration)
        {
            moduleTransform.localRotation = Quaternion.Slerp(start, end, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        moduleTransform.localRotation = end;
        if (!moduleSolved)
            StartCoroutine(Timer());
    }

    private void Update()
    {
        if (active)
        {
            if (!alreadyStruck && (Input.anyKey || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) || Mathf.Abs(Input.mousePosition.x - mousePos.x) > .1f || Mathf.Abs(Input.mousePosition.y - mousePos.y) > .1f))
            {
                Debug.LogFormat("[Red Light Green Light #{0}] You moved!", moduleId);
                if (!Settings.HardMode)
                    module.HandleStrike();
                else
                    StartCoroutine(Detonate());
                alreadyStruck = true;
            }
        }
    }

    private IEnumerator Detonate()
    {
        while (!bombExploded)
        {
            yield return new WaitForSeconds(.01f);
            service.CauseStrike("그만 움직여");
        }
    }
}
