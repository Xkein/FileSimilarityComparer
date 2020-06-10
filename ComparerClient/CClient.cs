using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using ComparerCore;

namespace ComparerClient
{
    class CClient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        CDisplayer displayer;

        public CClient()
        {
            SuspectedSim = 0.80;
            IgnoreRedundancy = false;
            IgnoreCommend = false;
            CompareState = "Idle.";
            //displayer = new CDisplayer();
            CompareInfos = new ObservableCollection<CInfo>();
            CompareCaseCount = 0;
            CompareTime = 0;
            IsNotComparing = true;
            FullPowerCompare = false;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000.0);
            timer.Tick += Timer_Tick;
            stopwatch = new Stopwatch();
        }


        private double suspectedSim;
        public double SuspectedSim
        {
            get => suspectedSim;
            set
            {
                suspectedSim = value;
                OnPropertyChanged(nameof(SuspectedSim));
            }
        }

        private bool ignoreRedundancy;
        public bool IgnoreRedundancy
        {
            get => ignoreRedundancy;
            set
            {
                ignoreRedundancy = value;
                OnPropertyChanged(nameof(IgnoreRedundancy));
            }
        }

        private bool ignoreCommend;
        public bool IgnoreCommend
        {
            get => ignoreCommend;
            set
            {
                ignoreCommend = value;
                OnPropertyChanged(nameof(IgnoreCommend));
            }
        }

        private string compareFolder;
        public string CompareFolder
        {
            get => compareFolder;
            set
            {
                compareFolder = value;
                OnPropertyChanged(nameof(CompareFolder));
            }
        }

        private string compareState;
        public string CompareState
        {
            get => compareState;
            set
            {
                compareState = value;
                OnPropertyChanged(nameof(CompareState));
            }
        }

        private bool isNotComparing;
        public bool IsNotComparing
        {
            get => isNotComparing;
            set
            {
                isNotComparing = value;
                OnPropertyChanged(nameof(IsNotComparing));
            }
        }

        private bool fullPowerCompare;
        public bool FullPowerCompare
        {
            get => fullPowerCompare;
            set
            {
                fullPowerCompare = value;
                OnPropertyChanged(nameof(FullPowerCompare));
            }
        }


        private int compareTime;
        public int CompareTime
        {
            get => compareTime;
            set
            {
                compareTime = value;
                OnPropertyChanged(nameof(CompareTime));
            }
        }

        private ObservableCollection<CInfo> compareInfos;
        public ObservableCollection<CInfo> CompareInfos
        {
            get => compareInfos;
            set
            {
                compareInfos = value;
                OnPropertyChanged(nameof(CompareInfos));
            }
        }
        void AddInfo(CInfo info)
        {
            compareInfos.Add(info);
            //OnPropertyChanged(nameof(CompareInfos));
        }

        void ClearInfo()
        {
            compareInfos.Clear();
            OnPropertyChanged(nameof(CompareInfos));
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool SelectFolder()
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result == DialogResult.Cancel)
            {
                return false;
            }

            CompareFolder = dialog.SelectedPath;
            return true;
        }

        DispatcherTimer timer;
        Stopwatch stopwatch;
        private void Timer_Tick(object sender, EventArgs e)
        {
            CompareTime = (int)stopwatch.Elapsed.TotalSeconds;
        }

        readonly object locker = new object();
        int compareThread = 0;
        public int CompareThread
        {
            get => compareThread;
            set
            {
                compareThread = value;
                OnPropertyChanged(nameof(CompareThread));
            }
        }

        private int compareCaseCount;
        public int CompareCaseCount
        {
            get => compareCaseCount;
            set
            {
                compareCaseCount = value;
                OnPropertyChanged(nameof(CompareCaseCount));
            }
        }

        CancellationTokenSource cancelTokenSource;

        public void StartCompare()
        {
            CompareState = "Starting...";
            ClearInfo();

            if (Directory.Exists(CompareFolder))
            {
                Task.Factory.StartNew(() =>
                {
                    CompareInit();

                    // find files to compare
                    var dir = new DirectoryInfo(CompareFolder);
                    var list = dir.GetFiles().ToList();
                    List<FileInfo> files = new List<FileInfo>();
                    foreach (var file in list)
                    {
                        switch (file.Extension)
                        {
                            case ".cpp":
                            case ".h":
                                files.Add(file);
                                break;
                            default:
                                break;
                        }
                    }

                    // calculate case count
                    CompareCaseCount = 0;
                    for (int i = 0; i < files.Count - 1; i++)
                    {
                        CompareCaseCount += files.Count - (i + 1);
                    }

                    CompareThread = 0;

                    List<Task> tasks = new List<Task>();
                    cancelTokenSource = new CancellationTokenSource();
                    CancellationToken token = cancelTokenSource.Token;
                    var factory = new TaskFactory(token);

                    for (int i = 0; i < files.Count - 1; i++)
                    {
                        // avoid too long progress
                        // 
                        var compareTask = factory.StartNew((object idx) =>
                        {
                            int idx1 = (int)idx;
                            // wait more thread start
                            Thread.Sleep(100);

                            //Console.WriteLine(file1Idx);
                            //Thread.Sleep(100000);
                            Stopwatch watch = new Stopwatch();
                            watch.Start();

                            bool afterWaiting = false;
                            bool multiCompare = false;

                            Action<object> Compare = (object arg) =>
                            {
                                Thread.Sleep(10);
                                lock (locker)
                                {
                                    CompareThread++;
                                }
                                var pair = arg as Tuple<int, int>;
                                (int file1Idx, int file2Idx) = (pair.Item1, pair.Item2);

                                CompareState = "Start Comparing " + files[file1Idx].Name + " to " + files[file2Idx].Name;

                                var tuple = CCore.CompareFile(files[file1Idx].FullName, files[file2Idx].FullName);

                                if (tuple.Item1 >= CCore.suspectedSim)
                                {
                                    System.Windows.Application.Current.Dispatcher.Invoke(
                                        () => AddInfo(new CInfo(files[file1Idx].Name, files[file2Idx].Name, tuple.Item1)
                                        ));
                                }
                                else if (tuple.Item2 >= CCore.suspectedSim)
                                {
                                    System.Windows.Application.Current.Dispatcher.Invoke(
                                        () => AddInfo(new CInfo(files[file2Idx].Name, files[file1Idx].Name, tuple.Item2)
                                        ));
                                }

                                lock (locker)
                                {
                                    CompareCaseCount--;
                                    CompareThread--;
                                }
                            };

                            // start compare
                            for (int j = idx1 + 1; j < files.Count; j++)
                            {
                                var curPair = new Tuple<int, int>(idx1, j);
                                if (multiCompare)
                                {
                                    var maxThreadCount = Environment.ProcessorCount * 2;
                                    if(CompareThread < maxThreadCount)
                                    {
                                        //var task = Task.Factory.StartNew(Compare, curPair, FullPowerCompare ? TaskCreationOptions.LongRunning : TaskCreationOptions.None);
                                        factory.StartNew(Compare, curPair);
                                        Thread.Sleep(CompareThread > maxThreadCount ? CompareThread * 50 : 250);
                                    }
                                    else
                                    {
                                        Compare(curPair);
                                    }
                                }
                                else if (afterWaiting)
                                {
                                    watch.Restart();
                                    Compare(curPair);
                                    watch.Stop();

                                    // use multi-compare if file size > 1024*4 kb && a compare progress > 50*thread seconds
                                    if (files[idx1].Length >= 1024 * 4 && watch.ElapsedMilliseconds >= 50 * CompareThread)
                                    {
                                        //Console.WriteLine("{0} Use Multi-Compare({1}).", files[idx1].Name, watch.Elapsed.TotalSeconds);
                                        multiCompare = true;
                                    }
                                }
                                else
                                {
                                    Compare(curPair);

                                    if (watch.Elapsed.TotalSeconds >= 30.0)
                                    {
                                        afterWaiting = true;
                                    }
                                }

                                if (cancelTokenSource.IsCancellationRequested)
                                {
                                    return;
                                }
                            }

                            CompareState = files[idx1].Name + " exit.";
                        }, i, FullPowerCompare ? TaskCreationOptions.LongRunning : TaskCreationOptions.None);

                        tasks.Add(compareTask);
                        Thread.Sleep(1);
                    }

                    try
                    { // wait all thread exit or cancel them
                        Task.WaitAll(tasks.ToArray(), token);
                    }
                    catch (OperationCanceledException)
                    {
                        CompareState = "Stopping...";
                    }

                    while (CompareThread > 0)
                    { // wait all thread exit
                        Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    }

                    cancelTokenSource.Dispose();
                    CompareFinish();
                    // Create new thread for Compare Manager
                }, TaskCreationOptions.LongRunning);
            }
            else
            {
                CompareState = "Folder " + CompareFolder + " not exist.";
            }
        }

        private void CompareFinish()
        {
            CompareState = "Finished.";
            timer.Stop();
            stopwatch.Stop();
            IsNotComparing = true;
        }

        private void CompareInit()
        {
            IsNotComparing = false;
            CompareTime = 0;
            timer.Start();
            stopwatch.Restart();

            CCore.ignoreCommend = IgnoreCommend;
            CCore.ignoreRedundancy = IgnoreRedundancy;
            CCore.suspectedSim = SuspectedSim;
        }

        public void StopCompare()
        {
            cancelTokenSource?.Cancel();
        }
    }
}
