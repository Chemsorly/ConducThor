using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ConducThor_Server.Utility
{
    public enum VersionStatus { UpToDate, UpdateAvailable, UpdateUnknown };

    public class UpdateNotifier
    {
        private readonly ApplicationDeployment applicationDeployment;
        private Timer timer;
        private bool processing;

        public event PropertyChangedEventHandler PropertyChanged;
        private VersionStatus _status = VersionStatus.UpToDate;
        public VersionStatus Status
        {
            get { return _status; }
            private set { _status = value; OnPropertyChanged("VersionStatus"); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UpdateNotifier()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
                return;
            applicationDeployment = ApplicationDeployment.CurrentDeployment;
            timer = new Timer {Interval = 60000, AutoReset = true };
            timer.Elapsed += Timer_Elapsed;
            Timer_Elapsed(null, null);
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (processing)
                return;

            timer.Stop();
            processing = true;
            try
            {
                var info = applicationDeployment.CheckForDetailedUpdate();

                if (info.UpdateAvailable)
                    Status = VersionStatus.UpdateAvailable;
                else
                    Status = VersionStatus.UpToDate;
            }
            catch (Exception)
            {
                Status = VersionStatus.UpdateUnknown;
            }

            processing = false;
            timer.Start();
        }

    }
}
