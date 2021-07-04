using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CsvFiles;

namespace TurboCup.Data {
public class UploadData {

    // CSV Definition
    private static CsvDefinition csvDefinition = new CsvDefinition() {
        Header = "Timestamp;ChallengeID;ChallengeName;MapKey;MapName;PlayerName;PlayerLogin;RaceTime;IsNewBestTime",
        FieldSeparator = ';'
    };

    public static List<UploadRecord> LoadFile(String fileName) {
        // Create CSV Reader for Upload Records
        CsvFileReader<UploadRecord> csvReader = null;
        List<UploadRecord> records = new List<UploadRecord>();

        try {
            // Create CSV Reader with Column Definition for Upload Records
            csvReader = new CsvFileReader<UploadRecord>(new StreamReader(fileName), csvDefinition);

            // Generate List of Records
            records = new List<UploadRecord>(csvReader);
        } catch (Exception e) {
            // Show Error Details
            MessageBox.Show(e.Message, "Error While Reading CSV File!");
        } finally {
            // Close Reader
            if (csvReader != null) {
                csvReader.Dispose();
            }
        }

        // Return Records
        return records;
    }
}

public class UploadRecord {

    public String Timestamp { get; set; }
    public String ChallengeID { get; set; }
    public String ChallengeName { get; set; }
    public String MapKey { get; set; }
    public String MapName { get; set; }
    public String PlayerName { get; set; }
    public String PlayerLogin { get; set; }
    public String RaceTime { get; set; }
    public String IsNewBestTime { get; set; }

    public UploadRecord() {
        this.Timestamp = "";
        this.ChallengeID = "";
        this.ChallengeName = "";
        this.MapKey = "";
        this.MapName = "";
        this.PlayerName = "";
        this.PlayerLogin = "";
        this.RaceTime = "";
        this.IsNewBestTime = "";
    }
}
}
