using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions;

public class Log
{
    public static void initLog()
    {
        System.IO.FileStream oFileStream = null;
        System.IO.File.Delete("log.txt");
        oFileStream = new System.IO.FileStream("log.txt", System.IO.FileMode.Create);
        oFileStream.Close();
    }



    public static void write(string message, string type = "debug")
    {
        StreamWriter writer = new StreamWriter("log.txt", true);
        writer.WriteLine(message);
        writer.Close();
    }

    public static void writeFull(string message, string type = "debug")
    {
        StreamWriter writer = new StreamWriter("log.txt", true);
        writer.WriteLine(message);
        writer.Close();
        /*switch (type)
        {
            case "debug":
                Debug.Log(message);
                break;
            case "warning":
                Debug.LogWarning(message);
                break;
            case "error":
                Debug.LogError(message);
                break;
            default:
                Debug.LogError("Unknown log type " + type + " " + message);
                break;
        }*/
    }

    public static void log(string message)
    {
        StreamWriter writer = new StreamWriter("log.txt", true);
        writer.WriteLine(message);
        writer.Close();
    }
}
