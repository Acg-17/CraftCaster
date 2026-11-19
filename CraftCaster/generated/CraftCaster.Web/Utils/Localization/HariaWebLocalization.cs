// HARIA-WEB GENERATED sha256:a8d8788258bc959095d42c4359914a125b48eb91fedaa0c27d9da3cfe35e46dc
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CraftCaster.Web.Utils.Localization;

public static class HariaWebLocalization
{
    public static IServiceCollection AddHariaWebLocalization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var section = configuration.GetSection("HariaWeb:Localization");
            var multilingual = section.GetValue<bool>("EnableCultureSelection");
            var names = multilingual ? section.GetSection("SupportedCultures").Get<string[]>() ?? ["es-ES"] : ["es-ES"];
            var cultures = names.Select(CultureInfo.GetCultureInfo).ToArray();
            var defaultCulture = multilingual ? section["DefaultCulture"] ?? "es-ES" : "es-ES";
            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;
            if (!multilingual) options.RequestCultureProviders.Clear();
            options.ApplyCurrentCultureToResponseHeaders = true;
        });
        services.Configure<MvcOptions>(options =>
        {
            options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, field) => $"El valor '{value}' no es válido para {field}.");
            options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(field => "Debes indicar un valor.");
            options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(value => $"El valor '{value}' no es válido.");
            options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(field => $"Debes indicar {field}.");
            options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(field => $"El campo {field} debe ser numérico.");
            options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(field => $"El valor de {field} no es válido.");
            if (!options.ModelBinderProviders.OfType<HariaFloatingPointModelBinderProvider>().Any())
                options.ModelBinderProviders.Insert(0, new HariaFloatingPointModelBinderProvider());
        });
        return services;
    }
}

// Keep query-string conventions invariant and use the form's request culture.
// Float excludes thousands separators: a decimal point in Spanish must not
// silently turn 12.50 into 1250. The browser uses the same decimal-only grammar.
public sealed class HariaFloatingPointModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.BindingInfo.BinderType is not null || context.BindingInfo.BindingSource?.IsGreedy == true) return null;
        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return type == typeof(double) || type == typeof(float) || type == typeof(decimal)
            ? new HariaFloatingPointModelBinder(type) : null;
    }
}

public sealed class HariaFloatingPointModelBinder(Type type) : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var result = context.ValueProvider.GetValue(context.ModelName);
        if (result == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName, result);
        var text = result.FirstValue;
        if (string.IsNullOrWhiteSpace(text) && context.ModelMetadata.IsReferenceOrNullableType)
        {
            context.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }
        object? value = null;
        if (type == typeof(double) && double.TryParse(text, NumberStyles.Float, result.Culture, out var number) && double.IsFinite(number)) value = number;
        else if (type == typeof(float) && float.TryParse(text, NumberStyles.Float, result.Culture, out var single) && float.IsFinite(single)) value = single;
        else if (type == typeof(decimal) && decimal.TryParse(text, NumberStyles.Float, result.Culture, out var money)) value = money;
        if (value is not null) context.Result = ModelBindingResult.Success(value);
        else
        {
            var label = context.ModelMetadata.GetDisplayName();
            var message = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es"
                ? $"El campo {label} debe ser un número válido con {(result.Culture.NumberFormat.NumberDecimalSeparator == "," ? "coma" : "punto")} decimal y sin separadores de miles."
                : $"The field {label} must be a valid number with a decimal point and no thousands separators.";
            context.ModelState.TryAddModelError(context.ModelName, message);
            context.Result = ModelBindingResult.Failed();
        }
        return Task.CompletedTask;
    }
}

// Uso explícito en filtros GET de la interfaz. No cambia rutas ni JSON.
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class HariaLocalizedNumberAttribute : ModelBinderAttribute
{
    public HariaLocalizedNumberAttribute() : base(typeof(HariaLocalizedNumberBinder)) { }
}

public sealed class HariaLocalizedNumberBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext context)
    {
        var previous = context.ValueProvider;
        context.ValueProvider = new LocalizedValueProvider(previous);
        try
        {
            var type = Nullable.GetUnderlyingType(context.ModelType) ?? context.ModelType;
            await new HariaFloatingPointModelBinder(type).BindModelAsync(context);
        }
        finally { context.ValueProvider = previous; }
    }

    private sealed class LocalizedValueProvider(IValueProvider original) : IValueProvider
    {
        public bool ContainsPrefix(string prefix) => original.ContainsPrefix(prefix);
        public ValueProviderResult GetValue(string key)
        {
            var result = original.GetValue(key);
            return result == ValueProviderResult.None ? result : new ValueProviderResult(result.Values, CultureInfo.CurrentCulture);
        }
    }
}
