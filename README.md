# AI-Ready REST API with Azure Functions and OpenAPI

A sample application that demonstrates how to use Azure Functions (isolated worker model) with the OpenAPI extension to create a REST API that can easily be consumed by AI systems such as ChatGPT, Copilot, or Microsoft Agent Framework (MAF).

## Features

- **Azure Functions v4** with .NET 8.0 isolated worker model
- **OpenAPI/Swagger documentation** automatically generated from code attributes
- **RESTful API** with investment calculation endpoint
- **AI-ready** endpoints with comprehensive OpenAPI annotations
- **Visual Studio 2022** compatible

## Project Structure

```
AIReadyRestApi/
├── Functions/
│   └── InvestmentCalculatorFunctions.cs  # HTTP-triggered functions with OpenAPI attributes
├── Models/
│   ├── InvestmentRequest.cs              # Investment calculation request model
│   └── InvestmentResult.cs               # Investment calculation result model
├── Program.cs                            # Host configuration
├── host.json                             # Functions runtime configuration
└── local.settings.json                   # Local development settings
```

## API Endpoints

The application provides the following endpoint:

- `POST /api/investment/calculate` - Calculate investment returns

### Investment Calculation Endpoint

This endpoint calculates the future value of regular monthly investments with compound interest.

**Request Body:**
```json
{
  "monthlyInvestment": 100.0,
  "numberOfMonths": 12,
  "annualInterestRate": 0.10
}
```

**Response:**
```json
{
  "monthlyInvestment": 100.0,
  "numberOfMonths": 12,
  "annualInterestRate": 0.10,
  "totalInvested": 1200.0,
  "totalInterest": 67.03,
  "finalValue": 1267.03
}
```

The calculation assumes:
- Investments are made at the beginning of each month
- Interest is compounded monthly
- Annual interest rate is expressed as a decimal (e.g., 0.1 for 10%)

## OpenAPI/Swagger Documentation

The OpenAPI documentation is automatically available at:

- Swagger UI: `http://localhost:7093/api/swagger/ui`
- OpenAPI JSON: `http://localhost:7093/api/openapi/v3.json`
- OpenAPI YAML: `http://localhost:7093/api/openapi/v3.yaml`

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) (optional, for local testing)

### Build and Run

1. Clone the repository:
   ```bash
   git clone https://github.com/MikaBerglund/ai-ready-rest-api-with-azure-functions-and-openapi.git
   cd ai-ready-rest-api-with-azure-functions-and-openapi
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run
   ```
   
   Or use Azure Functions Core Tools:
   ```bash
   func start
   ```

4. Open the Swagger UI in your browser:
   ```
   http://localhost:7093/api/swagger/ui
   ```

## Using with AI Systems

The OpenAPI specification makes this API easy to integrate with AI systems:

### ChatGPT / Custom GPTs

1. Navigate to `http://localhost:7093/api/openapi/v3.json`
2. Copy the OpenAPI specification
3. In ChatGPT, create a Custom GPT and provide the OpenAPI spec in the Actions section

### Microsoft Copilot

The OpenAPI specification allows Copilot to understand the API structure and generate appropriate requests.

### Semantic Kernel

```csharp
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey)
    .Build();

// Import the API as a plugin
await kernel.ImportPluginFromOpenApiAsync(
    "InvestmentAPI",
    new Uri("http://localhost:7093/api/openapi/v3.json"));
```

## Development

### Adding New Functions

1. Create a new class in the `Functions` folder
2. Add the `[Function]` attribute to your methods
3. Use OpenAPI attributes to document your endpoints:
   - `[OpenApiOperation]` - Describe the operation
   - `[OpenApiParameter]` - Describe parameters
   - `[OpenApiRequestBody]` - Describe request body
   - `[OpenApiResponseWithBody]` - Describe response with body
   - `[OpenApiResponseWithoutBody]` - Describe response without body

### Key OpenAPI Attributes

```csharp
[Function("MyFunction")]
[OpenApiOperation(operationId: "MyOperation", tags: new[] { "MyTag" })]
[OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(MyModel), Required = true)]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MyModel))]
public async Task<HttpResponseData> MyFunction(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "myroute/{id}")] HttpRequestData req,
    string id)
{
    // Implementation
}
```

## Deployment

### Azure

1. Create an Azure Function App (Windows or Linux)
2. Ensure it's configured for .NET 8.0 isolated
3. Deploy using Visual Studio, VS Code, Azure CLI, or GitHub Actions

### GitHub Actions

A sample GitHub Actions workflow for deployment:

```yaml
name: Deploy to Azure Functions

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Publish
      run: dotnet publish --configuration Release --output ./output
    
    - name: Deploy to Azure Functions
      uses: Azure/functions-action@v1
      with:
        app-name: your-function-app-name
        package: ./output
        publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}
```

## Technologies Used

- **Azure Functions** - Serverless compute platform
- **OpenAPI/Swagger** - API documentation standard
- **.NET 8.0** - Modern, cross-platform framework
- **Isolated Worker Model** - Enhanced performance and flexibility

## License

This project is provided as a sample/template for educational purposes.

## Resources

- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [OpenAPI Specification](https://swagger.io/specification/)
- [Azure Functions OpenAPI Extension](https://github.com/Azure/azure-functions-openapi-extension)

