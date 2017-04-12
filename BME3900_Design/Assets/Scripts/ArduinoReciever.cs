using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;

public class ArduinoReciever : MonoBehaviour
{

    //private float calcInput = 0f;
    //private float epsilon = 0.5f;
    //private float factor = 4f;
    private float currentInput = 0f;

    public GraphR breathingGraph;
    /* private float timer = 0f;
     private float timeToUpdate = 1f;
 */
    bool W = false;
    int counter;
    SerialPort serial;

    public bool master;

    void Start()
    {
        master = false;

        //Check to make sure a port is available
        string[] ports = SerialPort.GetPortNames();
        int portNumber = ports.Length;
        if(portNumber > 0)
        {
            serial = new SerialPort("COM4", 9600);
        }else
        {
            W = true;
        }

        counter = 0;
    }

    void Update()

    {
        if (!W & master)
        {
            counter++; 
        }
        if (counter > 100 & master) //Wait 100 frames
        {
            W = true;
            if (!serial.IsOpen)
                serial.Open(); //Open serial, where Arduino is currently printing data
            currentInput = float.Parse(serial.ReadLine());
            if(currentInput != 0) //Toss out any 0s, indicates bad data
            {
                breathingGraph.Graph(currentInput);
            }


            //Data smoothing, unnecessary for this program
            //---------------------------------------------
            // use this block for linear increase or decrease
            //if (calcInput < currentInput - epsilon)
            //{
            //    calcInput += Time.deltaTime * factor + .5f * Time.deltaTime;
            //}
            //else if (calcInput > currentInput + epsilon)
            //{
            //    calcInput -= Time.deltaTime * factor + .5f * Time.deltaTime;
            //}
            //else // calcInput is really close to currentInput so we are ready to update currentInput
            //{
            //    // get input from arduino


            //}


            /*timer += Time.deltaTime;
            if (timer >= timeToUpdate)
            {
                timer = 0f; // reset timer
                //get input from arduino
            }*/
        }
    }


}
