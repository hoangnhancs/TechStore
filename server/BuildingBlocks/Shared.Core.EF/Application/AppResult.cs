using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Core.EF.Application
{
    public class AppResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Value { get; set; }
        public string? Error { get; set; }
        public int Code { get; set; }
        public static AppResult<T> Success(T value) => new()
        {
            IsSuccess = true,
            Value = value
        };
        public static AppResult<T> Failure(string error, int code) => new()
        {
            IsSuccess = false,
            Error = error,
            Code = code,
        };
    }
}