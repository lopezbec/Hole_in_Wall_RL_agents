using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class EventLogger
{
    string filePath;
    StreamWriter writer;
    StringWriter buffer;
    int lineCnt = 0;

    public EventLogger(string path)
    {
        filePath = path;

        FileStream f = new FileStream(path, FileMode.Append);
        writer = new StreamWriter(f);
        buffer = new StringWriter();

        writer.WriteLine(DateTime.Now.ToString("d"));

    }

    ~EventLogger()
    {
        Close();
        Debug.Log("closed logger");
    }

    public void AddLog(string message)
    {
        buffer.WriteLine(message);
        lineCnt++;
    }

    public void WriteLog()
    {
        writer.WriteLine(lineCnt+1);
        writer.WriteLine(DateTime.Now.ToString("T"));
        writer.WriteLine(buffer.ToString());
        

        lineCnt = 0;
        buffer.Close();
        buffer = new StringWriter();
    }

    public void Close()
    {
        buffer.Close();
        writer.Close();
    }

}
