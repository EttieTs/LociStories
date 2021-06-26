using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

// Atti's solution from - https://answers.unity.com/questions/37416/how-can-i-bring-to-front-a-window-called-by-an-scr.html

public class Foregrounder : MonoBehaviour
{
    private const uint LSFW_LOCK = 1;
    private const uint LSFW_UNLOCK = 2;

    private IntPtr window;

    void Start()
    {
        LockSetForegroundWindow(LSFW_LOCK);
        window = GetActiveWindow();
        StartCoroutine(AsyncChecker());
    }

    IEnumerator AsyncChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);
            IntPtr newWindow = GetActiveWindow();

            if (window != newWindow)
            {
                Debug.Log("Set to foreground");
                SwitchToThisWindow(window, true);
            }
        }
    }

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")]
    static extern bool LockSetForegroundWindow(uint uLockCode);
    [DllImport("user32.dll")]
    static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
}