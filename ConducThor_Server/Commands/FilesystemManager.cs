using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ConducThor_Server.Utility;
using ConducThor_Shared;

namespace ConducThor_Server.Commands
{
    class FilesystemManager : ManagerClass
    {
        private String BasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Conducthor");
        private String ResultPath => System.IO.Path.Combine(BasePath, "results");

        public override void Initialize()
        {
            //check dirs
            if (!System.IO.Directory.Exists(BasePath))
            {
                NotifyNewLogMessageEvent($"Directory {BasePath} does not exist. Creating...");
                System.IO.Directory.CreateDirectory(BasePath);
                NotifyNewLogMessageEvent($"Directory {BasePath} created");
            }

            if (!System.IO.Directory.Exists(ResultPath))
            {
                NotifyNewLogMessageEvent($"Directory {ResultPath} does not exist. Creating...");
                System.IO.Directory.CreateDirectory(ResultPath);
                NotifyNewLogMessageEvent($"Directory {ResultPath} created");
            }

            base.Initialize();
        }

        public bool CheckIfFileExists(String pParameters)
        {
            var para = CleanParameters(pParameters);

            //check directory
            var dir = System.IO.Path.Combine(ResultPath, para);
            if (!System.IO.Directory.Exists(dir))
                return false;

            //check if directory is empty
            if (Directory.EnumerateFiles(dir).Any())
                return true;

            return false;
        }

        public void WriteResultsToFilesystem(ResultPackage pResults)
        {
            //get params
            var parameters = CleanParameters(pResults.WorkPackage.Commands.First().Parameters);

            //create dir 
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(ResultPath, parameters));

            //write transmitted files
            foreach(var file in pResults.ResultFiles)
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(ResultPath, parameters, file.Filename),file.FileData);

            //write meta file
            using (var filestream =new FileStream(System.IO.Path.Combine(ResultPath, parameters, GetMetaFilenameFromParameters(parameters)),FileMode.CreateNew))
            {
                var xmlserializer = new XmlSerializer(typeof(MetaStruct));
                xmlserializer.Serialize(filestream,new MetaStruct
                {
                    Duration = $"{pResults.DurationTime.TotalSeconds}s",
                    Epochs = pResults.ClientStatusAtEnd.CurrentEpoch,
                    LastEpochDuration = pResults.ClientStatusAtEnd.LastEpochDuration,
                    NodeName = pResults.MachineData.Name,
                    OS = pResults.MachineData.OperatingSystem.ToString(),
                    Version = pResults.MachineData.ContainerVersion,
                    ProcessingUnit = pResults.MachineData.ProcessingUnit.ToString()
                });
                filestream.Close();
            }
            //write logfile
            File.WriteAllLines(System.IO.Path.Combine(ResultPath, parameters, GetLogFilenameFromParameters(parameters)), pResults.OutLog);

            NotifyNewLogMessageEvent($"Saved results to file for {parameters}");
        }

        private String GetModelFilenameFromParameters(String pParameters)
        {
            //e.g. for pParameters: "1 3 0.2 200" => "model-1 3 0.2 200"
            return $"model-{pParameters}.h5";
        }
        private String GetPredictionFilenameFromParameters(String pParameters)
        {
            return $"results-{pParameters}.csv";
        }
        private String GetMetaFilenameFromParameters(String pParameters)
        {
            return $"meta-{pParameters}.xml";
        }
        private String GetLogFilenameFromParameters(String pParameters)
        {
            return $"log-{pParameters}.txt";
        }

        private String CleanParameters(String pParameters)
        {
            return pParameters.Replace(",", ".");
        }
    }
    public class MetaStruct
    {
        public String Duration { get; set; }
        public int Epochs { get; set; }
        public String LastEpochDuration { get; set; }
        public String NodeName { get; set; }
        public String Version { get; set; }
        public String OS { get; set; }
        public String ProcessingUnit { get; set; }
    }
}
