﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Server.Utility;
using ConducThor_Shared;
using ConducThor_Shared.Enums;

namespace ConducThor_Server.Commands
{
    class CommandManager : ManagerClass
    {
        private ResultManager _resultManager;

        //workitems
        public AsyncObservableCollection<WorkItem> QueuedWorkItems { get; set; }
        public AsyncObservableCollection<WorkItem> ActiveWorkItems { get; set; }

        public override void Initialize()
        {
            QueuedWorkItems = new AsyncObservableCollection<WorkItem>();
            ActiveWorkItems = new AsyncObservableCollection<WorkItem>();

            _resultManager = new ResultManager();
            _resultManager.NewLogMessageEvent += NotifyNewLogMessageEvent;
            _resultManager.Initialize();
            base.Initialize();
        }

        public void CreateWorkParameters(List<List<String>> pCombinations)
        {
            NotifyNewLogMessageEvent("Create new set of work parameters.");
            var combinations = CombinationHelper.AllCombinationsOf(pCombinations.ToArray());

            //fill up queue // comb separated by
            foreach (var comb in combinations)
            {
                var combstring = String.Join(" ", comb);

                //check if result items already exist; if it does, skip this combination
                if (_resultManager.CheckIfResultExists(combstring) || IsAlreadyInList(combstring))
                    continue;

                AddNewWorkItem(combstring);
            }

            NotifyNewLogMessageEvent($"Finished creating new set of work parameters. Total: {QueuedWorkItems.Count}");
        }

        public WorkPackage FetchWork(OSEnum pOS, String pAssignedClient)
        {
            if (!QueuedWorkItems.Any())
                return null;

            //get first item in list and create package for it
            var item = QueuedWorkItems.First();
            var package = item.Start(pOS, pAssignedClient);

            //move from queued to active
            QueuedWorkItems.Remove(item);
            ActiveWorkItems.Add(item);

            return package;
        }

        public void SendResults(ResultPackage pResults)
        {
            //if save results have been successfully saved, remove item from active operations
            if (_resultManager.VerifyAndSave(pResults))
            {
                NotifyNewLogMessageEvent($"Attempt to remove \"{pResults.WorkPackage.Commands.First().Parameters}\" from queue.");

                //remove from active
                var workitem = ActiveWorkItems.FirstOrDefault(t => t.Parameters == pResults.WorkPackage.Commands.First().Parameters);
                if (workitem != null)
                {
                    ActiveWorkItems.Remove(workitem);
                    NotifyNewLogMessageEvent($"Removed {workitem.Parameters} from active work.");
                }

                //(edge case): remove from queued (e.g. restarting server)
                var queueitem = QueuedWorkItems.FirstOrDefault(t => t.Parameters == pResults.WorkPackage.Commands.First().Parameters);
                if (queueitem != null)
                {
                    QueuedWorkItems.Remove(queueitem);
                    NotifyNewLogMessageEvent($"Removed {queueitem.Parameters} from queue.");
                }
            }
        }

        private bool IsAlreadyInList(String pParameters)
        {
            return QueuedWorkItems.Any(t => t.Parameters == pParameters) || ActiveWorkItems.Any(t => t.Parameters == pParameters);
        }

        private void AddNewWorkItem(String pParameters)
        {
            var newitem = new WorkItem(pParameters);
            newitem.OnTimeoutHappenedEvent += sender =>
            {
                //remove
                ActiveWorkItems.Remove(sender);

                //check if result items already exist; if it does, skip it
                if (_resultManager.CheckIfResultExists(pParameters))
                    return;

                //create new
                AddNewWorkItem(sender.Parameters);
            };
            QueuedWorkItems.Add(newitem);
        }
    }
}
