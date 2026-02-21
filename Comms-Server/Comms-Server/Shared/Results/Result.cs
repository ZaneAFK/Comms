namespace Comms_Server.Shared.Results
{
	public class Result<T>
	{
		public bool Succeeded { get; }
		public T? Value { get; }
		public IEnumerable<string> Errors { get; }

		private Result(bool succeeded, T? value, IEnumerable<string>? errors)
		{
			Succeeded = succeeded;
			Value = value;
			Errors = errors ?? Array.Empty<string>();
		}

		public static Result<T> Success(T value)
			=> new Result<T>(true, value, null);

		public static Result<T> Failure(IEnumerable<string> errors)
			=> new Result<T>(false, default, errors);
	}

	public class Result
	{
		public bool Succeeded { get; }
		public IEnumerable<string> Errors { get; }

		private Result(bool succeeded, IEnumerable<string>? errors)
		{
			Succeeded = succeeded;
			Errors = errors ?? Array.Empty<string>();
		}

		public static Result Success()
			=> new Result(true, null);

		public static Result Failure(IEnumerable<string> errors)
			=> new Result(false, errors);
	}
}
