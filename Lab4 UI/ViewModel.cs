using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Lab4_UI
{
	public class ViewModel : INotifyPropertyChanged
	{
		public List<Event> eventStack;
		private int workersCount  = 1;
		private int modellingTime  = 60;
		private int modellingProgress = 0;
		private int rps  = 1;
		private int queueSize  = 1;
		private int internationTime  = 1000;
		private int systemTime  = 0;
		private ICommand processModelling;
		private ICommand saveResults;

		public int ModellingProgress {
			get { return modellingProgress; }
			set {
				int temp = value / (modellingTime * 10);
				if(temp != modellingProgress)
				{
				modellingProgress = temp;
				OnPropertyChanged("ModellingProgress");
				}
			}
		}
		public ICommand SaveResults {
			get { return saveResults; }
			set { saveResults = value; }
		}
		public ICommand ProcessModelling{
			get { return processModelling; }
			set { processModelling = value;}
		}
		public int WorkersCount
		{
			get { return workersCount; }
			set
			{
				workersCount = value;
				OnPropertyChanged("WorkersCount");
			}
		}

		public int ModellingTime
		{
			get { return modellingTime; }
			set
			{
				modellingTime = value;
				OnPropertyChanged("ModellingTime");
			}
		}

		public int RPS
		{
			get { return rps; }
			set
			{
				rps = value;
				OnPropertyChanged("RPS");
			}
		}

		public int QueueSize
		{
			get { return queueSize; }
			set
			{
				queueSize = value;
				OnPropertyChanged("QueueSize");
			}
		}

		public int InternationTime
		{
			get { return internationTime; }
			set
			{
				internationTime = value;
				OnPropertyChanged("InternationTime");
			}
		}
		public ViewModel()
		{
			ProcessModelling = new DelegateCommand(() =>
			{
				systemTime = 0;
				ModellingProgress = systemTime;
				eventStack = new List<Event>(queueSize);
				EventProcessor SMO = new EventProcessor(workersCount, queueSize);
				while (systemTime < modellingTime * 1000)
				{
					if (systemTime % (1000 / RPS) == 0)
					{
						eventStack.Add(new Event { EventStatus = EventTypes.New, OccurrTime = systemTime, ProcesingTime = new Random().Next(300, 600) });
					}
					if (systemTime % internationTime == 0)
					{
						StatServant.CalcSlices(systemTime, eventStack.Count);
					}
					var currStack = eventStack.Where(x => x.OccurrTime <= systemTime);
					if (currStack.Count() > 0)
					{
						var startEvent = currStack.Min();
						var times = eventStack.Select(x => x.OccurrTime);
						var newEvent = SMO.ProcessRequest(startEvent);
						if (newEvent == null || newEvent.EventStatus == EventTypes.Processed)
						{
							eventStack.Remove(startEvent);
							systemTime++;
							ModellingProgress = systemTime;
							continue;
						}
						else
						{
							eventStack.Add(newEvent);
							eventStack.Remove(startEvent);
							systemTime++;
							ModellingProgress = systemTime;
						}
					}
					else
					{
						systemTime++;
						ModellingProgress = systemTime;
					}
				}

			});

			SaveResults = new DelegateCommand(() =>
			{
				List<string> file = new List<string>();
				var header = $"Время\t    Обработано\t    Потеряно\t      Всего\t ЗанятоКаналов\t Длина очереди\t {Environment.NewLine}";
				file.Add(header);
				foreach (var slice in ModellingResults.systemState)
				{
					var record = $"{slice.Time}\t \t{slice.Processed}\t \t{slice.Rejected}\t \t{slice.Total}\t \t{slice.BusyChannels}\t \t{slice.QueueSize}\t";
					file.Add(record);
				}
				Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
				dlg.FileName = "Document"; // Default file name
				dlg.DefaultExt = ".txt"; // Default file extension
				dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

				// Show save file dialog box
				Nullable<bool> result = dlg.ShowDialog();

				// Process save file dialog box results
				if (result == true)
				{
					// Save document
					File.WriteAllLines(dlg.FileName, file);
				}
				ModellingProgress = 0;
			});
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}
}
