namespace Conveyor
{
	using System;
	using System.Threading;

	/// <summary>
	/// Conveyor class. Allowed to queue values of type T and dequeue values of type V only after the item was processed
	/// preserving the order of enqueued items. Allow to push, pop and process items in different threads.
	/// T - type of values to Push in.
	/// V - type of values to Pop out.
	/// </summary>
	public sealed class Conveyor<T, V>
	{
		#region Private Fields

		/// <summary>
		/// The locker to synchronize the threads.
		/// </summary>
		private object _processingLocker = new object ();

		/// <summary>
		/// The index of the last taken <see cref="Conveyor.ConveyorItem"/> for processing.
		/// </summary>
		private long _lastTakenForProcessingIndex;

		/// <summary>
		/// The index of the last processed <see cref="Conveyor.ConveyorItem"/>.
		/// </summary>
		private long _lastProcessedIndex;

		/// <summary>
		/// The index of the last added <see cref="Conveyor.ConveyorItem"/>.
		/// </summary>
		private long _lastAddedIndex;

		/// <summary>
		/// The first not poped item in <see cref="Conveyor.Conveyor"/> queue.
		/// </summary>
		private volatile ConveyorItem<T, V> _first;

		/// <summary>
		/// The last item in <see cref="Conveyor.Conveyor"/> queue.
		/// </summary>
		private volatile ConveyorItem<T, V> _last;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the total amount of conveyed items.
		/// </summary>
		/// <value>The total amount if items being pushed.</value>
		public long TotalConveyedItems {
			get {
				return _lastAddedIndex;
			}
		}

		/// <summary>
		/// Gets the total amount of processed items.
		/// </summary>
		/// <value>The total amount of items being processes.</value>
		public long TotalProcessedItems {
			get {
				return _lastProcessedIndex;
			}
		}

		/// <summary>
		/// Gets the total items amount which are pending to be processed before pop them out.
		/// </summary>
		/// <value>The total amount of items to process.</value>
		public long TotalItemsToProcess
		{
			get{
				return _lastAddedIndex - _lastProcessedIndex;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Conveyor.Conveyor"/> class.
		/// </summary>
		public Conveyor ()
		{
			_lastAddedIndex = _lastProcessedIndex = _lastTakenForProcessingIndex = 0;
		}

		#endregion

		#region Public Interface

		/// <summary>
		/// Push the specified data item to the <see cref="Conveyor.Conveyor"/> queue.
		/// </summary>
		/// <param name="data">Data item to store in <see cref="Conveyor.Conveyor"/>.</param>
		public void Push(T data)
		{
			lock (_processingLocker) {
				ConveyorItem<T, V> item = new ConveyorItem<T, V> (data, _lastAddedIndex++);
				if (_first == null) {
					_first = item;
					_last = item;
					return;
				}

				_last.Next = item;
				_last = _last.Next;
			}
		}

		/// <summary>
		/// Process the next available item with a specified processor and mark the item as Processed to allow to pop it out.
		/// </summary>
		/// <param name="processor">Processor to process the data.</param>
		/// <returns><c>true</c> if if item was processed; otherwise, <c>false</c>.</returns>
		/// <remarks>Throws an <see cref="System.ArgumentException"/> if processor was not specified.</remarks>
		public bool Process(Func<T, V> processor)
		{
			if (_first == null)
				return false;

			if (processor == null)
				throw new ArgumentException ("Processor should be specified to process conveyed item.");

			ConveyorItem<T, V> item = null;

			lock (_processingLocker) {
				if (_first == null)
					return false;

				item = _first [_lastTakenForProcessingIndex];
				if (item == null)
					return false;

				_lastTakenForProcessingIndex++;
			}

			item.ProcessedValue = processor (item.Data);
			Interlocked.Increment(ref _lastProcessedIndex);

			return true;
		}

		/// <summary>
		/// Pop the next available processed item.
		/// </summary>
		/// <param name="timeout">Amount of time (in msecs) after that the default(V) will be returned.</param>
		/// <returns>Processed value of type V or <c>default(V)</c> if timeout exeeded and there is no processed 
		/// item available in <see cref="Conveyor.Conveyor"/>.</returns>
		public V Pop(int timeout)
		{
			int waitingTime = 0;

			while (_first == null || !_first.IsProcessed) {
				if (timeout != Timeout.Infinite && waitingTime == timeout)
					return default(V);

				waitingTime++;
				Thread.Sleep (1);
			}

			lock (_processingLocker) {
				if (_first == null)
					return default(V);

				var item = _first;
				_first = _first.Next;
				return item.ProcessedValue;
			}
		}

		/// <summary>
		/// Clear the <see cref="Conveyor.Conveyor"/> queue.
		/// </summary>
		public void Clear()
		{
			lock (_processingLocker) {
				_first = null;
				_last = null;
			}
		}

		#endregion
	}

	/// <summary>
	/// Conveyor item class. Container used to store data internally in <see cref="Conveyor.ConveyorItem"/>.
	/// </summary>
	internal sealed class ConveyorItem<T, V>
	{
		#region Public Properties

		/// <summary>
		/// Gets the data item stored in container.
		/// </summary>
		internal T Data {
			get;
			private set;
		}

		/// <summary>
		/// Gets the index if conveyed item.
		/// </summary>
		internal long Index {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the result of data processing.
		/// </summary>
		/// <value>The processed value.</value>
		internal V ProcessedValue{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the followed item in  <see cref="Conveyor.ConveyorItem"/>.
		/// </summary>
		internal ConveyorItem<T, V> Next {
			get;
			set;
		}

		/// <summary>
		/// Gets a flag indicating whether this data is processed.
		/// </summary>
		/// <value><c>true</c> if this data is processed; otherwise, <c>false</c>.</value>
		internal bool IsProcessed {
			get{ return ProcessedValue != null; }
		}

		/// <summary>
		/// Gets one of the following <see cref="Conveyor.ConveyorItem"/> with the specified idx.
		/// Returns <c>null</c> if there is no following item with the specified index.
		/// </summary>
		/// <param name="idx">Index of the following item.</param>
		internal ConveyorItem<T, V> this [long idx] {
			get {
				var item = this;
				while (item != null)
					if (item.Index == idx)
						return item;
					else
						item = item.Next;

				return null;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Conveyor.ConveyorItem"/> class.
		/// </summary>
		/// <param name="data">Data to storee in <see cref="Conveyor.ConveyorItem"/>.</param>
		/// <param name="index">Index of the item in a queue.</param>
		internal ConveyorItem(T data, long index)
		{
			Data = data;
			Index = index;
		}

		#endregion
	}
}

