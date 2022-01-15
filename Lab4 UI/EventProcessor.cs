using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4_UI
{
	public class EventProcessor
	{
		int WorkersCount { get; set; }
		int BusyWorkers { get; set; } = 0;
		int QueueLength { get; set; }
		Queue<Event> events { get; set; } = new Queue<Event>();

		public EventProcessor(int wc, int ql)
		{
			ModellingResults.EventsProcessed = 0;
			ModellingResults.EventsRejected = 0;
			WorkersCount = wc;
			QueueLength = ql;
		}
		public Event ProcessRequest(Event e)
		{
			switch (e.EventStatus)
			{
				case EventTypes.New:
					var ev = ProcessNew(e);
					return ev;
				case EventTypes.InProcess:
					var evt = ProcessInWork(e);
					return evt;
				default:
					return null;
			}
		}

		public Event ProcessNew(Event e)
		{
			if (BusyWorkers < WorkersCount)
			{
				if (events.Count == 0)
				{
					BusyWorkers++;
					ModellingResults.BusyChannels = BusyWorkers;
					return new Event { EventStatus = EventTypes.InProcess, OccurrTime = e.OccurrTime + e.ProcesingTime };
				}
				else
				{
					var evt = events.Dequeue();
					BusyWorkers++;
					events.Enqueue(e);
					return new Event { EventStatus = EventTypes.InProcess, OccurrTime = evt.OccurrTime + evt.ProcesingTime };
				}
			}
			else if (QueueLength > events.Count)
			{
				events.Enqueue(e);
				ModellingResults.BusyChannels = BusyWorkers;
				ModellingResults.QueueSize = events.Count;
				return new Event { EventStatus = EventTypes.InProcess, OccurrTime = e.OccurrTime + e.ProcesingTime };
			}
			else
			{
				ModellingResults.BusyChannels = BusyWorkers;
				ModellingResults.QueueSize = QueueLength;
				ModellingResults.EventsRejected++;
				return null;
			}
		}

		public Event ProcessInWork(Event e)
		{
			BusyWorkers--;
			ModellingResults.EventsProcessed++;
			if (events.Count > 0)
			{
				ProcessNew(events.Dequeue());
			}
			return new Event { EventStatus = EventTypes.Processed, OccurrTime = e.OccurrTime, };
		}
	}

	public static class StatServant
	{
		public static void CalcSlices(int internationTime, int stackSize)
		{
			MeasureSlice slice = new MeasureSlice(
			(double)internationTime,
			ModellingResults.EventsProcessed,
			ModellingResults.EventsRejected,
			ModellingResults.EventsProcessed + ModellingResults.EventsRejected,
			ModellingResults.BusyChannels,
			ModellingResults.QueueSize,
			stackSize
			);
			ModellingResults.systemState.Add(slice);
		}

		public static void WriteToFile()
		{
			List<string> file = new List<string>();
			var header = $"Время\t    Обработано\t    Потеряно\t      Всего\t ЗанятоКаналов\t Длина очереди\t {Environment.NewLine}";
			file.Add(header);
			foreach (var slice in ModellingResults.systemState)
			{
				var record = $"{slice.Time}\t \t{slice.Processed}\t \t{slice.Rejected}\t \t{slice.Total}\t \t{slice.BusyChannels}\t \t{slice.QueueSize}\t";
				file.Add(record);
			}
			File.WriteAllLines(@"C:\Users\Acer\Desktop\ТМО\Measures.txt", file);
		}
	}
}
