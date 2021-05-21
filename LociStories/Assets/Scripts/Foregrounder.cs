using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class Foregrounder : MonoBehaviour
{
    private const uint LOCK = 1;
    private const uint UNLOCK = 2;

    private IntPtr window;

    void Start()
    {
        LockSetForegroundWindow(LOCK);
        window = GetActiveWindow();
        StartCoroutine(Checker());
    }

    IEnumerator Checker()
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