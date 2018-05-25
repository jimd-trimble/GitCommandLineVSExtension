using System;
using System.IO;
using Newtonsoft.Json;

namespace GitConsoleExtension
{
	// ToDo: Refactor into model/provider pattern. This is too confusion.
    internal class ConfigModel
    {
	    private static string defaultCli = "";
	    private string DaemonArg => Daemon ? string.Empty : "--nodaemon";
	    private string LoginOpt => Login ? "-l" : string.Empty;
	    private const string DefaultCygPath = "'/c/Program Files/Git/usr/bin/bash'";
	    private string CygwinPath { get; set; }
		private static bool IsMintty { get; set; }
		private static string FullPath { get; set; }
		private static string Args { get; set; }
		public bool Daemon { get; set; }
		public bool Login { get; set; }
		public bool StartInSolution { get; set; }
	    private const string MinttyFile = "mintty.exe";

	    public static string CliName
	    {
		    get => string.IsNullOrEmpty(defaultCli)
			    ? MinttyFile
			    : defaultCli;
		    set
		    {
				defaultCli = value;
		        IsMintty = string.Equals(MinttyFile, defaultCli, StringComparison.InvariantCultureIgnoreCase);
		    } 
	    }

        public string CliPath {
	        get => FullPath;
	        set
	        {
		        FullPath = value;
				CliName = Path.GetFileName(FullPath);
	        }
        }

	    public string GetArgs()
	    {
			// Give user ability to send a custom string to the cli.
		    if (!string.IsNullOrEmpty(CliArgs) && !IsMintty)
			    return $" {CliArgs}";

		    var argsFrmt = string.IsNullOrEmpty(DaemonArg)
			    ? ""
			    : $" {DaemonArg}";
		    var cygFrmt = string.IsNullOrEmpty(CygPath)
			    ? ""
			    : $" {CygPath}";
		    var logFrmt = string.IsNullOrEmpty(LoginOpt)
			    ? ""
			    : $" {LoginOpt}";

		    return $"{argsFrmt}{cygFrmt}{logFrmt}";

	    }

		public string CliArgs
		{
			get => Args;
			set => Args = value;
		}

	    public string CygPath
	    {
		    get
		    {
			    CygwinPath = CygwinPath ?? DefaultCygPath;
			    return " "+CygwinPath;
		    }
		    set => CygwinPath = value;
	    }
		 
		// CmdTest is used as an example of what is being sent to the Cli.
	    // ReSharper disable once UnusedMember.Global
	    public string CmdTest => $"{FullPath}{GetArgs()}";
    }

    internal class Config
    {
	    public const string ConfigFileName = "Config.conf";
	    public const string ExtensionDataDir = "GitConsoleExtension";
        private static Config instance;
	    public static Config Instance => instance ?? (instance = new Config{Login = true, StartInSolution = false});
	    private ConfigModel config;

        private static string ConfigFile
        {
            get
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
	            var extensionPath = Path.Combine(appdata, ExtensionDataDir);
	            if (!Directory.Exists(extensionPath))
	            {
		            Directory.CreateDirectory(extensionPath);

	            }
	            
	            var configFile = Path.Combine(appdata, extensionPath, ConfigFileName);
	            if (File.Exists(configFile))
		            return configFile;

				// Be sure to close the stream after creating it, especialy if you're about to read it!
				using(var d = File.Create(configFile)) { d.Close(); }

	            return configFile;
            }
        }

	    public bool Login
	    {
		    get => config.Login;
		    set => config.Login = value;
	    }

		public bool StartInSolution
		{

			get => config.StartInSolution;
			set => config.StartInSolution = value;
		}

        public string CliPath
        {
            get => config.CliPath ?? "";
	        set
	        {
		        config.CliPath = value;
	            SaveConfig();
            }
        }

	    public string CliArgs
	    {
		    get => config.GetArgs() ?? "";

		    set
		    {
			    config.CliArgs = value;
				SaveConfig();
		    }
	    }

        private Config()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
	            var configs = File.ReadAllText(ConfigFile);
	            config = !string.IsNullOrEmpty(configs) ? JsonConvert.DeserializeObject<ConfigModel>(configs) : new ConfigModel();
            }
            catch (Exception)
            {
                config = new ConfigModel();
            }
        }

        private void SaveConfig()
        {
            try
            {
                var configs = JsonConvert.SerializeObject(config);
                File.WriteAllText(ConfigFile, configs);
            }
            catch (Exception e)
            {
				throw new Exception($"There was a problem saving the configuration file, please check that you have write access to the location: {e.Message}");
            }
        }
    }
}
