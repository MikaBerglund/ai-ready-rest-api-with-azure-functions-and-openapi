using System.Net;
using AIReadyRestApi.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace AIReadyRestApi.Functions;

/// <summary>
/// Azure Functions for calculating investment returns with compound interest.
/// </summary>
public class InvestmentCalculatorFunctions
{
    /// <summary>
    /// Calculate the future value of monthly investments with compound interest.
    /// This endpoint calculates how much you will have if you invest a fixed amount every month
    /// for a specified number of months at a given annual interest rate.
    /// </summary>
    [Function(nameof(CalculateInvestment))]
    [OpenApiOperation(
        operationId: "CalculateInvestment",
        tags: new[] { "Investment" },
        Summary = "Calculate investment returns",
        Description = "Calculates the future value of regular monthly investments with compound interest. " +
                      "The calculation assumes investments are made at the beginning of each month and " +
                      "interest is compounded monthly.")]
    [OpenApiRequestBody(
        contentType: "application/json",
        bodyType: typeof(InvestmentRequest),
        Required = true,
        Description = "Investment parameters including monthly investment amount, number of months, and annual interest rate")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(InvestmentResult),
        Description = "Investment calculation results including final value, total invested, and total interest earned")]
    [OpenApiResponseWithoutBody(
        statusCode: HttpStatusCode.BadRequest,
        Description = "Invalid request parameters (e.g., negative values or zero months)")]
    public async Task<HttpResponseData> CalculateInvestment(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "investment/calculate")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var request = await req.ReadFromJsonAsync<InvestmentRequest>();
        
        // Validate input
        if (request == null || 
            request.MonthlyInvestment <= 0 || 
            request.NumberOfMonths <= 0 || 
            request.AnnualInterestRate < 0)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        // Calculate monthly interest rate from annual rate
        double monthlyRate = request.AnnualInterestRate / 12;

        // Calculate future value using the formula for future value of an ordinary annuity
        // Since we're investing at the beginning of each month, we use annuity due formula
        // FV = PMT × [((1 + r)^n - 1) / r] × (1 + r)
        // Where: PMT = monthly payment, r = monthly interest rate, n = number of months
        
        double futureValue;
        if (monthlyRate == 0)
        {
            // If interest rate is 0, future value is simply sum of investments
            futureValue = request.MonthlyInvestment * request.NumberOfMonths;
        }
        else
        {
            // Future value of annuity due (payments at beginning of period)
            futureValue = request.MonthlyInvestment * 
                         (((Math.Pow(1 + monthlyRate, request.NumberOfMonths) - 1) / monthlyRate) * 
                          (1 + monthlyRate));
        }

        double totalInvested = request.MonthlyInvestment * request.NumberOfMonths;
        double totalInterest = futureValue - totalInvested;

        var result = new InvestmentResult
        {
            MonthlyInvestment = request.MonthlyInvestment,
            NumberOfMonths = request.NumberOfMonths,
            AnnualInterestRate = request.AnnualInterestRate,
            TotalInvested = totalInvested,
            TotalInterest = totalInterest,
            FinalValue = futureValue
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }
}
