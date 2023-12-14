using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine;
using System.Runtime.InteropServices; // DLL !!!
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEditor.PackageManager.UI;
using System;
using JetBrains.Annotations;
using UnityEngine.Profiling;
using System.Drawing;

public class HapticManager : SingletonBehaviour<HapticManager>
{
	public enum e_base_frequency
	{
		VIB_DEVICE_FREQUENCY = 0,
		GRIP_DEVICE_FREQUENCY
	}

	public bool debug = true;
	public bool useUDP = false;
	[SerializeField]
	private string _configurationFile = "globalSetting.json";
	private ConfigFileJSON config; //Contain the configuration

	private Thread t_threadSend;                    //Thread use to receive incomming message from client
	private bool running = false;
	private ulong frameID = 0;
    private ulong unityFrameID = 0;
    [Header("Vibrotactile Device Parameters")]
	public bool autoStartVibDevice = true;
	public string vibrotactileDeviceName = "VibrotactileDevice";
	public float frequencyVib = 4000f;
	private int correctionTiming = 0; // [-100, 100]	gap in sample between the write cursor and the read cursor of the STM, allow to speed up or slow down the sending of data
    public const int LEN_USBBUFF = 8; // number of sample send to controler (define in STM32)
	[Range(1,28)]
	public int numberOfChannel = 28;
	[Range(1,511)]
	public int maxPWM = 185; //-> Pour l'instant 185 équivaut à un signal d'amplitude maximale

    public List<HapticDevice> hapticDevices =  new List<HapticDevice>();

	[Header("Grip Device Parameters")]
	public bool autoStartGripDevice = true;
	public string gripDeviceName = "GripDevice";
	public float frequencyGrip = 500f;
	[Range(1, 255)]
	public int maxSpeed = 191;
	[Range(1, 255)]
	public int maxForce= 255;


	public HapticDeviceGrip hapticGripDevice;
	private PWMBalanceWatcher pwmWatcher;
    [Tooltip("if  kill thread after")]
    private int securityKillFrame = 1000000;

	//CONST VIB
	public const string SFRAME_NOSPLIT = "$A";
	public const string SFRAME_SPLIT = "$B";
	public const string TRAME_HEAD_16bits = "C";
	public const string TRAME_HEAD_8bits = "c";
	public const int HEADER_NOSPLIT_SIZE = 2; //2byte for Header
	public const int HEADER_SPLIT_SIZE = 4; //2byte for header + 2byte for size
	public const int MAX_SIZE = 64; //max value possible
	public const float COEFF_TIMING = 1.0f; //additional delay for sending thread in ms
	public const float GAIN_CORRECTION = 0.0005f; //gain to compute the correction of sending frequency

	//CONST GRIP
	public const string SFRAME_SPEED = "$V";
	public const string SFRAME_FORCE = "$F";
	public const int HEADER_GRIP_SIZE = 2; //2byte for Header

    // ****************************************************************************************** Windows 32/64
    [DllImport("kernel32.dll")] static extern int GetCurrentThread();
    [DllImport("kernel32.dll")] static extern int SetThreadAffinityMask(int hThread, int dwThreadAffinityMask);
    [DllImport("kernel32.dll")] static extern int GetCurrentProcessorNumber();
    // ****************************************************************************************** Windows 32/64
    public float getFrequency(e_base_frequency baseF)
    {
		return baseF == e_base_frequency.VIB_DEVICE_FREQUENCY ? HapticManager.Instance.frequencyVib : HapticManager.Instance.frequencyGrip;
	}

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

    private void Update()
    {
        unityFrameID++;
        if (Input.GetKey(KeyCode.Return))
        {
			Debug.Log("meanDelta" + meanDelt + " getperiode : "+ getPeriodeVib() + " correction " + correctionTiming);
		}
    }

