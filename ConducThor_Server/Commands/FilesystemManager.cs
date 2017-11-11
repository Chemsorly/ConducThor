using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
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
            //check directory
            var dir = System.IO.Directory.Exists(System.IO.Path.Combine(ResultPath, pParameters));
            if (!dir)
                return false;

            //check model pParameters
            var file1 = System.IO.File.Exists(System.IO.Path.Combine(ResultPath, pParameters,
                GetModelFilenameFromParameters(pParameters)));

            //check predictions file
            var file2 = System.IO.File.Exists(System.IO.Path.Combine(ResultPath, pParameters,
                GetPredictionFilenameFromParameters(pParameters)));

            if (file1 && file2)
                return true;
            return false;
        }

        public void WriteResultsToFilesystem(ResultPackage pResults)
        {
            //get params
            var parameters = pResults.pWorkPackage.Commands.First().Parameters;

            //write model-file
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(ResultPath, parameters, GetModelFilenameFromParameters(parameters)), 
                pResults.ModelFile);
            //write predictions-file
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(ResultPath, parameters, GetPredictionFilenameFromParameters(parameters)),
                pResults.PredictionFile);

            NotifyNewLogMessageEvent($"Saved results to file for {parameters}");
        }

        private String GetModelFilenameFromParameters(String pParameters)
        {
            //e.g. for pParameters: "1 3 0,2 200" => "model-1 3 0,2 200"
            return $"model-{pParameters}.h5";
        }
        private String GetPredictionFilenameFromParameters(String pParameters)
        {
            return $"results-{pParameters}.csv";
        }
    }
}
