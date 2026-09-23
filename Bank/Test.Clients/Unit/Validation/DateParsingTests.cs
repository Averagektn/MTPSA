using Bank.Common.Validation;

namespace Tests.Clients.Unit.Validation;

[TestClass]
public sealed class DateParsingTests
{
    [TestMethod]
    [DataRow("15.05.2003")]
    [DataRow("2003-05-15")]
    [DataRow("1.2.2000")]
    public void Parse_accepts_supported_formats(string input)
    {
        var result = DateParsing.Parse(input);
        result.IsSuccess.Should().BeTrue(result.Errors.FirstOrDefault()?.Message ?? "");
    }

    [TestMethod]
    [DataRow("not-a-date")]
    [DataRow("31.02.2015")]
    [DataRow("29.02.2015")]
    [DataRow("2015-02-29")]
    [DataRow("")]
    public void Parse_rejects_invalid_dates(string input)
    {
        DateParsing.Parse(input).IsFailed.Should().BeTrue();
    }
}