    // Start is called before the first frame update
    void Start()
    {
		unityFrameID = 0;
        Debug.Log("Main Cpu used > " + GetCurrentProcessorNumber()); // Just Checking ^^
		configure();

        pwmWatcher = new PWMBalanceWatcher(maxPWM, numberOfChannel);
        pwmWatcher.AlertDetected += PwmWatcher_AlertDetected;
        t_threadSend = new Thread(t_sendMessage);
		// According with the Official documentation : https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity8.html
		// ' The priority for the main thread and graphics thread are both ThreadPriority.Normal.
		//   Any threads with higher priority preempt the main/graphics threads and cause framerate hiccups,
		//   whereas threads with lower priority do not. If threads have an equivalent priority to the main thread, the CPU attempts to give equal time to the threads,
		//   which generally results in framerate stuttering if multiple background threads are performing heavy operations, such as AssetBundle decompression.'
		// So, better use a lowest Thread Priority
		t_threadSend.Priority = System.Threading.ThreadPriority.BelowNormal; //// Lowest, BelowNormal, Normal, AboveNormal, Highest
		t_threadSend.IsBackground = true;
		t_threadSend.Start();
		running = true;

		if(autoStartVibDevice)
			SerialCOMManager.Instance.StartCom(vibrotactileDeviceName);
		if(autoStartGripDevice)
			SerialCOMManager.Instance.StartCom(gripDeviceName);


    }

    private void PwmWatcher_AlertDetected(object sender, PWMBalanceWatcher.AlertPWMBalanceArgs e)
    {
        Debug.LogWarning("PWM channel ("+e.channel+") not balanced :"+e.average +" (center:" +e.center+"), device will be disabled");
        if (e.channel < hapticDevices.Count && hapticDevices[e.channel] != null)
		{
            hapticDevices[e.channel].Active = false;
        }
    }

    void OnApplicationQuit()
    {
		setGripConst(0);

		if (running)
        {
            running = false;
            t_threadSend.Join();
            Debug.Log("Stop sending thread"); // Just Checking ^^
        }
    }

	private void setGripConst(float v)
    {
        if (hapticGripDevice)
		{
			byte[] b = new byte[1];
			b[0] = Utils.ConvertToPWM(v, hapticGripDevice.GripCtrl == HapticDeviceGrip.e_gripCtrl.SPEED_CTRL ? maxSpeed : maxForce, false);
			//Debug.Log("GRI B :"+ v +" " + b[0]);
			b = generateGripFrames(b, hapticGripDevice.GripCtrl);
		   /* foreach (var item in b)
			{
				Debug.Log("B final :"+ item);
			}*/
			SerialCOMManager.Instance.SendCOMMessage(gripDeviceName, b);
		}
		else
		{
			Debug.LogWarning("No device Haptic Grip device defined");
		}
		
	}

