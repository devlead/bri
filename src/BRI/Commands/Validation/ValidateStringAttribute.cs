namespace BRI.Commands.Validation;

public class ValidateStringAttribute() : ParameterValidationAttribute(errorMessage: null!)
{
    public const int MinimumLength = 3;
    private static readonly (bool IsString, bool IsMinimumLength, string Value) InvalidStringValue = (false, false, string.Empty);

    public override ValidationResult Validate(CommandParameterContext context)
        => (
                context.Value is string stringValue
                    ? (
                        IsString: true,
                        IsMinimumLength: stringValue.Length >= MinimumLength,
                        Value: stringValue
                    )
                    : InvalidStringValue
            ) switch
            {
                {IsString: true, IsMinimumLength: true}
                    => ValidationResult.Success(),

                {IsString: true, IsMinimumLength: false} invalidValue
                    => ValidationResult.Error(
                        $"{context.Parameter.PropertyName} ({invalidValue.Value}) needs to be at least {MinimumLength} characters long was {invalidValue.Value.Length}."),

                _ => ValidationResult.Error(
                    $"Invalid {context.Parameter.PropertyName} ({context.Value ?? "<null>"}) specified.")
            };
}