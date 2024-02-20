using Microsoft.Extensions.Logging;

namespace Task8
{
	interface IHandler
	{
		TimeSpan Timeout { get; }

		Task PerformOperation(CancellationToken cancellationToken);
	}

	class Handler : IHandler
	{
		private readonly IConsumer _consumer;
		private readonly IPublisher _publisher;
		private readonly ILogger<Handler> _logger;

		public TimeSpan Timeout { get; }

		public Handler(TimeSpan timeout, IConsumer consumer, IPublisher publisher, ILogger<Handler> logger)
		{
			Timeout = timeout;

			_consumer = consumer;
			_publisher = publisher;
			_logger = logger;
		}

		public async Task PerformOperation(CancellationToken cancellationToken)
		{
			do
			{
				var data = await _consumer.ReadData();
				if (data == null)
					return;

				var payload = data.Payload;

				foreach (var recipient in data.Recipients)
				{
					var tr = Task.Run(async () =>
					{
						bool finished = false;

						while (!finished)
						{
							var res = await _publisher.SendData(recipient, payload);

							if (res == SendResult.Rejected)
								await Task.Delay(Timeout, cancellationToken);
							else
								finished = true;
						}
					});
				}

			} while (!cancellationToken.IsCancellationRequested);
		}
	}

	record Payload(string Origin, byte[] Data);
	record Address(string DataCenter, string NodeId);
	record Event(IReadOnlyCollection<Address> Recipients, Payload Payload);

	enum SendResult
	{
		Accepted,
		Rejected
	}

	interface IConsumer
	{
		Task<Event> ReadData();
	}

	interface IPublisher
	{
		Task<SendResult> SendData(Address address, Payload payload);
	}
}