	private void t_sendMessage()
	{
		int cpumask = 1 << 4;
		Debug.Log("Nb Processors available: " + System.Environment.ProcessorCount);
        // ************************************************************* Windows 32/64
        SetThreadAffinityMask(GetCurrentThread(), cpumask); // thread=>cpu
        Debug.Log("Send Thread n° Cpu used > " + GetCurrentProcessorNumber()); // Just Checking 
        // ************************************************************* Windows 32/64
		frameID = 0;
		ulong lastUnityFrameID = 0;
        double countLastChange = 0;
        double countdownGrip = 0;
		double countdownVib = 0;
		bool vibIsMinPeriode = getPeriodeVib() < getPeriodeGrip() || hapticGripDevice == null;
		Debug.Log("vibIsMinPeriode" + vibIsMinPeriode);
		/*string timeStr = "";
		string timeStr1 = "";
		string timeStr2 = "";*/
		Stopwatch swTest = Stopwatch.StartNew();
		double lastSend = swTest.Elapsed.TotalMilliseconds;
        double lastTime = swTest.Elapsed.TotalMilliseconds;
		double deltaTime = 0;
		while (running) //while the server is running
		{
			

            deltaTime = swTest.Elapsed.TotalMilliseconds - lastTime;
			lastTime = swTest.Elapsed.TotalMilliseconds;
            countdownVib += deltaTime;
			countdownGrip += deltaTime;

			//ThreadSecurity
            countLastChange += deltaTime;
			if (lastUnityFrameID != unityFrameID) countLastChange = 0;
            lastUnityFrameID = unityFrameID;
            if (countLastChange > 2000) //Stop thread after 2s without unity frame (prevent lost thread)
            {
                Debug.LogWarning("Debug Security, stoping thread");
                running = false;
            }

            if (countdownVib >= getPeriodeVib())
			{
                if (vibIsMinPeriode) { frameID++; }
				byte[] data = getNextVibSamples(LEN_USBBUFF);
				//Debug.Log("Size Data:" + data.Length.ToString());
				List<byte[]> frames = generateVibFrames(data);

				foreach (var fra in frames)
				{
                    if (useUDP) { 
						UDPClient.Instance.SendData(fra);
					}
					else
                    {
						SerialCOMManager.Instance.SendCOMMessage(vibrotactileDeviceName, fra);
						int rep = SerialCOMManager.Instance.ReadCOM(vibrotactileDeviceName); 
						if (rep >= 0)
						{
							correctionTiming = Mathf.Clamp((correctionTiming+rep - 100), -100, 100);
                            if (debug)
							{
                                Debug.Log("correctionTiming : " + correctionTiming.ToString());
                            }
                        }
                    }
				}
                
                countdownVib -= getPeriodeVib();
				meanDelt = swTest.Elapsed.TotalMilliseconds - lastSend;
				lastSend= swTest.Elapsed.TotalMilliseconds;
			}

			if (hapticGripDevice != null && countdownGrip >= getPeriodeGrip())
			{
				if (!vibIsMinPeriode) { frameID++; }
                byte[] data = getNextGripSamples(1);
                data = generateGripFrames(data, hapticGripDevice.GripCtrl);
                SerialCOMManager.Instance.SendCOMMessage(gripDeviceName, data);
                countdownGrip = 0;
			}

		}
		swTest.Stop();
        /*      Debug.Log("timeDelay : " + timeStr);
                Debug.Log("data : " + timeStr2);
                Debug.Log("totaltime : " + timeStr1);*/

    }
	double meanDelt = 0;

    //return the periode of the vib loop in ms
    private double getPeriodeVib()
    {
		return (1f / frequencyVib) * LEN_USBBUFF * (COEFF_TIMING + GAIN_CORRECTION * correctionTiming)  * 1000.0; //TODO verifier avec python si ok
    }

	//return the periode of the grip loop in ms
	private double getPeriodeGrip()
	{
		return 1f / frequencyGrip * 1000f;
	}

	int iter = 0; //Debug

	private byte[] getNextVibSamples(int size)
    {
		string debugStr = "Samples Vib :";
		int sizeEncodingPWMValue = maxPWM <= 255 ? 1 : 2; //depending on the maxPWM the value is encode on 8bits(1byte) or 16bits(2bytes)
        int trameExtraByte = 1 + 1; //   1byte for TRAME_HEAD_8/16Bits + 1byte for Trigger 
		int sizeEncodingValue = numberOfChannel * sizeEncodingPWMValue; //Each pwm value(numChannel) on 1 or 2 byte (8/16bits encoding)
        byte[] nxtSamples = new byte[size * (sizeEncodingValue + trameExtraByte)];
		byte[] zeroVal = Utils.ConvertToLargePWM(0, maxPWM, true, sizeEncodingPWMValue); //default value if no HapticDevices
        for (int i = 0; i< size; i++)
		{
			debugStr += "\n => " + Encoding.ASCII.GetBytes(sizeEncodingPWMValue == 1 ? TRAME_HEAD_8bits : TRAME_HEAD_16bits)[0].ToString() + " " + Utils.ConvertIntToByte(iter);
            // TRAME_HEAD_8bits
            nxtSamples[i * (sizeEncodingValue + trameExtraByte)] = Encoding.ASCII.GetBytes(sizeEncodingPWMValue == 1 ? TRAME_HEAD_8bits:TRAME_HEAD_16bits)[0]; //Debug
            // Trigger Bit
            // nxtSamples[i * (numberOfChannel+2)] = 0; //Synchro byte (not use)
            nxtSamples[i * (sizeEncodingValue + trameExtraByte) + 1] = Utils.ConvertIntToByte(iter); //Debug
			iter++; //Debug
			if (iter > maxPWM) //Debug
            { iter = 0; } //Debug
            for (int j = 0; j < numberOfChannel ; j++)
			{
				float sample = 0;
                byte[] val = zeroVal; //default value if no HapticDevices
                if (j < hapticDevices.Count && hapticDevices[j] != null)
                {
					sample = hapticDevices[j].getSamples(frameID, size)[i]; 
                    val = Utils.ConvertToLargePWM(sample, maxPWM, true, sizeEncodingPWMValue);
                }
				pwmWatcher.addValue(j, sample);
				debugStr += "(" + j + ")";
				for(int s=0; s <sizeEncodingPWMValue; s++)
				{
                    nxtSamples[i * (sizeEncodingValue + trameExtraByte) + j*sizeEncodingPWMValue + trameExtraByte + s] = val[s];
                    debugStr +=  val[s].ToString() + "|";

                }
                debugStr += ",";
            }
            pwmWatcher.addframe();

        }
        if (debug)
        {
			Debug.Log(debugStr);
			string st = "";
			foreach(var va in nxtSamples)
			{
				st += va.ToString() + '.';
			}
            //Debug.Log(st);
        }
		return nxtSamples;
    }

