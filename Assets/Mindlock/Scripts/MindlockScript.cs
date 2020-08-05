using System.Collections.Generic;
using System.Linq;
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
        //Debug.LogFormat(@"[Mindlock #{0}] The correct points in order are: {1}", moduleId, correctPoints.Select(x => points[x]).Join(", "));
        for (int i = 0; i < ConnectionPoints.Length; i++)
            ConnectionPoints[i].OnInteract += PointPressed(i);
    }

    KMSelectable.OnInteractHandler PointPressed(int point)
    {
        return delegate
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if (moduleSolved || pointsPressed.Contains(point))
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
}
