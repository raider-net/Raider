﻿//using System;
//using System.Collections.Concurrent;
//using System.Threading;

//namespace Raider.Logging.SerilogEx.Sink
//{
//	internal class BoundedConcurrentQueue<T>
//	{
//		public const int Unbounded = -1;

//		readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
//		readonly int _queueLimit;

//		int _counter;

//		public BoundedConcurrentQueue(int? queueLimit = null)
//		{
//			if (queueLimit.HasValue && queueLimit <= 0)
//				throw new ArgumentOutOfRangeException(nameof(queueLimit), "Queue limit must be positive, or `null` to indicate unbounded.");

//			_queueLimit = queueLimit ?? Unbounded;
//		}

//		public int Count => _queue.Count;

//		public bool TryDequeue(out T? item)
//		{
//			if (_queueLimit == Unbounded)
//				return _queue.TryDequeue(out item);

//			var result = false;
//			try
//			{ }
//			finally // prevent state corrupt while aborting
//			{
//				if (_queue.TryDequeue(out item))
//				{
//					Interlocked.Decrement(ref _counter);
//					result = true;
//				}
//			}

//			return result;
//		}

//		public bool TryEnqueue(T item)
//		{
//			if (_queueLimit == Unbounded)
//			{
//				_queue.Enqueue(item);
//				return true;
//			}

//			var result = true;
//			try
//			{ }
//			finally
//			{
//				if (Interlocked.Increment(ref _counter) <= _queueLimit)
//				{
//					_queue.Enqueue(item);
//				}
//				else
//				{
//					Interlocked.Decrement(ref _counter);
//					result = false;
//				}
//			}

//			return result;
//		}
//	}
//}
