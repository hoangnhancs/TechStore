using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SearchService.RequestHelpers
{
    public class ListIntModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();
            var result = new List<int>();

            if (!string.IsNullOrEmpty(value))
            {
                var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var v in values)
                {
                    if (int.TryParse(v, out var intVal))
                        result.Add(intVal);
                }
            }

            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }
    }
}