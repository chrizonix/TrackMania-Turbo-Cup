using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvFiles;

namespace TurboCup.Data {
public class ChallengeData {

    public static List<Challenge> Challenges = new List<Challenge>() {
        new Challenge("1", "Challenge 1", "2020-03-17"),
            new Challenge("2", "Challenge 2", "2020-04-21"),
            new Challenge("3", "Challenge 3", "2020-05-26"),
            new Challenge("4", "Challenge 4", "2020-06-16"),
            new Challenge("5", "Challenge 5", "2020-07-31"),
            new Challenge("6", "Challenge 6", "2020-08-28"),
            new Challenge("7", "Challenge 7", "2020-09-15"),
            new Challenge("8", "Challenge 8", "2020-10-13"),
            new Challenge("9", "Challenge 9", "2020-11-17"),
    };

    // CSV FileName
    public const String FileName = "config.ini";

    // CSV Definition
    private static CsvDefinition csvDefinition = new CsvDefinition() {
        Header = "ID;Name;Date;DisplayName",
        FieldSeparator = ';'
    };

    public static void DeleteFile() {
        new FileInfo(FileName).Delete();
    }

    public static bool FileExists() {
        return new FileInfo(FileName).Exists;
    }

    public static void LoadFile() {
        // Create CSV Reader
        CsvFileReader<Challenge> csvReader = new CsvFileReader<Challenge>(new StreamReader(FileName), csvDefinition);

        // Clear List
        ChallengeData.Challenges.Clear();

        // Load External Data
        foreach (Challenge challenge in csvReader) {
            ChallengeData.Challenges.Add(challenge);
        }

        // Close Reader
        csvReader.Dispose();
        csvReader = null;
    }

    public static void ExportFile() {
        // Create CSV Writer
        CsvFile<Challenge> csvWriter = new CsvFile<Challenge>(FileName, csvDefinition);

        // Export Data
        foreach (Challenge challenge in ChallengeData.Challenges) {
            csvWriter.Append(challenge);
        }

        // Close File
        csvWriter.Dispose();
        csvWriter = null;
    }
}

public class Challenge {

    public String ID { get; set; }
    public String Name { get; set; }
    public String Date { get; set; }
    public String DisplayName { get; set; }

    public Challenge() {
        this.ID = "";
        this.Name = "";
        this.Date = "";
        this.DisplayName = "";
    }

    public Challenge(String ID, String Name, String Date) {
        // Members
        this.ID = ID;
        this.Name = Name;
        this.Date = Date;

        // Generate Display Name
        this.DisplayName = String.Format("{0} - {1}", this.Name, this.Date);
    }
}
}
