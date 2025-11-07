namespace AIReadyRestApi.Models;

/// <summary>
/// Represents the result of an investment calculation.
/// </summary>
public class InvestmentResult
{
    /// <summary>
    /// The total amount invested (monthly investment × number of months).
    /// </summary>
    public double TotalInvested { get; set; }

    /// <summary>
    /// The total interest earned.
    /// </summary>
    public double TotalInterest { get; set; }

    /// <summary>
    /// The final value of the investment (total invested + total interest).
    /// </summary>
    public double FinalValue { get; set; }

    /// <summary>
    /// The monthly investment amount used in the calculation.
    /// </summary>
    public double MonthlyInvestment { get; set; }

    /// <summary>
    /// The number of months used in the calculation.
    /// </summary>
    public int NumberOfMonths { get; set; }

    /// <summary>
    /// The annual interest rate used in the calculation.
    /// </summary>
    public double AnnualInterestRate { get; set; }
}