	private byte[] getNextGripSamples(int size)
	{
		string debugStr = "Samples Grip:";
		byte[] nxtSamples = new byte[size];
		for (int i = 0; i < size; i++)
		{
			debugStr += " =>";
			byte val = Utils.ConvertToPWM(0, maxPWM, false);
			if (hapticGripDevice != null)
            {
				val = Utils.ConvertToPWM(hapticGripDevice.getSamples(frameID, 1)[0], hapticGripDevice.GripCtrl == HapticDeviceGrip.e_gripCtrl.SPEED_CTRL? maxSpeed :  maxForce, false);
				nxtSamples[i] = val;
				debugStr += val.ToString() + ",";
			}
		}
		if (debug)
		{
			Debug.Log(debugStr);
		}
		return nxtSamples;
	}

	//Create a Frames from byte (add header and split if too big)
	public List<byte[]> generateVibFrames(byte[] value)
	{
		int L = value.Length;
		List<byte[]> frames = new List<byte[]>();
		List<byte> tabchar = new List<byte>();
		if (L > 2058) //Max lenght managed by the STM32
		{
			UnityEngine.Debug.LogWarning("Value lenght bigger than 2058");
		}
		//depending of the size of data
		if (L <= MAX_SIZE - HEADER_NOSPLIT_SIZE)
		{
			tabchar.AddRange(Encoding.ASCII.GetBytes(SFRAME_NOSPLIT));
			tabchar.AddRange(value);
			frames.Add(tabchar.ToArray());
		}
		else
		{
            //Add Header
            tabchar.AddRange(Encoding.ASCII.GetBytes(SFRAME_SPLIT));
			//Add size // other possibility  Convert.ToByte(((L >> 8) & 0xff)); Convert.ToByte(L & 0xff);
			byte[] lenb = Utils.ConvertIntToBytes(L, 2);
			tabchar.AddRange(lenb);
			//Add Data
			tabchar.AddRange(Utils.SubArray(value, 0, MAX_SIZE - HEADER_SPLIT_SIZE));
			frames.Add(tabchar.ToArray());

            tabchar.Clear();
			//Create a new frame for data remaining
			for (int i = 0; i < (L - (MAX_SIZE - HEADER_SPLIT_SIZE)) / MAX_SIZE + 1; i++)
			{
				int len = (int)Mathf.Min(L - ((MAX_SIZE - HEADER_SPLIT_SIZE) + MAX_SIZE * i), MAX_SIZE);
				tabchar.AddRange(Utils.SubArray(value, (MAX_SIZE - HEADER_SPLIT_SIZE) + MAX_SIZE * i, len));
				frames.Add(tabchar.ToArray());
                tabchar.Clear();
			}
        }

		return frames;
	}

