using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using TurboCup.Data;
using TurboCup.Extensions;

namespace TurboCup {
public struct SocketAddress {

    // Socket Parameters
    public String host;
    public int port;

    public SocketAddress(String host, int port) {
        this.host = host;
        this.port = port;
    }
}

public enum LogLevel {
    DEBUG,
    INFO,
    WARN,
    ERROR
}

// Log Message Delegate
public delegate void LogMessageHandler(LogLevel level, String msg);

public partial class Main : Form {

    // Tcp Server and Thread
    private Thread thServer = null;
    private TcpListener server = null;

    // Mutex for Process Data
    private static Mutex processMutex = new Mutex();

    // Instance Reference
    public static Main Instance;

    // Log Files
    private static readonly String LogFile = String.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
    private static readonly String RawFile = String.Format("{0}.dat", DateTime.Now.ToString("yyyy-MM-dd"));

    // Upload Files
    private static readonly String UploadFile = String.Format("{0}.tmp", DateTime.Now.ToString("yyyy-MM-dd"));
    private static readonly String SessionFile = String.Format("{0}.csv", DateTime.Now.ToString("yyyy-MM-dd"));

    // Member Variables
    private Challenge currentChallenge = new Challenge();
    private Dictionary<String, PlayerScore> playerScores = new Dictionary<String, PlayerScore>();

    public Main() {
        InitializeComponent();
        Instance = this;
    }

    private void Main_Load(object sender, EventArgs e) {
        // Load Challenge Data
        if (ChallengeData.FileExists()) {
            ChallengeData.LoadFile();
        } else {
            ChallengeData.ExportFile();
        }

        // Checkbox - Challenges
        cbChallenge.ValueMember = null;
        cbChallenge.DisplayMember = "DisplayName";
        cbChallenge.DataSource = new BindingList<Challenge>(ChallengeData.Challenges);

        // Current Date
        String today = DateTime.Now.ToString("yyyy-MM-dd");

        // Select Challenge for Current Day
        foreach (Challenge challenge in ChallengeData.Challenges) {
            // Compare Challenge Dates
            if (challenge.Date.Equals(today)) {
                // Set Challenge for Current Day
                cbChallenge.SelectedItem = challenge;
            }
        }
    }

    private void cbChallenge_SelectedIndexChanged(object sender, EventArgs e) {
        currentChallenge = (Challenge)cbChallenge.SelectedValue;
    }

