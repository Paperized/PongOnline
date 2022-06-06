using IniParser;
using IniParser.Model;
using System.IO;
using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "GameConfiguration", menuName = "GameConfiguration/New Configuration")]
public class GameConfiguration : ScriptableObject
{
    public static GameConfiguration InstanceGame { get; private set; }

    private static GameConfiguration defaultServerConfig = null;
    public static GameConfiguration DefaultServerConfig
    {
        get { return defaultServerConfig; }
        set
        {
            defaultServerConfig = value;
            if (GlobalInitializer.StartedAsServer && InstanceGame == null)
                InstanceGame = value;
        }
    }

    private static GameConfiguration defaultClientConfig = null;
    public static GameConfiguration DefaultClientConfig
    {
        get { return defaultClientConfig; }
        set
        {
            defaultClientConfig = value;
            if (GlobalInitializer.StartedAsClient && InstanceGame == null)
                InstanceGame = value;
        }
    }

    //Tug Boat
    [Header("Connection")]
    public string bindAddress = "127.0.0.1";
    public int port = 7770;
    public int timeoutAfterSeconds = 15;
    // server only
    public int maximumClient = 20;

    // Time Manager
    [Header("Time")]
    public int tickRate = 30;
    public int pingInterval = 1;
    public int timingInterval = 2;
    public int maxPredictionBuffer = 15;

    [Header("Config Location (No Web-GL)")]
    public string relativePathToConfigFile;
    public string configFileName;

    private string fullConfigPath;
    private string fullPathToFile;

    public string FullConfigPath => fullConfigPath;
    public string FullPathToFile => fullPathToFile;

    private void OnEnable()
    {
        if (configFileName == null || configFileName.Length == 0)
            CDebug.LogError("Relative file path cannot be empty");
        fullConfigPath = Path.Combine(Application.persistentDataPath, relativePathToConfigFile, configFileName);
        fullPathToFile = Path.Combine(Application.persistentDataPath, relativePathToConfigFile);
    }

    public void SaveConfiguration(string path, IniData iniData)
    {
        if (path == null) return;
        SectionData connectionData = new SectionData("Connection");
        connectionData.Comments.Add("Essential fields for the connection");
        connectionData.Keys.AddKey("bindAddress", bindAddress);
        connectionData.Keys.AddKey("port", port.ToString());
        if (GlobalInitializer.StartedAsServer)
            connectionData.Keys.AddKey("maximumClient", maximumClient.ToString());
        connectionData.Keys.AddKey("timeout", timeoutAfterSeconds.ToString());

        SectionData timeData = new SectionData("Time");
        timeData.Comments.Add("Ingame advanced settings");
        timeData.Keys.AddKey("tickRate", tickRate.ToString());
        timeData.Keys.AddKey("pingInterval", pingInterval.ToString());
        timeData.Keys.AddKey("timingInterval", timingInterval.ToString());
        timeData.Keys.AddKey("maxPredictionBuffer", maxPredictionBuffer.ToString());

        iniData.Sections.Add(connectionData);
        iniData.Sections.Add(timeData);

        new FileIniDataParser().WriteFile(path, iniData);
    }

    public bool LoadFromIniData(IniData iniData)
    {
        if (iniData == null) return false;

        KeyDataCollection connectionCol = iniData["Connection"];
        if (!LoadConnectionData(connectionCol)) return false;
        KeyDataCollection timeCol = iniData["Time"];
        if (!LoadTimeData(timeCol)) return false;
        return true;
    }

    private bool LoadConnectionData(KeyDataCollection con)
    {
        if (con == null) return false;
        bindAddress = con["bindAddress"];

        if (int.TryParse(con["port"], out port))
        {
            if(port < 1024 || port > 49151)
            {
                CDebug.Log("Port must be between 1024 and 49151");
                return false;
            }
        }
        else
        {
            CDebug.Log("Port is not valid");
            return false;
        }

        if (GlobalInitializer.StartedAsServer)
        {
            if(int.TryParse(con["maximumClient"], out maximumClient))
            {
                if(maximumClient < 0)
                {
                    CDebug.Log("Max Clients must be a positive number");
                    return false;
                }
            }
            else
            {
                CDebug.Log("Max Clients must be a number");
                return false;
            }
        }

        if (int.TryParse(con["timeout"], out timeoutAfterSeconds))
        {
            if (timeoutAfterSeconds <= 0)
            {
                CDebug.Log("Timeout must be greater then one");
                return false;
            }
        }
        else
        {
            CDebug.Log("Timeout must be a number");
            return false;
        }

        return true;
    }
    private bool LoadTimeData(KeyDataCollection con)
    {
        if (con == null) return false;
        if (int.TryParse(con["tickRate"], out tickRate))
        {
            if (tickRate <= 0)
            {
                CDebug.Log("Timeout must be greater then zero");
                return false;
            }
        }
        else
        {
            CDebug.Log("Tick Rate must be greater then zero");
            return false;
        }

        if (int.TryParse(con["pingInterval"], out pingInterval))
        {
            if (pingInterval <= 0)
            {
                CDebug.Log("Ping Interval must be greater then zero");
                return false;
            }
        }
        else
        {
            CDebug.Log("Ping Interval must be greater then zero");
            return false;
        }

        if (int.TryParse(con["pingInterval"], out timingInterval))
        {
            if (pingInterval <= 0)
            {
                CDebug.Log("Timing Interval must be greater then zero");
                return false;
            }
        }
        else
        {
            CDebug.Log("Timing Interval must be greater then zero");
            return false;
        }

        if (int.TryParse(con["maxPredictionBuffer"], out timingInterval))
        {
            if (timingInterval <= 0)
            {
                CDebug.Log("Max Prediction Buffer must be greater then zero");
                return false;
            }
        }
        else
        {
            CDebug.Log("Max Prediction Buffer must be greater then zero");
            return false;
        }

        return true;
    }

    public override string ToString()
    {
        return $"{bindAddress}:{port} | {timeoutAfterSeconds} | {maximumClient}";
    }
}
