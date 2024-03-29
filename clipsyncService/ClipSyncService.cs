﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using clipsyncService.Models;
using UserAppsService;
using UserAppsService.Interfaces;
using UserAppsService.Models;

namespace clipsyncService
{
    public partial class ClipSyncService : ServiceBase
    {
        public List<string> gameProcesses = new List<string>()
        {
            "chrome", "firefox"
        };

        private UserProcess _userProcess;
        private IApp[] _currentApps;
        private UserApps _userApps;
        public ClipSyncService()
        {
            InitializeComponent();
            this.eventLog1 = new EventLog();
            if (!EventLog.SourceExists("ClipSyncSource"))
            {
                EventLog.CreateEventSource("ClipSyncSource", "ClipSyncLog");
            }

            eventLog1.Source = "ClipSyncSource";
            eventLog1.Log = "ClipSyncLog";
            _userProcess = new UserProcess();
            _userApps = new UserApps();
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry($"{Path.GetTempPath()}resyncDbContext.sqlite");
            if (!File.Exists($"{Path.GetTempPath()}resyncDbContext.sqlite"))
            {
                File.Create($"{Path.GetTempPath()}resyncDbContext.sqlite"); // make sure that this is in the installation folder, instead of the temp path
                // C:\WINDOWS\TEMP folder
            }
            eventLog1.WriteEntry("Service Started");
            Timer timer = new Timer
            {
                Interval = 2000
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Service Stopped");
        }

        public async void OnTimer (object sender, ElapsedEventArgs args) // refactored as of 22/5/2021
        {
            string output = "";
            _currentApps = _userProcess.GetRunningApps().ToArray();
            _userApps.SetApps(_currentApps);
            List<SelectedApp> apps = await _userProcess.GetSelectedApps();
            foreach (var _app in apps)
            {
                output += $"{_app.AppName} ";
            }

            eventLog1.WriteEntry(output);
            if (_userApps.CheckSync()) // kind of shit code because it relies on the SyncApps function being called in order for it to not constantly sync, but it works for now
            {                          // because the sync bool not being disabled on the CheckSync function
                List<IApp> syncedApps = _userApps.GetSyncQueuedApps();
                foreach (IApp app in syncedApps)
                {
                }
                await _userApps.SyncApps();
                eventLog1.WriteEntry(output);
            }
            else
            {
            }
        }
    }
}