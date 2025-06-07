using System;
using System.Collections.Generic;
using System.Linq;

namespace GestMantIA.Application // Namespace ajustado a la raíz del proyecto Application
{
    public class Result
    {
        public bool Succeeded { get; protected init; }
        public string[] Errors { get; protected init; }

        protected Result(bool succeeded, IEnumerable<string>? errors)
        {
            Succeeded = succeeded;
            Errors = errors?.ToArray() ?? Array.Empty<string>();
        }

        public static Result Success()
        {
            return new Result(true, null);
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors);
        }

        public static Result Failure(string error)
        {
            return new Result(false, new[] { error });
        }
    }

    public class Result<T> : Result
    {
        public T? Data { get; private init; }

        private Result(bool succeeded, IEnumerable<string>? errors, T? data)
            : base(succeeded, errors)
        {
            Data = data;
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>(true, null, data);
        }

        public new static Result<T> Failure(IEnumerable<string> errors)
        {
            return new Result<T>(false, errors, default);
        }

        public new static Result<T> Failure(string error)
        {
            return new Result<T>(false, new[] { error }, default);
        }
    }
}
