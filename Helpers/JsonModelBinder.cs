using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

public class JsonModelBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		if (bindingContext == null)
		{
			throw new ArgumentNullException(nameof(bindingContext));
		}

		var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
		if (string.IsNullOrEmpty(value))
		{
			bindingContext.ModelState.AddModelError(bindingContext.ModelName, "The input value is null or empty.");
			return Task.CompletedTask;
		}

		try
		{
			var result = JsonConvert.DeserializeObject(value, bindingContext.ModelType);
			bindingContext.Result = ModelBindingResult.Success(result);
		}
		catch (JsonException ex)
		{
			bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid JSON: {ex.Message}");
		}

		return Task.CompletedTask;
	}
}
