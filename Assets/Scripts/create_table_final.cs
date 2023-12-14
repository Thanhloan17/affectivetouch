using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class create_table_final : MonoBehaviour
{
    // the file with names of the files should have firstly named files of splashes - 1-4 and than they should have named files of audio or compex signals - 5-12

    [SerializeField] private int NumOfATM = 5; // NUMBERS 5
    [SerializeField] private int NumSpeedsForATM = 4; 
    [SerializeField] private int NumStepsOfATM = 3;
    [SerializeField] private List<List<float>> BrushSpeedsForATM1 = new List<List<float>>();
    [SerializeField] private int NumRepetations = 5;

    
    void Start()
    {
       // declare the brush speeds for each of the ATM
        BrushSpeedsForATM1.Add( new List<float> { 0.2f, 0.3f, 0.45f, 30f} );
        BrushSpeedsForATM1.Add(new List<float> { 0.67f, 1f, 1.5f, 10f });
        BrushSpeedsForATM1.Add(new List<float> { 2f, 3f, 4.5f, 1f });
        BrushSpeedsForATM1.Add(new List<float> { 6.67f, 10f, 15f, 1f });
        BrushSpeedsForATM1.Add(new List<float> { 20f, 30f, 45f, 0.3f });
    }

    // This function to create and save randomized trial data to a CSV file
    public void CreateCSV()
    {
        //int NumTrials = NumOfATM * NumSpeedsForATM * NumRepetations; //100

        // Create a list to hold the lines of the CSV file
        List<string> lines = new List<string>();
        // Add the CSV header line
        lines.Add("Trial,Brush/Visual speed,Audio 1,Audio 2,Audio 3,Rank,Magnitude estimation");

        // Generate all trials 
        List<List<string>> AllTrials = GenerateATMForTrials(BrushSpeedsForATM1);

        // Generate trial data and write to CSV lines
        for (int i = 0; i < AllTrials.Count; i++)
        {
            // create trials for the .csv file
            List<string> trialData1 = GenerateTrialData("Trials " + (i + 1), AllTrials[i]);
            print(AllTrials[i]);

            // Add trial data to the CSV lines
            lines.Add(string.Join(",", trialData1));

        }

        // Define the path to the CSV file
        string path = Path.Combine(Application.dataPath, "Resources/GeneratedData.csv");

        // Write all the lines to the CSV file
        File.WriteAllLines(path, lines.ToArray());

        // Log the path of the generated CSV file
        Debug.Log("CSV file generated at: " + path);
    }

    // create a list of trials
    private List<List<string>> GenerateATMForTrials(List<List<float>> brushSpeeds)
    {
        List<List<string>> allATM = new List<List<string>>();
        for (int t = 0; t < NumOfATM; t++) // 5
        {
            for (int visualSpeed = 0; visualSpeed < NumSpeedsForATM; visualSpeed++) // 4
            {
                for (int i = 0; i < NumRepetations; i++) // 5
                {
                    // Create a new trial
                    List<string> trialData = new List<string>();

                    // Add the brush speed as the first element
                    trialData.Add(brushSpeeds[t][visualSpeed].ToString());

                    // Add other components to the trial data
                    for (int a = 0; a < NumStepsOfATM; a++) // 3
                    {
                        trialData.Add($"{t+1}.{a}");
                    }

                    // Collect the trial data into the list of all trials
                    allATM.Add(trialData);
                }
            }
        }
        ShuffleList(allATM);
        return allATM;
    }
        
    // Function to generate randomized trial data
    private List<string> GenerateTrialData(string trialName, List<string> trial)
    {
        //ShuffleList(stimuli); // Shuffle the list for each trial

        List<string> trialData = new List<string>();
        trialData.Add(trialName);

        for (int i = 0; i < trial.Count; i++)
        {
            trialData.Add(trial[i]);
        }

        return trialData;
    }
    

    // Fonction pour mélanger une liste d'entiers
    void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}





