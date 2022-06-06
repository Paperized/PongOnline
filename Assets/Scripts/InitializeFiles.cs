using IniParser;
using IniParser.Model;
using System;
using System.IO;
using UnityEngine;
using Utils;

public class InitializeFiles : MonoBehaviour
{
    [Header("Startup")] 
    public bool buildForDeploy;

    [Header("Dedicated Server")]
    [SerializeField] private GameConfiguration localServerConfiguration;
    [SerializeField] private GameConfiguration deployServerConfiguration;

    [Header("Standalone Client")]
    [SerializeField] private GameConfiguration localStandaloneConfiguration;
    [SerializeField] private GameConfiguration deployStandaloneConfiguration;

    [Header("Web-GL Client")]
    [SerializeField] private GameConfiguration localWebConfiguration;
    [SerializeField] private GameConfiguration deployWebConfiguration;

    public void SetupDefaultGameConfig()
    {
        if (GlobalInitializer.StartedAsServer)
        {
            if (buildForDeploy)
                GameConfiguration.DefaultServerConfig = deployServerConfiguration;
            else
                GameConfiguration.DefaultServerConfig = localServerConfiguration;
        }
        else
        {
#if UNITY_WEBGL
            // webgl
            if (buildForDeploy)
                GameConfiguration.DefaultClientConfig = localWebConfiguration;
            else
                GameConfiguration.DefaultClientConfig = deployWebConfiguration;
#else
            // standalone and others
            if (buildForDeploy)
                GameConfiguration.DefaultClientConfig = deployStandaloneConfiguration;
            else
                GameConfiguration.DefaultClientConfig = localStandaloneConfiguration;
#endif
        }
    }

    public GameConfiguration LoadGameConfiguration()
    {
        GameConfiguration config = GameConfiguration.InstanceGame;
        // load default one if web gl, otherwise create config file
#if UNITY_WEBGL
        return config;
#else
        if (!Directory.CreateDirectory(config.FullPathToFile).Exists)
        {
            CDebug.Log("Could not create directories for configuration " + config.FullPathToFile);
            Application.Quit();
        }

        IniData data = null;
        if (File.Exists(config.FullConfigPath))
        {
            FileIniDataParser parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";

            try
            {
                data = parser.ReadFile(config.FullConfigPath);
            } 
            catch(Exception ex)
            {
                CDebug.Log("Error parsing ini configutation data: " + ex);
                Application.Quit();
            }

            if(!config.LoadFromIniData(data))
                Application.Quit();
        }
        else
        {
            data = new IniData();
            data.Configuration.CommentString = "#";
            try
            {
                config.SaveConfiguration(config.FullConfigPath, data);
            } catch(IOException ex)
            {
                CDebug.Log("Error parsing ini configutation data: " + ex);
                Application.Quit();
            }
        }

        return config;
#endif
    }
}
