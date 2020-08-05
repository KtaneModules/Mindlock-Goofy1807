using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class MindlockScript : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable[] ConnectionPoints;

    int moduleId;
    static int moduleIdCounter = 1;
    bool moduleSolved;
    int[] correctPoints = new int[5];
    string[] points = new string[] { "A1", "B1", "C1", "A2", "B2", "C2", "A3", "B3", "C3" };
    int pointsAmount = 0;
    List<int> pointsPressed = new List<int>();

    void Start()
    {
        moduleId = moduleIdCounter++;
        var numbers = Enumerable.Range(0, 9).ToList().Shuffle();
        for (int i = 0; i < 5; i++)
            correctPoints[i] = numbers[i];
        Debug.LogFormat(@"[Mindlock #{0}] The correct points in order are: {1}", moduleId, correctPoints.Select(x => points[x]).Join(", "));
        for (int i = 0; i < ConnectionPoints.Length; i++)
            ConnectionPoints[i].OnInteract += PointPressed(i);
    }

    KMSelectable.OnInteractHandler PointPressed(int point)
    {
        return delegate
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if (moduleSolved || (pointsPressed.Contains(point) && pointsAmount != 5))
                return false;

            if (pointsAmount == 5)
            {
                pointsPressed.Clear();
                for (int i = 0; i < ConnectionPoints.Length; i++)
                    ConnectionPoints[i].GetComponent<MeshRenderer>().material.color = new Color32(255, 255, 255, 255);
                pointsAmount = 0;
            }

            pointsPressed.Add(point);
            ConnectionPoints[point].GetComponent<MeshRenderer>().material.color = new Color32(255, 255, 0, 255);
            pointsAmount++;

            if (pointsAmount == 5)
                checkAnswer();

            return false;
        };
    }

    private void checkAnswer()
    {
        var allGood = true;
        for (int i = 0; i < pointsPressed.Count; i++)
        {
            if (!correctPoints.Contains(pointsPressed[i]))
            {
                ConnectionPoints[pointsPressed[i]].GetComponent<MeshRenderer>().material.color = new Color32(255, 0, 0, 255);
                allGood = false;
            }
            else if (correctPoints[i] != pointsPressed[i])
            {
                ConnectionPoints[pointsPressed[i]].GetComponent<MeshRenderer>().material.color = new Color32(0, 0, 255, 255);
                allGood = false;
            }
            else
                ConnectionPoints[pointsPressed[i]].GetComponent<MeshRenderer>().material.color = new Color32(0, 255, 0, 255);
        }
        if (allGood)
        {
            Module.HandlePass();
            moduleSolved = true;
        }
    }

#pragma warning disable 0414
    readonly string TwitchHelpMessage = "!{0} 1 2 3 4 5 [Press A1, B1, C1, A2, B2 in that order]";
#pragma warning restore 0414

    IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;
        int i;
        if (moduleSolved)
        {
            yield return "sendtochaterror The module is already solved.";
            yield break;
        }
        else if ((m = Regex.Match(command, @"^\s*([1-9 ]*)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            if (m.Groups[1].Value.Split(' ').ToList().Distinct().Count() != 5)
            {
                yield return "sendtochaterror Please input 5 distinct digits.";
                yield break;
            }
            yield return null;
            yield return m.Groups[1].Value.Split(' ').Select(v => ConnectionPoints[int.Parse(v) - 1]);
            yield break;

        }
        else
        {
            yield return "sendtochaterror Invalid Command.";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat(@"[Mindlock #{0}] Module was force-solved by TP.", moduleId);
        foreach (var point in correctPoints)
        {
            ConnectionPoints[point].OnInteract();
            yield return new WaitForSeconds(.1f);
        }
    }
}
