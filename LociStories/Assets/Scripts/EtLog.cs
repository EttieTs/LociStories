using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EtLog : MonoBehaviour
{
    static public void Log(string logging)
    {
        // Persistent - because it's in the data folder of the app
        // e.g. for saved games, tilt brush outputs etc.
        //string fileName = Path.Combine(Application.persistentDataPath, "log.txt");
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string fileName = Path.Combine(desktopPath, "log.txt");
        Debug.Log("Log file created at: " + fileName);

        DateTime dt = DateTime.Now;
        string dateTime = dt.ToString("dd/MM/yyyy HH:mm:ss");
        StreamWriter writer = new StreamWriter(fileName, true);
        string output = dateTime + "," + logging;
        writer.WriteLine(output);
        writer.Close();
        writer.Dispose();
        writer = null;

        Debug.Log(logging);
    }
}
