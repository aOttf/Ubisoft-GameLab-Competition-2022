using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{
    string logText = "*begin log";
    string filename = "";
    bool showOnScreen = false;
    int kChars = 700;

    private DebugLogger Instance;

    private void Awake()
    {
        if(Instance != null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void OnEnable() 
    { 
        Application.logMessageReceived += Log; 
    
    }

    void OnDisable() 
    {
        Application.logMessageReceived -= Log; 
    }

    void Update() { if (Input.GetKeyDown(KeyCode.F10)) { showOnScreen = !showOnScreen; } }
    public void Log(string logString, string stackTrace, LogType type)
    {
        // On screen
        logText = logText + "\n" + logString;
        if (logText.Length > kChars) { logText = logText.Substring(logText.Length - kChars); }

        // File
        if (filename == "")
        {
            string d = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/STUDENT_XP_LOGS";
            System.IO.Directory.CreateDirectory(d);
            string r = Random.Range(1000, 9999).ToString();
            filename = d + "/log-" + r + ".txt";
        }
        try 
        { 
            System.IO.File.AppendAllText(filename, logString + "\n"); 
        }
        catch { }
    }

    void OnGUI()
    {
        if (!showOnScreen) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.TextArea(new Rect(10, 10, 540, 370), logText);
    }
}
