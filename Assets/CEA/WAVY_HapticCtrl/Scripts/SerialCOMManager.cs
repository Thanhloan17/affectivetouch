using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using System.Linq;
using System.Text;
using System;

/*Some  reference for COM communication 
	https://www.youtube.com/watch?v=5ElKFY3N1zs
	https://github.com/dwilches/Ardity/blob/master/UnityProject/Assets/Ardity/Scripts/SerialController.cshttps://github.com/dwilches/Ardity/blob/master/UnityProject/Assets/Ardity/Scripts/SerialController.cs
 */

public class SerialCOMManager : SingletonBehaviour<SerialCOMManager>
{
	public bool debug = true;

	public List<SerialParameter> devicesParameters;

	protected override bool Awake()
	{
		if (base.Awake())
		{
			//initialize here
			return true;
		}
		else
			return false;
	}

	void Start()
	{
	}

	public SerialParameter getDeviceParameter(string deviceName)
    {
		return devicesParameters.Find(dp => dp.DeviceName == deviceName);
	}

	// Start is called before the first frame update
	public void StartCom(string deviceName = "")
    {
		if(deviceName == "") //Start All devices
        {
            foreach (var dp in devicesParameters)
            {
				if (dp.Ready) continue; //Start only once

                if (!GetPortsName().Contains(dp.Port))
                {
                    Debug.Log("Device " + deviceName + ", Port: " + dp.Port + ", don't seem to be available, can't start com");
                    continue;
                }
                openConnection(dp);
			}
        } else
        {
			SerialParameter dp = devicesParameters.Find(dp => dp.DeviceName == deviceName);
			if(dp == null)
            {
				Debug.Log("Device: " + deviceName+ ", not defined, can't start com");

			}else
            {
				if (dp.Ready) return; //Start only once

                if (!GetPortsName().Contains(dp.Port))
                {
                    Debug.Log("Device " + deviceName + ", Port: " + dp.Port + ", don't seem to be available, can't start com");
                    return;
                }
                openConnection(dp);

            }
		}

		foreach (var dp in devicesParameters)
		{
				Debug.Log("TEST Device " + dp.DeviceName + ", Port: " + dp.Port + ", ready?"+dp.Ready);
		}


	}

	void OnApplicationQuit()
	{
		//Should be sure that no closing data need to send first closing serial
		closeAllConnection();
    }

	public void closeAllConnection()
	{
        foreach (var dp in devicesParameters)
        { closeConnetion(dp); }
    }

	private void openConnection(SerialParameter dp)
	{
		lock (dp.spLock)
		{
			if (dp.Serial == null)
			{
				dp.Serial = new SerialPort(dp.Port, dp.Speed, Parity.None, 8, StopBits.One);
				dp.Serial.WriteTimeout = 500;
				dp.Serial.ReadTimeout = 100;  // sets the timeout value before reporting error
			}
			if (dp.Serial != null)
			{
				if (!dp.Serial.IsOpen)
				{
					try
					{
						dp.Serial.Open();  // opens the connection
						dp.Serial.DiscardInBuffer();
						dp.Serial.DiscardOutBuffer();
						dp.Ready = dp.Serial.IsOpen;
					}
					catch (System.Exception e)
					{
						Debug.LogWarning(e.ToString());
					}
				}
			}
        }

    }

    private void closeConnetion(SerialParameter dp)
    {
		lock (dp.spLock)
		{
			if (dp.Ready)
			{
				dp.Serial.Close();
				dp.Serial = null;
				dp.Ready = false;
			}
		}
	}


    public void SendCOMMessage(string deviceName, byte[] message)
    {
		SerialParameter dp = devicesParameters.Find(dp => dp.DeviceName == deviceName);
		if (dp != null)
        {
			lock (dp.spLock)
			{
				if (dp.Ready)
					dp.Serial.Write(message, 0, message.Length);
			}
        }
		else
        {
			if(debug)
				Debug.Log("No device(" + deviceName +") available, try to send: "+ Encoding.Default.GetString(message));
        }
    }

	//read on byte on the COM port if possible else return -1
    public int ReadCOM(string deviceName)
    {
		int val = -1;
        SerialParameter dp = devicesParameters.Find(dp => dp.DeviceName == deviceName);
        if (dp != null)
        {
			lock (dp.spLock)
			{
				if (dp.Ready && dp.Serial.BytesToRead > 0)
				{ val = dp.Serial.ReadByte(); }
			}
        }
        else
        {
            if (debug)
                Debug.Log("No device(" + deviceName + ") available, that you trying to read");
        }

		return val;
    }

	//Read all the data receive on a COM Port, return a empty array if no data
	public int[] ReadAllCOM(string deviceName)
	{
		int[] val = {};
        SerialParameter dp = devicesParameters.Find(dp => dp.DeviceName == deviceName);
        if (dp != null)
        {
			lock (dp.spLock)
			{
				if(dp.Ready && dp.Serial.BytesToRead > 0)
				{
					val = new int[dp.Serial.BytesToRead];
					for (int i = 0; i < val.Length; i++)
					{
						val[i] = dp.Serial.ReadByte();
					}
				}
			}
        }
        else
        {
            if (debug)
                Debug.Log("No device(" + deviceName + ") available, that you trying to read");
        }

        return val;
    }


    public static List<string> GetPortsName()
	{
		List<string> portNames = new List<string>(SerialPort.GetPortNames());
		return portNames;
	}

}

[System.Serializable]
public class SerialParameter
{
    public readonly object spLock = new object();
    [SerializeField]
	private string deviceName = "";
	private bool ready = false;
	[SerializeField]
	private SerialPort sp;
	[SerializeField]
	private string port = "COM4";                //communication port
	[SerializeField]
	private int speed = 9600;

	public SerialPort Serial { get => sp; set => sp = value; }
	public string Port { get => port; set => port = value; }
	public int Speed { get => speed; set => speed = value; }
	public bool Ready { get => ready; set => ready = value; }
    public string DeviceName { get => deviceName; set => deviceName = value; }
}