	public byte[] generateGripFrames(byte[] value, HapticDeviceGrip.e_gripCtrl typeCtrl)
	{ 
		List<byte> tabchar = new List<byte>();
		tabchar.AddRange(Encoding.ASCII.GetBytes(typeCtrl==HapticDeviceGrip.e_gripCtrl.SPEED_CTRL? SFRAME_SPEED : SFRAME_FORCE));
		tabchar.Add((byte)1);//dir =1 -> fermeture  =0 ouverture
		tabchar.AddRange(value);
		return tabchar.ToArray();
	}


	private void configure()
	{
		PlayerPrefs.SetString("configurationFile", _configurationFile);
		config = new ConfigFileJSON(_configurationFile,true);
		if (config.Parameters.HasKey("global_setting"))
		{
			if (config.Parameters["global_setting"]["debug"] != null)
			{ debug = config.Parameters["global_setting"]["debug"].AsBool; }
            if (config.Parameters["global_setting"]["use_udp"] != null)
            { useUDP = config.Parameters["global_setting"]["use_udp"].AsBool; }
        }
		if (config.Parameters["vibrotactile_device_setting"]["overwrite_data"] != null && config.Parameters["vibrotactile_device_setting"]["overwrite_data"].AsBool)
		{
			vibrotactileDeviceName = config.Parameters["vibrotactile_device_setting"]["device_name"];
			SerialParameter sp = SerialCOMManager.Instance.getDeviceParameter(config.Parameters["vibrotactile_device_setting"]["device_name"]);
			if (sp == null) // If Device not exist, create one
			{
				sp = new SerialParameter();
				SerialCOMManager.Instance.devicesParameters.Add(sp);
			}
			if (config.Parameters["vibrotactile_device_setting"]["device_name"] != null)
			{
				sp.DeviceName = config.Parameters["vibrotactile_device_setting"]["device_name"];
			}
			if (config.Parameters["vibrotactile_device_setting"]["host_port"] != null)
			{
				sp.Port = config.Parameters["vibrotactile_device_setting"]["host_port"];
			}
			if (config.Parameters["vibrotactile_device_setting"]["host_speed"] != null)
			{
				sp.Speed = config.Parameters["vibrotactile_device_setting"]["host_speed"];
			}
			if (config.Parameters["vibrotactile_device_setting"]["auto_start"] != null)
			{
				autoStartVibDevice = config.Parameters["vibrotactile_device_setting"]["auto_start"].AsBool;
			}
            if (config.Parameters["vibrotactile_device_setting"]["frequency"] != null)
            {
                frequencyVib = config.Parameters["vibrotactile_device_setting"]["frequency"].AsInt;
            }
            if (config.Parameters["vibrotactile_device_setting"]["nb_channel"] != null)
            {
                numberOfChannel = config.Parameters["vibrotactile_device_setting"]["nb_channel"].AsInt;
            }
            if (config.Parameters["vibrotactile_device_setting"]["max_pwm"] != null)
            {
                maxPWM = config.Parameters["vibrotactile_device_setting"]["max_pwm"].AsInt;
            }
        }

		if (config.Parameters["grip_device_setting"]["overwrite_data"] != null && config.Parameters["grip_device_setting"]["overwrite_data"].AsBool)
		{
			gripDeviceName = config.Parameters["grip_device_setting"]["device_name"];
			SerialParameter sp = SerialCOMManager.Instance.getDeviceParameter(config.Parameters["grip_device_setting"]["device_name"]);
			if (sp == null) // If Device not exist, create one
			{
				sp = new SerialParameter();
				SerialCOMManager.Instance.devicesParameters.Add(sp);
			}
			if (config.Parameters["grip_device_setting"]["device_name"] != null)
			{
				sp.DeviceName = config.Parameters["grip_device_setting"]["device_name"];
			}
			if (config.Parameters["grip_device_setting"]["host_port"] != null)
			{
				sp.Port = config.Parameters["grip_device_setting"]["host_port"];
			}
			if (config.Parameters["grip_device_setting"]["host_speed"] != null)
			{
				sp.Speed = config.Parameters["grip_device_setting"]["host_speed"];
			}
			if (config.Parameters["grip_device_setting"]["auto_start"] != null)
			{
				autoStartGripDevice = config.Parameters["grip_device_setting"]["auto_start"].AsBool;
			}
            if (config.Parameters["grip_device_setting"]["frequency"] != null)
            {
                frequencyGrip = config.Parameters["grip_device_setting"]["frequency"].AsInt;
            }
            if (config.Parameters["grip_device_setting"]["max_speed"] != null)
            {
                maxSpeed = config.Parameters["grip_device_setting"]["max_speed"].AsInt;
            }
            if (config.Parameters["grip_device_setting"]["max_force"] != null)
            {
                maxForce = config.Parameters["grip_device_setting"]["max_force"].AsInt;
            }
        }

	}


}
class PWMBalanceWatcher
{
	int m_PWMCenter = 0;
	int m_maxPWM = 511;
	int[] sumPWM = { };
	int[] alertingChannels = { };
    int m_count = 0;
	float m_tolerance = 0;
	int m_countBeforeAlert = 500; //alert when the PWM average not around zero for more than : minCountBeforeAlert*1/Fe

