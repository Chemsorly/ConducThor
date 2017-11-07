using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConducThor_Client.Machine
{
    interface IFilesystemManager
    {
        byte[] GetFileFromFS(String pFile);

        String GetResultsFromCSV(String pFile);
    }
}
