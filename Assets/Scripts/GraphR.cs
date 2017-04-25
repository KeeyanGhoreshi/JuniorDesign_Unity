using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GraphR : MonoBehaviour {
    
    public ArduinoReciever controlMaster;


    public Text averageBpmCalibration;

    public Slider snapShotScroll;
    public LineRenderer snapShotLine;
    List<float> timesS;
    float startTime;
    public Text startT;
    public Text endT;
    public LineRenderer border;

    LineRenderer graphLine;
    Vector3 R;
    public float increment = .05f;
    float position;
    float rand;
    float i;
    float graphedValue;
    Vector3[] alpha;
    List<Vector3> linePositions;
    List<float> clampingPos;
    int counter;
    bool startScrolling;
    int interval;
    int criticalValue;
    float max;
    float min;


    public Scrollbar breathing;
    public GameObject breathIndicator;
    public GameObject handle;
    GameObject g;

    bool logApnea;
    bool changeMax;
    float breathTime;
    float expectedInterval = 8f;

    List<string> Breaths;
    List<string> Times;
    ExcelWriter fileWriter;

    public Text start;
    public Text first_quadrant;
    public Text second_quadrant;
    public Text end;

    List<float> calibration;
    int calibration_amount;
    float oldtime;
    bool calibrate = true;
    float calibrationTime;



    public Dropdown fileController;
    public Text bpmIndicator;
    public Text durationIndicator;
    public Text riskIndicator;

    //Test Section

	// Use this for initialization
	void Start () {
        border.sortingLayerName = "o";
        snapShotLine.sortingLayerName = "l";
        border.sortingOrder = 10;
        snapShotLine.sortingOrder = -10;
   

        fileWriter = new ExcelWriter();
        Breaths = new List<string>();
        Times = new List<string>();

        max = 80f;
        min = 0f;
        graphLine = GetComponent<LineRenderer>();
        position = -7.5f;
        i = 0;
        linePositions = new List<Vector3>();
        clampingPos = new List<float>();
        interval = 35;
        criticalValue = 2;
        changeMax = true;
        counter = 0;
        breathing.value = 0;
        breathTime = 0;
       
        first_quadrant.text = "" + Mathf.Round(2 * expectedInterval / 3);
        second_quadrant.text = "" + Mathf.Round(4 * expectedInterval / 3);
        end.text = "" + Mathf.Round(expectedInterval * 2);


        calibration = new List<float>();
        oldtime = Time.time;
        calibration_amount = 10;
	}
	
	//Graph is called once per INPUT, rather than frame.  Halting input halts the graph.
	public void Graph(float i) {
    
        
        

        if (i > 500)
        {
            i -= 500;
        }
        
        //Calibrate breathing interval based on 10 breath samples
        if (calibration.Count >= calibration_amount & calibrate)
        {
            calibrationTime = Time.time;
            
            calibrate = false;
            float a = 0;
            for (int k = 0; k < calibration.Count; k++)
            {
                 a += calibration[k];
            }
            float average = a / calibration.Count;
            averageBpmCalibration.text = "BPM: " + average.ToString();
            if(average < 5)
            {
                expectedInterval = 5;
            }else
            {
                expectedInterval = average;
            }
            
            first_quadrant.text = "" + Mathf.Round(2 * expectedInterval / 3);
            second_quadrant.text = "" + Mathf.Round(4 * expectedInterval / 3);
            end.text = "" + Mathf.Round(expectedInterval * 2);
            calibration.Clear();

        }


        //Recalibrate every 10 minutes
        if (!calibrate)
        {
            if((Time.time - calibrationTime)/60 > 10)
            {
                calibrate = true;
            }
        }

        //Move every breath indicator one "tick" to the left, so that they move with the graph
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Indicator");
        for (int j = 0; j < objects.Length; j++)
        {
            objects[j].GetComponent<Movement>().Move();
        }

        //Log an Apnea event if the slider has reached 90% of its length and we haven't already logged this event
        if (breathing.value > .9 & logApnea)
        {
            Times.Add(DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString());
            Breaths.Add("Apnea");
            logApnea = false;
        }
        else if (breathing.value < .9)
        {
            logApnea = true;
        }

        //Move the slider in time such that it fills up after twice the expected interval
        breathing.value += Time.deltaTime / (expectedInterval * 2);

        //Change its color if its getting close to apnea event for visual indication
        handle.GetComponent<Image>().color = Color.Lerp(Color.green, Color.red, breathing.value);


        //Define min and max for clamping purposes
        if (i > max)
        {
            max = i;
            changeMax = true;
            min = max - 50;

        }
        else if (i < min)
        {
            max = i + 10;
        }
        Debug.Log(max);
        Debug.Log(min);
        //Main call, updates graph one tick
        UpdateGraph(i);     
        //---------------------------------

        //Check to see if the graph should be scrolling and scroll it if it should
        if (position < 5)
        {


        }
        else
        {
            if (!startScrolling)
            {
               startScrolling = true;
            }

            for (int k = 0; k < linePositions.Count - 1; k++)
            {
                linePositions[k + 1] = new Vector3(linePositions[k + 1].x - Time.deltaTime - increment, Clamp(clampingPos[k + 1], min, max), 0); //Update outermost value

                linePositions[k] = linePositions[k + 1]; //Move all positions over to the left one, first and second values will be identical
                clampingPos[k] = clampingPos[k + 1];
            }
            linePositions.RemoveAt(linePositions.Count - 1); //Chop off the identical value
            clampingPos.RemoveAt(clampingPos.Count - 1);
            graphLine.SetPositions(linePositions.ToArray()); //Move the line visually
        }

        //Check to see if enough data has come in to start checking for breaths
        if (linePositions.Count > interval + 5)
        {
            //The counter decides how long before another breath can be logged
            if (counter == 0)
            {
                float aggregator = 0;
                float old = clampingPos[clampingPos.Count - interval - 2]; 
                float next = 0;

                //Find the difference between each point in the interval
                for (int k = linePositions.Count - 1 - interval; k < linePositions.Count - 1; k++)
                {
                    next = clampingPos[k];
                    aggregator += next - old;
                    old = next;
                }

                //Check if sum of all differences surpass a critical value, log a breath if they do
                if (aggregator > criticalValue)
                {

                    g = Instantiate(breathIndicator); //Create pointer arrow and line
                    counter += interval*2; //Add duration to counter so the same breath isn't counted twice
                    Times.Add(DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString()); //Log time of breath
                    Breaths.Add("Breath"); //Log the breath event
                    breathing.value = 0; //Reset breathing slider
                    g.transform.position = new Vector2(-.785f, 9.8f); //Manually set the position of indicator, causes issues when graph isn't scrolling

                    //Add to calibration array
                    if (calibrate)
                    {
                        calibration.Add(Time.time - oldtime);
                    }
                        
                        oldtime = Time.time;
                        Debug.Log("Breath");
                    }
                }
                else
                {
                    counter--;
                }


            }


        
    }

    public void UpdateGraph(float newPoint)
    {
        //Static graph update case
        if (!startScrolling)
        {
            int length = graphLine.numPositions;
            graphLine.numPositions = length + 1; //Add a vertex to the line
            position = position + Time.deltaTime + increment; //If the graph is static, the line is still growing to the right
 
        }


        R = new Vector3(position, newPoint, 0);
        linePositions.Add(R);
        clampingPos.Add(R.y); //Clamping only cares about the y value
        R = new Vector3(position, Clamp(newPoint, min, max), 0); //Clamp the value and give it its position

        graphLine.SetPosition(graphLine.numPositions-1, R); //Set the last vertex

    }

    //Clamping ensures values are between a max and min 
    //by making them percentages and multiplying by half 
    //the size of the graph (4.5)
    private float Clamp(float n, float min, float max)
    {
      
        if(min > max)
        {
            return (n / min) * 4.5f;
        }else
        {
            return (n-min) / (max-min) * 4.5f;
        }
    }

    //Ends current session
    public void WriteToFile()
    {
        fileWriter.WriteToXml(Times.ToArray(), Breaths.ToArray());
        controlMaster.master = false;
    }

    //Begins a session
    public void BeginDataCollection()
    {
        controlMaster.master = true;
    }

    //Called by dropdown element, updates info categories
    public void ReadFromFile()
    {
        Text label = fileController.GetComponentInChildren<Text>(); //Get current dropdown text
        string fileName = label.text; 
        if (fileName != "None") 
        {


            fileName = fileName.Replace("/", "_"); //Untidy date string

            //times index 0, breaths index 1
            List<string>[] dataArray = fileWriter.ReadFromXml(Application.dataPath + "/StreamingAssets/" + fileName + ".xml"); //Compose full filepath, read from file
            List<string> times = dataArray[0];
            List<string> breathInfo = dataArray[1];
            List<string> breathInf = breathInfo;

            float[] bpm = dataParseTime(times);
           
            bpmIndicator.text = bpm[1].ToString().Substring(0,5);
            float durationSeconds = bpm[0];

            //Convert seconds to hours : minutes : seconds
            float hours = Mathf.FloorToInt((durationSeconds / 3600)); 
            float minutes = Mathf.FloorToInt((durationSeconds % 3600) / 60);
            float seconds = (durationSeconds % 3600) % 60;
            string h = hours.ToString();
            string m = minutes.ToString();
            string s = seconds.ToString();

            //Ensure there are always two characters so the display looks good
            if (h.Length == 1)
            {
                h = "0" + h;
            }
            if (m.Length == 1)
            {
                m = "0" + m;
            }
            if (s.Length == 1)
            {
                s = "0" + s;
            }
            durationIndicator.text = h + ":" + m + ":" + s;
            Predicate<string> apneaFinder = FindMatch;
            //Update risk display
            if (breathInfo.Contains("Apnea"))
            {
                int apneaCountBoi = breathInfo.FindAll(apneaFinder).Count;
                if(apneaCountBoi == 1)
                {
                    riskIndicator.text = "None";

                }else if(apneaCountBoi<3)
                {
                    riskIndicator.text = "Low";
                }else if(apneaCountBoi < 6)
                {
                    riskIndicator.text = "Medium";
                }else if(apneaCountBoi >6)
                {
                    riskIndicator.text = "High";
                }
                

            }else
            {
                riskIndicator.text = "None";
            }
            snapShotScroll.value = 0;
            snapShot(times, breathInf);
        }
        else
        {
            durationIndicator.text = "--:--:--";
            riskIndicator.text = "---";
            bpmIndicator.text = "---";
        }
    }

    private static bool FindMatch(string obj)
    {
        return obj == "Apnea";
    } 

    //Overloaded parse function for defining a start and end time
    public void dataParseTime(List<string> times, string startTimeHour,string startTimeMinute, string endTimeHour,string endTimeMinute)
    {
        List<float> breathingRate = new List<float>();
        float oldseconds = 0;
        float s;
        float startTimeInSeconds = float.Parse(startTimeHour) * 60 * 60 + float.Parse(startTimeMinute) * 60;
        float endTimeInSeconds = float.Parse(endTimeHour) * 60 * 60 + float.Parse(endTimeMinute) * 60;
        
        foreach (string k in times)
        {
            Debug.Log(k);
            
            string[] a = k.Split(":".ToCharArray());
            for (int i = 0; i < a.Length; i += 3)
            {
                s = float.Parse(a[i]) * 60 * 60 + float.Parse(a[i + 1]) * 60 + float.Parse(a[i + 2]);
                if (s > startTimeInSeconds & s < endTimeInSeconds)
                {
                    if (oldseconds == 0)
                    {
                        oldseconds = s;
                    }
                    else
                    {
                        breathingRate.Add(s - oldseconds);
                        oldseconds = s;
                    }
                }

            }
        }
        float agg = 0;
        foreach(float i in breathingRate)
        {
            agg += i;
        }
        Debug.Log(agg / breathingRate.Count);
    }

    //Original method for parsing times 
    //and returning total duration and 
    //breaths per minute.
     
    public float[] dataParseTime(List<string> times)
    {
        List<float> breathingRate = new List<float>();
        float oldseconds = 0;
        float s;

        //Change format of time from hour:minute:seconds to just seconds
        foreach (string k in times)
        {
            string[] a = k.Split(":".ToCharArray());
            for (int i = 0; i < a.Length; i += 3)
            {
                s = float.Parse(a[i]) * 60 * 60 + float.Parse(a[i + 1]) * 60 + float.Parse(a[i + 2]); //Conversion line
                if (oldseconds == 0)
                {
                    oldseconds = s;
                }
                else
                {
                    breathingRate.Add(s - oldseconds); //Log change in time
                    oldseconds = s;
                }
            }

        }
        float agg = 0;
        foreach (float i in breathingRate) //Finds total time and average breaths per minute
        {
            agg += i;
        }
        float[] returnValues = new float[2];
        returnValues[0] = agg; //Total time
        returnValues[1] = (agg / breathingRate.Count); //BPM
        return returnValues;
    }

    //Switches a positive time string to a negative one
    //by appending a "-" in front of each value.
    //Effectively creates "-hours:-minutes:-seconds".
    private string timeNegativeSwitch(string apneaEvent)
    {
        string[] b = apneaEvent.Split(":".ToCharArray());
        for(int i = 0; i < b.Length; i++)
        {
            b[i] = "-" + b[i];
        }
        string returnValue = String.Join(":", b);
        return returnValue;


        }

    //Creates preliminary graph of saved data.
   
    public void snapShot(List<string> timesX, List<string> breathInf)
    {
        List<float> timeinSeconds = new List<float>();
        //Do not change
        float oldseconds = 0;
        float s;
        float agg = 0;
        float increment = 0;
        //------------------------
        List<Vector3> positions = new List<Vector3>();
        int indice;

        //Make the time value that Apnea events occur at
        //negative so that they can be distinguished from
        //breaths but still carry their original time value
        while (breathInf.Contains("Apnea"))
        {
            Debug.Log("Deleting Apneas");
            indice = breathInf.IndexOf("Apnea");
            breathInf.RemoveAt(indice);
            string ApneaEvent = timesX[indice];
            ApneaEvent = timeNegativeSwitch(ApneaEvent);
            timesX.RemoveAt(indice);
            timesX.Insert(indice, ApneaEvent);
            breathInf.Insert(indice, "placeholder"); //If you don't put a placeholder the indices will be off
            
        }
        
        //Change format of time from hour:minute:seconds to just seconds
        foreach (string k in timesX)
        {
           
                string[] a = k.Split(":".ToCharArray());
                for (int i = 0; i < a.Length; i += 3)
                {
                    s = float.Parse(a[i]) * 60 * 60 + float.Parse(a[i + 1]) * 60 + float.Parse(a[i + 2]); //Conversion line
                    if (oldseconds == 0)
                    {
                        oldseconds = s;
                        startTime = s;
                    }
                    else if(s > 0)
                    {
                        timeinSeconds.Add(s - oldseconds); //Log change in time
                        oldseconds = s;
                }else if (s < 0)
                {
                    timeinSeconds.Add(s +oldseconds);
                    oldseconds = -s;
                }
                }
            }
            

        
        foreach (float i in timeinSeconds) //Finds total time and average breaths per minute
        {
            if (i > 0)
            {
               agg += i;
            }
            
        }

        int length = timeinSeconds.Count;
        
        timesS = timeinSeconds;
        snapShotScroll.maxValue = length;
        snapShotScroll.value = 0;
        if (length > 60)
        {
            length = 60;
        }
        float aggregate = 0;
        for (int index = 0; index < length; index++)
        {
            if(timesS[index]>= 0)
            {
                aggregate += timesS[index];
            }else
            {
                aggregate -= timesS[index];
            }
            
        }
        startT.text = convertTime(startTime);
        endT.text = convertTime(startTime + aggregate);
        for (int index = 0; index < length-1;index++)
        {
            positions.Add(new Vector3(increment, -5));
            if(timeinSeconds[index] > 0)
            {
                increment += (timeinSeconds[index] / aggregate)*15;
                positions.Add(new Vector3(increment, -5));
                positions.Add(new Vector3(increment, -4));
            }else
            {
                increment += (-timeinSeconds[index]/aggregate)* 15;
                positions.Add(new Vector3(increment, -5));
                positions.Add(new Vector3(increment, -6));
            }
            
        }
        snapShotLine.numPositions = positions.Count;
        snapShotLine.SetPositions(positions.ToArray());
    }

    //Adjusts the graphical representation of data with the slider.  
    public void adjustSnapshot()
    {
        List<Vector3> positionsX = new List<Vector3>();
        int value = Mathf.RoundToInt(snapShotScroll.value);
        
        int length = timesS.Count;
        float inc = 0;
        if(timesS.Count - value > 60)
        {
            length = 60 + value;
        }else if(timesS.Count > 60)
        {
            value = timesS.Count - 60;
        }
        else
        {
            value = 0;
        }

        float aggregate = 0;
        float antiAgg =0;
        for(int j = 0; j< value; j++)
        {
            antiAgg += timesS[j];
        }
        for(int index = value; index < length; index++)
        {
            if (timesS[index] >= 0)
            {
                aggregate += timesS[index];
            }
            else
            {
                
                aggregate -= timesS[index];
            }
        }
        startT.text = convertTime(startTime + antiAgg);
        endT.text = convertTime(startTime + antiAgg + aggregate);
        for (int index = value ; index < length; index++)
        {
            positionsX.Add(new Vector3(inc, -5));
            if (timesS[index]> 0)
            {
                inc += (timesS[index] / aggregate) * 14.75f;
                positionsX.Add(new Vector3(inc, -5));
                positionsX.Add(new Vector3(inc, -4));
            }else
            {
               
                inc += (-timesS[index] / aggregate) * 14.75f;
                positionsX.Add(new Vector3(inc, -5));
                positionsX.Add(new Vector3(inc, -6));
    
            }
            
            
        }
        snapShotLine.numPositions = positionsX.Count;
        snapShotLine.SetPositions(positionsX.ToArray());
    }

    //Converts seconds into hours:minutes:seconds
    private string convertTime(float durationSeconds)
    {
        float hours = Mathf.FloorToInt((durationSeconds / 3600));
        float minutes = Mathf.FloorToInt((durationSeconds % 3600) / 60);
        float seconds = (durationSeconds % 3600) % 60;
        string h = hours.ToString();
        string m = minutes.ToString();
        string s = seconds.ToString();

        //Ensure there are always two characters so the display looks good
        if (h.Length == 1)
        {
            h = "0" + h;
        }
        if (m.Length == 1)
        {
            m = "0" + m;
        }

        return(h + ":" + m);

    }

}
