using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using TurboCup.Data;
using TurboCup.Extensions;
using CsvFiles;

namespace TurboCup {

// Progress Bar Delegates
public delegate void InitProgressBarHandler(int max);
public delegate void UpdateProgressBarHandler(int value, int max, String info);

public partial class Uploader : Form {

    // Static Members
    private const String uploadUrl = "https://your-website/upload.php";

    // Login Credentials
    private const String username = "website-user";
    private const String password = "website-password";

    // Member Variables
    private String uploadFile;
    private Thread thUpload;

    // Define Exit Behaviour
    private bool exitAfterUpload = false;

    public Uploader(String file) {
        // Initialize Form
        InitializeComponent();

        // Set Members
        this.uploadFile = file;

        // Set Labels
        this.lblFilename.Text = new FileInfo(this.uploadFile).Name;
    }

    public static String BuildUploadQuery(PlayerScore score, Attempt attempt, Challenge challenge) {
        // Initialize Query Builder
        var uriBuilder = new UriBuilder(uploadUrl);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        // Build Player Record
        query["Timestamp"] = attempt.timestamp;
        query["ChallengeID"] = challenge.ID;
        query["ChallengeName"] = challenge.Name;
        query["MapID"] = score.mapKey;
        query["MapName"] = score.mapName;
        query["PlayerID"] = score.playerLogin;
        query["PlayerName"] = score.playerName;
        query["RaceTime"] = attempt.raceTime.ToString();
        query["NewBestTime"] = attempt.isNewBestTime.ToString();

        // Build Query
        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    public static String BuildUploadQuery(UploadRecord record) {
        // Initialize Query Builder
        var uriBuilder = new UriBuilder(uploadUrl);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        // Build Player Record
        query["Timestamp"] = record.Timestamp;
        query["ChallengeID"] = record.ChallengeID;
        query["ChallengeName"] = record.ChallengeName;
        query["MapID"] = record.MapKey;
        query["MapName"] = record.MapName;
        query["PlayerID"] = record.PlayerLogin;
        query["PlayerName"] = record.PlayerName;
        query["RaceTime"] = record.RaceTime;
        query["NewBestTime"] = record.IsNewBestTime;

        // Build Query
        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    public static void UploadPlayerScoreAsync(PlayerScore score, Attempt attempt, Challenge challenge) {
        // Build Upload Query and Send Web Request with Player Score
        WebUtils.SendRequestAsync(BuildUploadQuery(score, attempt, challenge), username, password).NoWarning();
    }

    public static HttpStatusCode UploadPlayerRecord(UploadRecord record) {
        // Build Upload Query and Send Web Request with Player Record
        return WebUtils.SendRequest(BuildUploadQuery(record), username, password);
    }

    public void UploadAndExit() {
        // Start Upload
        btnStart_Click(null, null);

        // Set Exit Behaviour
        this.exitAfterUpload = true;
    }

    private void btnStart_Click(object sender, EventArgs args) {
        // Stop Upload Thread
        StopUploadThread();

        // Start Upload Thread
        StartUploadThread();
    }

    private void StopUploadThread() {
        // Check Thread
        if (thUpload != null) {
            // Abort Thread and Wait
            thUpload.Abort();
            thUpload.Join();

            // Clear Variable
            thUpload = null;
        }
    }

    private void StartUploadThread() {
        // Check Thread
        if (thUpload == null) {
            // Disable Start Button
            btnStart.Enabled = false;

            // Create and Start Upload Thread
            thUpload = new Thread(new ThreadStart(UploadFile));
            thUpload.IsBackground = true;
            thUpload.Start();
        }
    }

    private void InitProgressBar(int max) {
        if (pbRecords.InvokeRequired) {
            // Invoke Progress Bar Update
            BeginInvoke(new InitProgressBarHandler(InitProgressBar), max);
        } else {
            // Update Progress Bar
            pbRecords.Minimum = 0;
            pbRecords.Maximum = max;
            pbRecords.Value = 0;
        }
    }

    private void EnableStartButton() {
        if (btnStart.InvokeRequired) {
            // Invoke Button Update
            BeginInvoke(new MethodInvoker(EnableStartButton));
        } else {
            // Update Button
            btnStart.Enabled = true;
        }
    }

    private void UpdateProgressBar(int value, int max, String info) {
        if (pbRecords.InvokeRequired) {
            // Invoke Progress Bar Update
            BeginInvoke(new UpdateProgressBarHandler(UpdateProgressBar), value, max, info);
        } else {
            // Update Progress Bar
            pbRecords.Value = value;

            // Update Labels
            lblRecords.Text = String.Format("Record {0} of {1}", value, max);
            lblProgress.Text = String.Format("Progress: {0:N2} %", ((decimal)value / (decimal)max) * 100);
            lblUser.Text = String.Format("Current User: {0}", info);
        }
    }

    private void UploadFile() {
        // Load CSV File as Upload Record List
        List<UploadRecord> records = UploadData.LoadFile(this.uploadFile);

        // Prepare Progress Bar
        InitProgressBar(records.Count);

        // Read Player Scores of List
        for (int index = 0, value = 1; index < records.Count; index++, value++) {
            // Current Player Record
            UploadRecord record = records[index];

            // Update Labels
            UpdateProgressBar(value, records.Count, record.PlayerName);

            // Upload Player Record
            HttpStatusCode status = UploadPlayerRecord(record);

            // Check Status
            if (status != HttpStatusCode.OK) {
                MessageBox.Show("HTTP Status Code: " + status, "Error While Uploading!");
                break;
            }
        }

        // Enable Start Button
        EnableStartButton();

        // Check Exit Behaviour
        if (this.exitAfterUpload) {
            // Close Dialog
            BeginInvoke(new MethodInvoker(Close));
        }
    }
}
}
