using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class dropDownHandler : MonoBehaviour {
    public Dropdown fileSelector;
    string[] files;
    List<string> placeholder;
    int pathLength;
	// Use this for initialization
	void Start () {
        placeholder = new List<string>(); //List of files
        fileSelector.ClearOptions();
        files = Directory.GetFiles(Application.dataPath + "/StreamingAssets/"); //All files should be located in StreamingAssets directory
        pathLength = (Application.dataPath + "/StreamingAssets/").Length;
        placeholder.Add("None");
        foreach(string file in files)
        {
            if (!file.Contains("meta")) //.meta files are unwanted
            {
             
                placeholder.Add(file.Replace("_", "/").Remove(file.Length - 4).Substring(pathLength)); //Clean up date strings
            }
   
        }
        fileSelector.AddOptions(placeholder);
		
	}
	

}
