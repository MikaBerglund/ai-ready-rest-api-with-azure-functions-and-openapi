namespace AIReadyRestApi.Models;

/// <summary>
/// Represents an investment calculation request.
/// </summary>
public class InvestmentRequest
{
    /// <summary>
    /// The monthly investment amount in currency units.
    /// </summary>
    public double MonthlyInvestment { get; set; }

    /// <summary>
    /// The number of months to invest.
    /// </summary>
    public int NumberOfMonths { get; set; }

    /// <summary>
    /// The expected annual average interest rate (e.g., 0.1 for 10%).
    /// </summary>
    public double AnnualInterestRate { get; set; }
}
