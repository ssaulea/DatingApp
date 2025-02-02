using System;

namespace API.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateOnly source) => (new DateTime(1, 1, 1) + (DateTime.Today - source.ToDateTime(new TimeOnly()))).Year - 1;
}