    public void ProcessData(LinkedList<String> networkData) {
        // Wait for Mutex
        processMutex.WaitOne();

        try {
            // Process Network Data
            foreach (String data in networkData) {
                // Log Network Data
                WriteLineToRawFile(String.Format("{0} - {1}", DateTime.Now.ToString("HH:mm:ss.fff"), data));

                // Split Records
                LinkedList<String> records = new LinkedList<String>(data.Split("\x1E".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

                // Map Details
                String mapKey = "";
                String mapName = "";

                // Get Map Details
                if (records.Count > 0) {
                    // Remove First Record
                    String mapRecord = records.First.Value;
                    records.RemoveFirst();

                    // Get Map Details
                    String[] details = mapRecord.Split("\x1F".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    // Check Record
                    if (details.Length == 2) {
                        // Get Map Details
                        mapKey = details[0];
                        mapName = details[1];

                        // Log Map
                        LogDebugMessage("Map: {0} Name: {1}", mapKey, mapName);
                    } else {
                        // Log Error Details
                        LogErrorMessage("[ERROR] Invalid Map Record {0}", mapRecord);
                        lblStatus.BackColor = Color.OrangeRed;
                    }
                }

                // For Each Record
                foreach (String record in records) {
                    // Split Items
                    String[] items = record.Split("\x1F".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    // Record Details
                    String playerName = "";
                    String playerLogin = "";

                    String bestTime = "";
                    String lastTime = "";

                    // Check Record
                    if (items.Length == 4) {
                        // Get Player Details
                        playerName = items[0];
                        playerLogin = items[1];

                        // Get Race Score
                        bestTime = items[2];
                        lastTime = items[3];

                        // Check Player Name
                        if (playerName.Contains(';')) {
                            // Replace CSV Separator in Name
                            playerName = playerName.Replace(';', '_');
                        }

                        // Check Player Name
                        if (playerName.Contains('"')) {
                            // Replace CSV Escape Character in Name
                            playerName = playerName.Replace('"', '_');
                        }

                        // Build Key
                        String recordKey = String.Format("{0}|{1}", mapKey, playerName);

                        // Player Score
                        PlayerScore score;

                        // Check Player Score
                        if (playerScores.ContainsKey(recordKey)) {
                            // Get Player Score
                            score = playerScores[recordKey];
                        } else {
                            // Create Player Score (for Current Map)
                            score = new PlayerScore(mapKey, mapName, playerName, playerLogin);
                            playerScores.Add(recordKey, score);

                            // Log Info Details
                            LogInfoMessage("Created New Player Score for {0}", recordKey);
                        }

                        try {
                            // Add Attempt to Player Score
                            AttemptStatus status = score.addAttempt(UInt32.Parse(bestTime), UInt32.Parse(lastTime));

                            // Check Status
                            if (status.isNewAttempt()) {
                                // Write Last Attempt to Session File
                                WriteAttemptToFile(SessionFile, score, status.attempt);

                                // Check Instant Upload
                                if (chkInstantUpload.Checked) {
                                    // Upload Attempt to Web Server
                                    Uploader.UploadPlayerScoreAsync(score, status.attempt, currentChallenge);
                                }
                            }
                        } catch (Exception e) {
                            // Log Error Details
                            LogErrorMessage("Exception: " + e.Message);
                            lblStatus.BackColor = Color.OrangeRed;
                        }
                    } else {
                        // Log Error Details
                        LogErrorMessage("[ERROR] Invalid Player Record {0}", record);
                        lblStatus.BackColor = Color.OrangeRed;
                    }
                }
            }
        } catch (Exception e) {
            // Log Error Details
            LogErrorMessage("Exception: " + e.Message);
        } finally {
            // Always Release Mutex
            processMutex.ReleaseMutex();
        }
    }

    public void WriteLineToFile(String file, String text) {
        // Write Single Line to File
        using (StreamWriter sw = File.AppendText(file)) {
            sw.WriteLine(text);
        }
    }

    public void WriteLineToLogFile(String text) {
        // Write Single Line to Log File
        WriteLineToFile(LogFile, text);
    }

    public void WriteLineToRawFile(String data) {
        // Write Single Line to Raw File
        WriteLineToFile(RawFile, data);
    }

    public void WriteScoreToFile(String file, PlayerScore score) {
        // Write All Attempts to Given File
        using (StreamWriter sw = File.AppendText(file)) {
            // Get All Attempts of Player Score
            foreach (Attempt attempt in score.getAttempts()) {
                // Build and Write Player Record to File
                sw.WriteLine(String.Join(";", BuildAttemptRecord(score, attempt)));
            }
        }
    }

    public void SavePlayerScores() {
        // Write Players Scores to Upload File
        foreach (PlayerScore score in playerScores.Values) {
            // Write Current Score to Upload File
            WriteScoreToFile(UploadFile, score);
        }

        // Get File Infos
        FileInfo uploadFileInfo = new FileInfo(UploadFile);
        FileInfo sessionFileInfo = new FileInfo(SessionFile);

        // Check Upload and Session File
        if (!uploadFileInfo.Exists || !sessionFileInfo.Exists) {
            return;
        }

        // Check Size of Upload and Session File
        if (uploadFileInfo.Length == sessionFileInfo.Length) {
            // Log Info Details
            LogInfoMessage("Upload File Size Comparison Successful :)");
        } else {
            // Log Warning Details
            LogWarnMessage("Upload File Size Mismatch {0} != {1}. Check Files!", uploadFileInfo.Length, sessionFileInfo.Length);
        }

        // Generate Timestamp String
        String timeString = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");

        // Move Files to Permanent Location
        uploadFileInfo.MoveTo(String.Format("Upload-{0}.csv", timeString));
        sessionFileInfo.MoveTo(String.Format("Session-{0}.csv", timeString));

        // Clear Player Scores
        playerScores.Clear();
    }

    public List<String> BuildAttemptRecord(PlayerScore score, Attempt attempt) {
        // List of Line Items
        List<String> record = new List<String>();

        // Build Player Record
        record.Add(attempt.timestamp);
        record.Add(currentChallenge.ID);
        record.Add(currentChallenge.Name);
        record.Add(score.mapKey);
        record.Add(score.mapName);
        record.Add(score.playerName);
        record.Add(score.playerLogin);
        record.Add(attempt.raceTime.ToString());
        record.Add(attempt.isNewBestTime.ToString());

        // Return Player Record
        return record;
    }

    public void WriteAttemptToFile(String file, PlayerScore score, Attempt attempt) {
        // Build and Write Player Record to File
        WriteLineToFile(file, String.Join(";", BuildAttemptRecord(score, attempt)));
    }

    public void LogMessage(LogLevel level, String msg) {
        if (lstOutput.InvokeRequired) {
            BeginInvoke(new LogMessageHandler(LogMessage), level, msg);
        } else {
            // Add Timestamp to Message
            msg = String.Format("{0} [{1,-5}] - {2}", DateTime.Now.ToString("HH:mm:ss.fff"), level, msg);

            // Clear Items at the Beginning
            if (lstOutput.Items.Count > 1000) {
                lstOutput.Items.RemoveAt(0);
            }

            // Check Log Level
            if ((uint)level >= (uint)LogLevel.INFO) {
                // Write Log Message to File
                WriteLineToLogFile(msg);

                // Check WSACancelBlockingCall
                if (msg.Contains("WSACancelBlockingCall")) {
                    return;
                }

                // Check Thread Abort Exception
                if (msg.Contains("System.Threading.ThreadAbortException")) {
                    return;
                }

                // Add Log Item to Output List
                lstOutput.SelectedIndex = lstOutput.Items.Add(msg);
            }
        }
    }

    public void LogDebugMessage(String msg) {
        LogMessage(LogLevel.DEBUG, msg);
    }

    public void LogDebugMessage(String format, params object[] args) {
        LogMessage(LogLevel.DEBUG, String.Format(format, args));
    }

    public void LogInfoMessage(String msg) {
        LogMessage(LogLevel.INFO, msg);
    }

    public void LogInfoMessage(String format, params object[] args) {
        LogMessage(LogLevel.INFO, String.Format(format, args));
    }

    public void LogWarnMessage(String msg) {
        LogMessage(LogLevel.WARN, msg);
    }

    public void LogWarnMessage(String format, params object[] args) {
        LogMessage(LogLevel.WARN, String.Format(format, args));
    }

    public void LogErrorMessage(String msg) {
        LogMessage(LogLevel.ERROR, msg);
    }

    public void LogErrorMessage(String format, params object[] args) {
        LogMessage(LogLevel.ERROR, String.Format(format, args));
    }

    private void btnStart_Click(object sender, EventArgs e) {
        // Grey Logic
        btnStart.Enabled = false;
        btnStop.Enabled = true;
        nudPort.Enabled = false;
        cbChallenge.Enabled = false;

        // Start Server
        StartServerThread("127.0.0.1", Convert.ToInt32(nudPort.Value));
    }

    private void btnStop_Click(object sender, EventArgs e) {
        // Grey Logic
        btnStart.Enabled = true;
        btnStop.Enabled = false;
        nudPort.Enabled = true;
        cbChallenge.Enabled = true;

        // Stop Server
        StopServerThread();

        // Save Scores
        SavePlayerScores();
    }

    private void StartServerThread(String host, int port) {
        // Stop Server Thread
        StopServerThread();

        // Check Thread
        if (thServer == null) {
            thServer = new Thread(new ParameterizedThreadStart(StartServer));
            thServer.IsBackground = true;
            thServer.Start(new SocketAddress(host, port));
        }
    }

    private void StopServerThread() {
        // Check Server Thread
        if (thServer != null && thServer.IsAlive) {
            try {
                // Stop Server
                if (server != null) {
                    server.Stop();
                }

                // Stop Server Thread
                thServer.Abort();
                thServer.Join();

                // Log Message
                LogInfoMessage("Stopped Server.");
            } catch (Exception e) {
                // Log Error Details
                LogErrorMessage("Exception: " + e.Message);
            }
        }

        // Clear Variables
        thServer = null;
        server = null;
    }

    private void StartServer(Object objectParam) {
        try {
            // Host and Port
            SocketAddress address;

            // Check Parameters
            if (objectParam is SocketAddress) {
                // Parse Parameters
                address = (SocketAddress)objectParam;
            } else {
                // Invalid Parameters
                return;
            }

            // Start Server
            server = new TcpListener(IPAddress.Parse(address.host), address.port);
            server.Server.ReceiveTimeout = 2000;
            server.Server.SendTimeout = 2000;
            server.Start();

            // Log Messages
            LogInfoMessage("Server has started on {0}:{1}.", address.host, address.port);
            LogInfoMessage("Waiting for a connection...");

            // Thread Helpers
            bool isInterrupted = false;

            // Wait for Client Connections
            while (!isInterrupted) {
                try {
                    var client = server.AcceptTcpClient();
                    var cw = new ClientWorking(this, client, true);
                    cw.ProcessClientAsync().NoWarning();
                } catch (ThreadInterruptedException) {
                    isInterrupted = true;
                }
            }

            // Stop Server
            server.Stop();

            // Log Message
            LogInfoMessage("Server Thread Terminated Peacefully!");
        } catch (Exception e) {
            // Log Error Details
            LogErrorMessage("Exception: " + e.Message);
        }
    }

    private void btnUpload_Click(object sender, EventArgs e) {
        // Create Open File Dialog
        OpenFileDialog dialog = new OpenFileDialog();

        // Configure Dialog
        dialog.Filter = "CSV-Files (*.csv)|*.csv|All Files (*.*)|*.*";
        dialog.Multiselect = false;
        dialog.CheckFileExists = true;

        // Open File Chooser Dialog
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
            // Create Uploader Dialog
            Uploader uploader = new Uploader(dialog.FileName);

            // Show Uploader Dialog
            uploader.Show();
        }
    }

    private void btnUploadFolder_Click(object sender, EventArgs e) {
        // Create Open Folder Dialog
        FolderBrowserDialog dialog = new FolderBrowserDialog();

        // Configure Dialog
        dialog.ShowNewFolderButton = false;
        dialog.RootFolder = Environment.SpecialFolder.MyComputer;
        dialog.SelectedPath = Directory.GetCurrentDirectory();

        // Open File Chooser Dialog
        if (dialog.ShowDialog() == DialogResult.OK && !String.IsNullOrWhiteSpace(dialog.SelectedPath)) {
            // Get Directory Info
            DirectoryInfo info = new DirectoryInfo(dialog.SelectedPath);

            // Check Directory
            if (String.IsNullOrEmpty(dialog.SelectedPath) || !info.Exists) {
                LogMessage(LogLevel.ERROR, "Selected Path is empty or does not exist!");
                return;
            }

            // Get All Files of Directory
            FileInfo[] files = info.GetFiles("*.csv");

            // Decrypt All *.bin Files
            foreach (FileInfo file in files) {
                try {
                    // Decrypt File
                    Uploader uploader = new Uploader(file.FullName);

                    // Upload File
                    uploader.UploadAndExit();

                    // Show Uploader
                    uploader.ShowDialog();

                    // Log Message
                    LogMessage(LogLevel.INFO, String.Format("Successfully uploaded {0}", file.Name));
                } catch (Exception ex) {
                    // Error During Decryption
                    LogMessage(LogLevel.ERROR, String.Format("Error During Uploading: {0}", ex.Message));
                }
            }
        }
    }
}

public class ClientWorking {

    // Members
    private Main _main;
    private TcpClient _client;
    private bool _ownsClient;

    public ClientWorking(Main main, TcpClient client, bool ownsClient) {
        _main = main;
        _client = client;
        _ownsClient = ownsClient;
    }

    public async Task ProcessClientAsync() {
        // Start Async Task for Client
        var task = Task.Run(() => ProcessClient());

        // Run Communication Task for max 10 Seconds
        if (await Task.WhenAny(task, Task.Delay(10 * 1000)) == task) {
            _main.LogDebugMessage("Peaceful Disconnect");
        } else {
            _main.LogInfoMessage("Client Timeout");
        }

        // Close Client Connection
        if (_client != null && _client.Connected) {
            _client.Close();
        }
    }

    private void ProcessClient() {
        // Storage for Network Data
        LinkedList<String> networkData = new LinkedList<String>();

        try {
            // Open HTTP Stream
            using (var stream = _client.GetStream()) {
                // Stream Reader
                using (var sr = new StreamReader(stream)) {
                    // Stream Writer
                    using (var sw = new StreamWriter(stream)) {
                        // Data Storage
                        var data = default(string);

                        // Client Connected
                        _main.LogDebugMessage("Client Connected!");

                        try {
                            // Read until Empty Line
                            while ((data = sr.ReadLine()) != null && !String.IsNullOrEmpty(data)) {
                                // Add Data to Linked List
                                networkData.AddLast(data);
                            }
                        } catch (Exception e) {
                            // Log Error Details
                            _main.LogErrorMessage("Exception: " + e.Message);
                        }

                        // Send Disconnect Message
                        sw.Write("\n");
                        sw.Flush();

                        // Client Disconnected
                        _main.LogDebugMessage("Client Disconnected");
                    }
                }
            }
        } catch (Exception e) {
            // Log Error Details
            _main.LogErrorMessage("Exception: " + e.Message);
        } finally {
            // Dispose Client
            if (_ownsClient && _client != null) {
                (_client as IDisposable).Dispose();
                _client = null;
            }
        }

        // Process Data
        _main.ProcessData(networkData);
    }
}

public class AttemptStatus {

    // Attempt
    public Attempt attempt;

    // Race Time Updates
    public bool isNewBestTime;
    public bool isNewLastTime;

    public AttemptStatus() {
        // Attempt
        this.attempt = null;

        // Race Time Updates
        this.isNewBestTime = false;
        this.isNewLastTime = false;
    }

    public bool isNewAttempt() {
        return isNewBestTime || isNewLastTime;
    }
}

public class Attempt {

    // Timestamp
    public String timestamp;

    // Race Time
    public UInt32 raceTime;

    // Best Time Indicator
    public bool isNewBestTime;

    public Attempt(String timestamp, UInt32 raceTime, bool isNewBestTime) {
        // Timestamp
        this.timestamp = timestamp;

        // Race Time Status
        this.raceTime = raceTime;
        this.isNewBestTime = isNewBestTime;
    }
}

public class PlayerScore {

    // Map Info
    public String mapKey;
    public String mapName;

    // Player Info
    public String playerName;
    public String playerLogin;

    // Race Time Info
    private UInt32 bestTime;
    private UInt32 lastTime;

    // Store Attempts
    private List<Attempt> attempts;

    public PlayerScore(String mapKey, String mapName, String playerName, String playerLogin) {
        this.mapKey = mapKey;
        this.mapName = mapName;

        this.playerName = playerName;
        this.playerLogin = playerLogin;

        this.bestTime = UInt32.MaxValue;
        this.lastTime = UInt32.MaxValue;

        this.attempts = new List<Attempt>();
    }

    public AttemptStatus addAttempt(UInt32 newBestTime, UInt32 newLastTime) {
        // Race Time Update Status
        AttemptStatus status = new AttemptStatus();

        // Check Single Player Mode
        if (newBestTime == UInt32.MaxValue && newLastTime < UInt32.MaxValue) {
            newBestTime = newLastTime;
        }

        // Check New Best Time
        if (newBestTime < this.bestTime) {
            this.bestTime = newBestTime;

            // Update Status
            status.isNewBestTime = true;
        }

        // Check Last Time Updates
        if (newLastTime != this.lastTime) {
            this.lastTime = newLastTime;

            // Update Status
            status.isNewLastTime = true;
        }

        // Get Current Timestamp
        String timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // Check Attempt
        if (status.isNewBestTime) {
            // Log New Best Time
            Main.Instance.LogInfoMessage("New Best Time {0} for {1}.", this.bestTime, this.playerName);

            // Set Attempt with Current Best Time
            status.attempt = new Attempt(timestamp, this.bestTime, status.isNewBestTime);
        } else if (status.isNewLastTime) {
            // Log New Attempt
            Main.Instance.LogInfoMessage("New Attempt {0} for {1}.", this.lastTime, this.playerName);

            // Set Attempt with Current Time
            status.attempt = new Attempt(timestamp, this.lastTime, status.isNewBestTime);
        }

        // Check Attempt
        if (status.attempt != null) {
            // Add Attempt with Current Time
            attempts.Add(status.attempt);
        }

        // Return Status
        return status;
    }

    public List<Attempt> getAttempts() {
        return this.attempts;
    }

    public UInt32 getBestTime() {
        return this.bestTime;
    }
}

}
