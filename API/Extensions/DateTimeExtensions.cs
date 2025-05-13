namespace API.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - dateOfBirth.Year;

        if (new DateOnly(today.Year, dateOfBirth.Month, dateOfBirth.Day) > today)
        {
            age--;
        }

        return age;
    }
}