using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class guraModScript : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
	public KMBombModule module;
    public KMSelectable[] buttons;
    public TextMesh[] words;
    public Transform trident;

    public GameObject modObj;
    public GameObject buttonAnims;
    public GameObject[] evilButtonAnims;
    public Material[] evilColours;

    public KMSelectable[] evilButtons;
    public TextMesh[] evilWords;
    public Transform evilTrident;

    private string greekAlphabet = "αβγδεζηθικλμνξοπρστυφχψω";
    private string[] chosenWords = new string[8];
    private int chosenButton;
    private int correctButton;
    private float holdingTime = 0f;
    private bool buttonHeld;

    //Logging purposes
    private string evilWordslog, tridentDirlog, xlog, ylog;
    private int centerlog, evilCorrectButton;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved, evilMode, animating;

    void Awake()
    {
    	moduleId = moduleIdCounter++;
        for (int i = 0; i < buttons.Length; i++)
        {
            int j = i;
            buttons[j].OnInteract += () => { buttonHolder(j); return false; };
            buttons[j].OnInteractEnded += () => { buttonHandler(j); };
        }
        for (int i = 0; i < evilButtons.Length; i++)
        {
            int j = i;
            evilButtons[j].OnInteract += () => { buttonHandler(j); return false; };
        }
        module.OnActivate += solutionFinder;        
    }

    void Start()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < chosenWords.Length; i++)
        {
            chosenWords[i] = greekWordList.words[UnityEngine.Random.Range(0, greekWordList.words.Count())];
            words[i].text = chosenWords[i];
            if (i == 7) { sb.AppendFormat("{0}", chosenWords[i]); }
            else { sb.AppendFormat("{0}, ", chosenWords[i]); }            
        }
        Debug.LogFormat("[The Atlantis #{0}] The words are {1}.", moduleId, sb.ToString());
        chosenButton = UnityEngine.Random.Range(0, buttons.Count());
        trident.localEulerAngles = new Vector3(0f, chosenButton * 45f, 0f);
        Debug.LogFormat("[The Atlantis #{0}] The trident is pointing to button #{1}, counting from north.", moduleId, chosenButton);
    }

    void evilStart()
    {
        StringBuilder sb = new StringBuilder();
        chosenWords = new string[] { "αντιο", "ομικρον", "ημερολογιο", "ασχημοσ", "δεκεμβριοσ", "δακτυλο", "ξι", "πι" };
        for (int i = 0; i < chosenWords.Length; i++)
        {
            chosenWords[i] = greekWordList.words[UnityEngine.Random.Range(0, greekWordList.words.Count())];
            evilWords[i].text = chosenWords[i];
            if (i == 7) { sb.AppendFormat("{0}", chosenWords[i]); }
            else { sb.AppendFormat("{0}, ", chosenWords[i]); }
        }
        evilWordslog = sb.ToString();
        chosenButton = UnityEngine.Random.Range(0, buttons.Count());
        evilTrident.localEulerAngles = new Vector3(0f, chosenButton * -45f + 180f, 0f);
        tridentDirlog = chosenButton.ToString();
    }

    void solutionFinder()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < chosenWords.Length; i++)
        {
            sb.Append(chosenWords[i]);
        }
        string wholeString = sb.ToString();
        sb.Remove(0, sb.Length);
        List<int> alphas = new List<int>();
        for (int j = 0; j < greekAlphabet.Length; j++)
        {
            alphas.Add(0);
            for (int i = 0; i < wholeString.Length; i++)
            {
                sb.Append(greekAlphabet[greekAlphabet.IndexOf(wholeString[i]) == 23 ? 0 : greekAlphabet.IndexOf(wholeString[i]) + 1]);
                if (sb[i] == 'α')
                {
                    alphas[j]++;
                }
            }
            wholeString = sb.ToString();
            sb.Remove(0, sb.Length);
        }
        int valueY = alphas.Max();
        int valueX = alphas.IndexOf(valueY) + 1;
        if (valueX == 24) { valueX = 0; }
        Debug.LogFormat("[The Atlantis #{0}] Value of X is {1}.", moduleId, valueX);
        Debug.LogFormat("[The Atlantis #{0}] Value of Y is {1}.", moduleId, valueY);
        correctButton = (chosenButton + Math.Abs(valueX - valueY)) % 8;
        Debug.LogFormat("[The Atlantis #{0}] The correct button is button #{1}, counting from north.", moduleId, correctButton);
        evilStart();//Prepping first to prevent lag
        evilSolutionFinder();
    }

    void evilSolutionFinder()
    {
        StringBuilder sb = new StringBuilder();
        //StringBuilder sb2 = new StringBuilder();
        int start = bomb.GetSerialNumberNumbers().Sum() % 9 == 0 ? 8 : bomb.GetSerialNumberNumbers().Sum() % 9 - 1;//Digital root
        centerlog = start;
        int funcNo = 0;
        int offset = 0;
        string[] shiftedStrings = new string[8];
        for (int i = 0; i < chosenWords.Length; i++)
        {
            shiftedStrings[i] = chosenWords[i];
        }
        List<int> alphas = new List<int>();
        for (int h = 0; h < greekAlphabet.Length; h++)
        {
            alphas.Add(0);
            for (int i = 0; i < shiftedStrings.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        funcNo = 3 * (((start / 3 - 1) % 3 + 3) % 3) + start % 3;
                        break;
                    case 1:
                        funcNo = 3 * (((start / 3 - 1) % 3 + 3) % 3) + (start % 3 + 1) % 3;
                        break;
                    case 2:
                        funcNo = 3 * (start / 3) + (start % 3 + 1) % 3;
                        break;
                    case 3:
                        funcNo = 3 * ((start / 3 + 1) % 3) + (start % 3 + 1) % 3;
                        break;
                    case 4:
                        funcNo = 3 * ((start / 3 + 1) % 3) + start % 3;
                        break;
                    case 5:
                        funcNo = 3 * ((start / 3 + 1) % 3) + (((start % 3 - 1) % 3 + 3) % 3);
                        break;
                    case 6:
                        funcNo = 3 * (start / 3) + (((start % 3 - 1) % 3 + 3) % 3);
                        break;
                    case 7:
                        funcNo = 3 * (((start / 3 - 1) % 3 + 3) % 3) + (((start % 3 - 1) % 3 + 3) % 3);
                        break;
                }
                for (int j = 0; j < shiftedStrings[i].Length; j++)
                {
                    switch (funcNo)
                    {
                        case 0:
                            offset = h + 2;
                            break;
                        case 1:
                            offset = 2 * h - 1;
                            break;
                        case 2:
                            offset = 3 * h - 1;
                            break;
                        case 3:
                            offset = 2 * h + 1;
                            break;
                        case 4:
                            offset = h;
                            break;
                        case 5:
                            offset = h - 1;
                            break;
                        case 6:
                            offset = 3 * h;
                            break;
                        case 7:
                            offset = h + 1;
                            break;
                        case 8:
                            offset = 2 * h;
                            break;
                    }
                    sb.Append(greekAlphabet[(greekAlphabet.IndexOf(chosenWords[i][j]) + offset + 24) % 24]);
                    if (sb[j] == 'α')
                    {
                        alphas[h]++;
                    }
                }
                //sb2.Append(sb.ToString());
                //sb2.Append(" ");
                shiftedStrings[i] = sb.ToString();
                sb.Remove(0, sb.Length);
            }
            //Debug.Log(h.ToString() + ", " + sb2.ToString());
            //sb2.Remove(0, sb2.Length);
        }
        int valueY = alphas.Max();
        int valueX = alphas.IndexOf(valueY);
        xlog = valueX.ToString();
        ylog = valueY.ToString();
        evilCorrectButton = (chosenButton + valueX + valueY) % 8;
    }

    void buttonHolder(int k)
    {
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (!evilMode) { buttons[k].AddInteractionPunch(0.5f); }
        if (!moduleSolved)
        {
            buttonHeld = true;
            StartCoroutine(buttonHolding());
        }
    }

    void buttonHandler(int k)
    {
        if (moduleSolved || animating) { return; }
        if (!evilMode)
        {
            buttonHeld = false;
            if (holdingTime > 0.3f)
            {
                Debug.LogFormat("[The Atlantis #{0}] Cruel mode activated, going deeper into the waters...", moduleId);
                evilMode = true;
                evilModeLogger();
                StartCoroutine(moduleFlip());
            }
            else if (k == correctButton)
            {
                module.HandlePass();
                Debug.LogFormat("[The Atlantis #{0}] Correct button pressed, module solved.", moduleId);
                foreach (TextMesh i in words) { i.text = ""; }
                audio.PlaySoundAtTransform("a", transform);
                StartCoroutine(solveAnim());
                moduleSolved = true;
            }
            else
            {
                Debug.LogFormat("[The Atlantis #{0}] Wrong button pressed (button #{1}), strike.", moduleId, k);
                module.HandleStrike();
                holdingTime = 0f;
            }
        }
        else
        {
            evilButtons[k].AddInteractionPunch(0.5f);
            if (k == evilCorrectButton)
            {
                module.HandlePass();
                moduleSolved = true;
                evilMode = false;
                Debug.LogFormat("[The Atlantis #{0}] Correct button pressed, module solved. Congrats.", moduleId);
                foreach (TextMesh i in words) { i.text = ""; }
                StartCoroutine(moduleFlip());
                StartCoroutine(evilSolveAnim());
            }
            else
            {
                Debug.LogFormat("[The Atlantis #{0}] Wrong button pressed (button #{1}), strike.", moduleId, k);
                module.HandleStrike();
            }
        }
    }

    void evilModeLogger()
    {
        string[] functions = new string[] { "X+2", "2X-1", "3X-1", "2X+1", "X", "X-1", "3X", "X+1", "2X" };
        string[] positions = new string[] { "top left", "top middle", "top right", "middle left", "middle right", "bottom left", "bottom middle", "bottom right" };
        int logger = 0;
        Debug.LogFormat("[The Atlantis #{0}] The words are {1}.", moduleId, evilWordslog);
        Debug.LogFormat("[The Atlantis #{0}] The trident is pointing to button #{1}, counting from north.", moduleId, tridentDirlog);
        Debug.LogFormat("[The Atlantis #{0}] The center cell is cell #{1}.", moduleId, centerlog + 1);
        for (int i = 0; i < 8; i++)
        {
            switch (i)
            {
                case 0:
                    logger = 3 * (((centerlog / 3 - 1) % 3 + 3) % 3) + (((centerlog % 3 - 1) % 3 + 3) % 3);
                    break;
                case 1:
                    logger = 3 * (((centerlog / 3 - 1) % 3 + 3) % 3) + centerlog % 3;
                    break;
                case 2:
                    logger = 3 * (((centerlog / 3 - 1) % 3 + 3) % 3) + (centerlog % 3 + 1) % 3;
                    break;
                case 3:
                    logger = 3 * (centerlog / 3) + (((centerlog % 3 - 1) % 3 + 3) % 3);
                    break;
                case 4:
                    logger = 3 * (centerlog / 3) + (centerlog % 3 + 1) % 3;
                    break;
                case 5:
                    logger = 3 * ((centerlog / 3 + 1) % 3) + (((centerlog % 3 - 1) % 3 + 3) % 3);
                    break;
                case 6:
                    logger = 3 * ((centerlog / 3 + 1) % 3) + centerlog % 3;
                    break;
                case 7:
                    logger = 3 * ((centerlog / 3 + 1) % 3) + (centerlog % 3 + 1) % 3;
                    break;
            }
            Debug.LogFormat("[The Atlantis #{0}] The {1} button is assigned to function {2}.", moduleId, positions[i], functions[logger]);
        }
        Debug.LogFormat("[The Atlantis #{0}] Value of X is {1}.", moduleId, xlog);
        Debug.LogFormat("[The Atlantis #{0}] Value of Y is {1}.", moduleId, ylog);
        Debug.LogFormat("[The Atlantis #{0}] The correct button is button #{1}, counting from north.", moduleId, evilCorrectButton);
    }
    IEnumerator moduleFlip()
    {
        audio.PlaySoundAtTransform("Flip", transform);
        if (evilMode) { audio.PlaySoundAtTransform("Submerge", transform); audio.PlaySoundAtTransform("Reflect", transform); } 
        else { audio.PlaySoundAtTransform("Emerge", transform); }
        animating = true;
        float from = modObj.transform.localEulerAngles.x;
        float to = modObj.transform.localEulerAngles.x + 180;
        float duration = 1.5f;
        float delta = 0f;
        float y = modObj.transform.localEulerAngles.y;
        float z = modObj.transform.localEulerAngles.z;
        while (delta < duration)
        {
            delta += Time.deltaTime;
            modObj.transform.localEulerAngles = new Vector3 (Easing.OutSine(delta, from, to, duration), y, z);
            yield return null;
        }
        yield return new WaitForSeconds(1f);//For the audio
        animating = false;
    }

    IEnumerator solveAnim()
    {
        animating = true;
        float from = buttonAnims.transform.localPosition.y;
        float to = 0.021f;
        float duration = 2.5f;
        float delta = 0f;
        float x = buttonAnims.transform.localPosition.x;
        float z = buttonAnims.transform.localPosition.z;
        while (delta < duration)
        {
            delta += Time.deltaTime;
            buttonAnims.transform.localPosition = new Vector3(x, Easing.InQuad(delta, from, to, duration), z);
            yield return null;
        }
        buttonAnims.SetActive(false);
        animating = false;
    }

    IEnumerator evilSolveAnim()
    {
        animating = true;
        bool toSwitch = true;
        float[] timing = new float[] { 0.883f, 0.331f, 0.345f, 0.425f, 0.217f, 0.436f, 0.315f, 0.337f, 0.329f, 0.328f, 0.392f };
        audio.PlaySoundAtTransform("Reflected", transform);
        for (int k = 0; k < timing.Length; k++)
        {
            if (k < 5)
            {
                foreach (GameObject button in evilButtonAnims)
                {
                    if (toSwitch) { button.GetComponent<MeshRenderer>().material = evilColours[0]; }
                    else { button.GetComponent<MeshRenderer>().material = evilColours[1]; }
                    toSwitch = !toSwitch;
                }
                toSwitch = !toSwitch;
            }
            else
            {
                switch (k)
                {
                    case 5:
                    case 9:
                        foreach (GameObject button in evilButtonAnims)
                        {
                            button.GetComponent<MeshRenderer>().material = evilColours[2];
                        }
                        break;
                    case 6:
                        evilButtonAnims[2].GetComponent<MeshRenderer>().material = evilColours[0];
                        evilButtonAnims[6].GetComponent<MeshRenderer>().material = evilColours[0];
                        break;
                    case 7:
                        evilButtonAnims[1].GetComponent<MeshRenderer>().material = evilColours[1];
                        evilButtonAnims[3].GetComponent<MeshRenderer>().material = evilColours[1];
                        evilButtonAnims[5].GetComponent<MeshRenderer>().material = evilColours[1];
                        evilButtonAnims[7].GetComponent<MeshRenderer>().material = evilColours[1];
                        break;
                    case 8:
                        evilButtonAnims[0].GetComponent<MeshRenderer>().material = evilColours[0];
                        evilButtonAnims[4].GetComponent<MeshRenderer>().material = evilColours[0];
                        break;
                    case 10:
                        foreach (GameObject button in evilButtonAnims)
                        {
                            button.GetComponent<MeshRenderer>().material = evilColours[3];
                        }
                        break;
                }
            }
            yield return new WaitForSecondsRealtime(timing[k]);
        }
        float from = buttonAnims.transform.localPosition.y;
        float to = 0.021f;
        float duration = 0.5f;
        float delta = 0f;
        float x = buttonAnims.transform.localPosition.x;
        float z = buttonAnims.transform.localPosition.z;
        while (delta < duration)
        {
            delta += Time.deltaTime;
            buttonAnims.transform.localPosition = new Vector3(x, Easing.InQuad(delta, from, to, duration), z);
            yield return null;
        }
        buttonAnims.SetActive(false);
        animating = false;
    }

    IEnumerator buttonHolding()
    {
        while (buttonHeld)
        {
            yield return null;
            holdingTime += Time.deltaTime;
        }
    }

    
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press 0 (Counted from north going clockwise), !{0} press N, !{0} press U to press the top button, !{0} enter to dive deeper into the waters, !{0} text to end the suffering of superzooming to read super small text";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        string[] parameters = command.Split(' ');
        int btn = 0;
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify what button to press!";
                yield break;
            }
            else
            {
                switch (parameters[1].ToUpper())
                {
                    case "0":
                    case "N":
                    case "U":
                        btn = 0;
                        break;
                    case "1":
                    case "NE":
                    case "UR":
                        btn = 1;
                        break;
                    case "2":
                    case "E":
                    case "R":
                        btn = 2;
                        break;
                    case "3":
                    case "SE":
                    case "DR":
                        btn = 3;
                        break;
                    case "4":
                    case "S":
                    case "D":
                        btn = 4;
                        break;
                    case "5":
                    case "SW":
                    case "DL":
                        btn = 5;
                        break;
                    case "6":
                    case "W":
                    case "L":
                        btn = 6;
                        break;
                    case "7":
                    case "NW":
                    case "UL":
                        btn = 7;
                        break;
                    default:
                        yield return "sendtochaterror Please specify a valid button to press!";
                        yield break;
                }
                if (evilMode) { evilButtons[btn].OnInteract(); yield return null; evilButtons[btn].OnInteractEnded();}
                else { buttons[btn].OnInteract(); yield return null; buttons[btn].OnInteractEnded(); }
            }
        }
        else if (Regex.IsMatch(command, @"^\s*enter\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (!evilMode)
            {
                buttons[0].OnInteract();
                yield return new WaitForSeconds(0.4f);
                buttons[0].OnInteractEnded();
                yield return "sendtochat Good luck on your journey, and try not to drown...";
            }
            else
            {
                yield return "sendtochaterror You're already in the deepest ocean. Can't get deeper than that.";
                yield break;
            }
        }
        else if (Regex.IsMatch(command, @"^\s*text\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < chosenWords.Length; i++)
            {
                if (!evilMode)
                {
                    if (i == 7) { sb.AppendFormat("{0}", words[i].text); }
                    else { sb.AppendFormat("{0}, ", words[i].text); }
                }
                else
                {
                    if (i == 7) { sb.AppendFormat("{0}", evilWords[i].text); }
                    else { sb.AppendFormat("{0}, ", evilWords[i].text); }
                }
            }
            yield return "sendtochat The labelled words, starting from north, are " + sb.ToString() + ".";
        }
        else
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
    }
  
    IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            if (!evilMode) { buttons[correctButton].OnInteract(); yield return null; buttons[correctButton].OnInteractEnded(); }
            else { evilButtons[evilCorrectButton].OnInteract(); yield return null; }
        }
    }

}
