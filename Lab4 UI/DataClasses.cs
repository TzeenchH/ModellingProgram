using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4_UI
{
	public static class ModellingResults
	{
		public static int EventsProcessed { get; set; }
		public static int EventsRejected { get; set; }
		public static int BusyChannels { get; set; }
		public static int QueueSize { get; set; }
		public static int TotalEvents { get; set; }

		public static List<MeasureSlice> systemState = new List<MeasureSlice>();
	}

	public class MeasureSlice
	{
		public double Time { get; set; }
		public int Processed { get; set; }
		public int Rejected { get; set; }
		public int BusyChannels { get; set; }
		public int QueueSize { get; set; }
		public int Total { get; set; }
		public int InSystem { get; set; }
		public MeasureSlice(double time, int processed, int rejected, int total, int busyChannels, int queueSize, int inSystem)
		{
			Time = time / 1000;
			Processed = processed;
			Rejected = rejected;
			Total = total;
			BusyChannels = busyChannels;
			QueueSize = queueSize;
			InSystem = inSystem;
		}
	}

	public class Event : IComparable<Event>
	{
		public EventTypes EventStatus { get; set; }
		public int OccurrTime { get; set; }
		public int ProcesingTime { get; set; }

		public int CompareTo([AllowNull] Event other)
		{
			return this.OccurrTime.CompareTo(other.OccurrTime);
		}
	}

	public enum EventTypes
	{
		New,
		InProcess,
		Processed
	}
}