    /// <summary>
    /// 
    /// </summary>
    /// <param name="MaxPWM">value max of PWM</param>
    /// <param name="nbChannel">number of Channel</param>
    /// <param name="minCountAlert">number of value before alert</param>
    /// <param name="tolerance">tolerence around the zero</param>
    public PWMBalanceWatcher(int maxPWM, int nbChannel, int countBeforeAlert = 500,  int tolerance = -1)
	{
		m_maxPWM = maxPWM;
        m_tolerance = tolerance;
        if (m_tolerance < 0) m_tolerance = m_maxPWM * 0.01f; //Default tolerence 1% of MaxPWM
		m_PWMCenter = (m_maxPWM + 1) / 2;
        sumPWM = new int[nbChannel];
        alertingChannels = new int[nbChannel];
        m_countBeforeAlert = countBeforeAlert;
        reset();
    }

	public void addValue(int channel, float value)
	{
		if(channel>= 0 && channel < sumPWM.Length)
		{
            sumPWM[channel] += Utils.floatToPWM(value, m_maxPWM) ;
        }
	}

	public void reset() {
        for (int i = 0; i < sumPWM.Length; i++)
        {
            sumPWM[i] = 0;
            alertingChannels[i] = 0;
        }
        m_count = 0;
    }

	public void addframe()
	{
        m_count++;
		checkBalance();

    }

	private void checkBalance()
	{
        for (int i = 0; i < sumPWM.Length; i++)
        {
			float avg = m_count == 0 ? m_PWMCenter : (sumPWM[i] / m_count);
            float dist = Mathf.Abs(avg - m_PWMCenter);
            if (dist > m_tolerance)
			{
				alertingChannels[i]++;
				if(alertingChannels[i] > m_countBeforeAlert)
				{
                    RaiseAlert(i, avg);
                }
			}
			else { alertingChannels[i] = 0; }
        }
    }
    // Declare the delegate design to handle alert detection.
    public delegate void AlertPWMBalanceHandler(object sender, AlertPWMBalanceArgs e);

    /// <summary>
    /// Occurs when new alert is detected.
    /// </summary>
    public event AlertPWMBalanceHandler AlertDetected;

    /// <summary>
    /// raise the alert event
    /// </summary>
    private void RaiseAlert(int channelId, float avg)
    {
        // Raise the event by using the () operator.
        if (AlertDetected != null)
            AlertDetected(this, new AlertPWMBalanceArgs(channelId, avg, m_PWMCenter));
    }

    /// <summary>
    /// <para>Arguments dispatched with NetworkManager events.</para>
    /// </summary>
    public class AlertPWMBalanceArgs
    {
        public int channel { get; private set; } 
        public float average { get; private set; }    
        public float center { get; private set; }
        public AlertPWMBalanceArgs(int channelId, float avg, float ctr) { channel = channelId; average = avg; center = ctr; }
    }



}