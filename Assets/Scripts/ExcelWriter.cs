using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;

public class ExcelWriter{


    //Main xml reader function
    public List<string>[] ReadFromXml(string filepath)
    {
        List<string> times = new List<string>();
        List<string> breaths = new List<string>();
        List<string>[] returnArray = new List<string>[2];

        XmlDocument xmlDoc = new XmlDocument();
        if (File.Exists(filepath))
        {
            Debug.Log("editing file");
            xmlDoc.Load(filepath);
            XmlNode root = xmlDoc.LastChild;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode n = root.ChildNodes.Item(i);
                for (int k = 0; k < n.ChildNodes.Count; k++)
                {
                    XmlNode option = n.ChildNodes.Item(k);
                    if (option.Name.Equals("Time"))
                    {
                        times.Add(option.InnerText);
                    }else if (option.Name.Equals("Rate"))
                    {
                        breaths.Add(option.InnerText);
                    }
                    //Debug.Log(n.ChildNodes.Item(k).InnerText);
                }
            }

            returnArray[0] = times;
            returnArray[1] = breaths;

            //for(int i = 0; i < n.Count; i++)
            //{
            //  Debug.Log(n.);
            //}

        }

        return returnArray;
    }

    //Main function for converting data arrays to xml data, called on press of "End Session" button
    public void WriteToXml(string[] times, string[] breath)
    {
        DateTime currentDate = DateTime.Now;
        string date = currentDate.Month.ToString() + "_" + currentDate.Day.ToString() + "_" + currentDate.Year.ToString();
        string filepath = Application.dataPath + @"/StreamingAssets/" + date + ".xml";

        int version = 1;
        while(File.Exists(filepath))
        {
            filepath = Application.dataPath + @"/StreamingAssets/" + date + "_V" + version.ToString() + ".xml";
            version++;
        }
       
        Debug.Log("creating File");
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        XmlWriter writer = XmlWriter.Create(filepath, settings);
        writer.WriteStartDocument();
        writer.WriteStartElement("BreathingInfo");
        for(int i = 0; i < times.Length; i++)
        {
            writeNewData(writer, i, times[i], breath[i]);

        }



        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();
        writer.Close();


    }


    //Used in WriteToXml() to streamline process
    public void writeNewData(XmlWriter writer, int id, string time, string breath)
    {
        writer.WriteStartElement("Breath");
        writer.WriteElementString("ID", id.ToString());
        writer.WriteElementString("Time", time);
        writer.WriteElementString("Rate", breath);
        writer.WriteEndElement();
    }

    public void editData()
    {

    }
}
