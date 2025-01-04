using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    #region Fields

    // Timer duration
    float totalSeconds = 0;

    // Timer execution
    float elapsedSeconds = 0;
    bool running = false;

    // Support for finished property
    bool started = false;

    #endregion

    // Update is called once per frame
    void Update()
    {
        // update timer and check for finished
        if (running)
        {
            elapsedSeconds += Time.deltaTime;
            if (elapsedSeconds >= totalSeconds)
            {
                running = false;
            }
        }
    }

    public float Duration
    {
        set
        {
            if (!running)
            {
                totalSeconds = value;
            }
        }
    }

    public bool Finished
    {
        get { return started && !running; }
    }

    public bool Running
    {
        get { return running; }
    }

    public void Run()
    {
        if (totalSeconds > 0)
        {
            started = true;
            running = true;
            elapsedSeconds = 0;
        }
    }
}
