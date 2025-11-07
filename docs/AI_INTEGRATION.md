# Using the API with AI Systems

This document provides examples of how to integrate the AI-Ready REST API with various AI systems.

## ChatGPT and Custom GPTs

### Setup Instructions

1. **Start your local API**:
   ```bash
   dotnet run
   ```
   The API will start on `http://localhost:7093`

2. **Get the OpenAPI specification**:
   Navigate to `http://localhost:7093/api/openapi/v3.json` and copy the JSON content.

3. **Create a Custom GPT**:
   - Go to [ChatGPT](https://chat.openai.com/)
   - Click on your name > My GPTs > Create a GPT
   - In the Configure tab, scroll to Actions
   - Click "Create new action"
   - Paste your OpenAPI specification
   - Click "Save"

### Example Prompts

Once configured, you can use prompts like:

- "Get all products from the API"
- "Add a new product named 'Monitor' with price $299.99 in the Electronics category"
- "Search for products in the Electronics category"
- "Update product with ID 1 to have a price of $899.99"
- "Delete product with ID 3"

## Microsoft Copilot

Microsoft Copilot can use the OpenAPI specification to understand your API structure:

1. Share the OpenAPI specification URL with Copilot
2. Copilot can help generate code to consume the API
3. Copilot can suggest improvements based on the API structure

### Example with Copilot

```
User: "I have an API at http://localhost:7093/api. 
The OpenAPI spec is at http://localhost:7093/api/openapi/v3.json. 
Can you help me write C# code to get all products?"

Copilot will generate appropriate code based on the OpenAPI spec.
```

## Semantic Kernel

Semantic Kernel is Microsoft's SDK for integrating AI into applications. Here's how to use this API with Semantic Kernel:

### Installation

```bash
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Plugins.OpenApi
```

### C# Example

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;

// Create the kernel
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4",
    endpoint: "https://your-resource.openai.azure.com/",
    apiKey: "your-api-key"
);
var kernel = builder.Build();

// Import the API as a plugin
await kernel.ImportPluginFromOpenApiAsync(
    pluginName: "ProductsAPI",
    uri: new Uri("http://localhost:7093/api/openapi/v3.json")
);

// Now you can use the API functions
var result = await kernel.InvokeAsync("ProductsAPI", "GetProducts");
Console.WriteLine(result);

// Or use it with a prompt
var prompt = "Get all products and tell me which ones are in the Electronics category";
var response = await kernel.InvokePromptAsync(prompt);
Console.WriteLine(response);
```

### Advanced Semantic Kernel Usage

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4",
        endpoint: "https://your-resource.openai.azure.com/",
        apiKey: "your-api-key"
    )
    .Build();

// Import the plugin
await kernel.ImportPluginFromOpenApiAsync(
    pluginName: "Products",
    uri: new Uri("http://localhost:7093/api/openapi/v3.json")
);

// Enable auto function calling
var executionSettings = new OpenAIPromptExecutionSettings 
{ 
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions 
};

// Use natural language to interact with the API
var prompt = @"
I need to manage my product catalog. 
Please list all products, then add a new product called 'Webcam' 
with a description 'HD Webcam with microphone', price $79.99, 
in the Electronics category.
";

var result = await kernel.InvokePromptAsync(prompt, new(executionSettings));
Console.WriteLine(result);
```

## LangChain (Python)

LangChain is a popular framework for building LLM applications. Here's how to use this API:

### Installation

```bash
pip install langchain openai
```

### Python Example

```python
from langchain.agents import load_tools, initialize_agent, AgentType
from langchain.llms import OpenAI

# Initialize the LLM
llm = OpenAI(temperature=0)

# Load the OpenAPI tools
tools = load_tools(
    ["requests_all"],
    llm=llm,
    requests_wrapper_kwargs={
        "headers": {"Content-Type": "application/json"}
    }
)

# Create an agent
agent = initialize_agent(
    tools,
    llm,
    agent=AgentType.ZERO_SHOT_REACT_DESCRIPTION,
    verbose=True
)

# Use the agent
response = agent.run(
    "Get all products from http://localhost:7093/api/products"
)
print(response)
```

## AutoGen (Multi-Agent Framework)

AutoGen enables multi-agent conversations. Here's an example:

```python
import autogen

config_list = [
    {
        "model": "gpt-4",
        "api_key": "your-api-key"
    }
]

# Create an assistant agent
assistant = autogen.AssistantAgent(
    name="assistant",
    llm_config={"config_list": config_list}
)

# Create a user proxy agent
user_proxy = autogen.UserProxyAgent(
    name="user_proxy",
    human_input_mode="NEVER",
    max_consecutive_auto_reply=10,
    code_execution_config={"work_dir": "coding"}
)

# Start the conversation
user_proxy.initiate_chat(
    assistant,
    message="""
    Use the API at http://localhost:7093/api to:
    1. Get all products
    2. Find products in the Electronics category
    3. Add a new product named 'Speaker' for $149.99
    
    The OpenAPI spec is at http://localhost:7093/api/openapi/v3.json
    """
)
```

## Testing the API

### Using curl

```bash
# Get all products
curl http://localhost:7093/api/products

# Get a specific product
curl http://localhost:7093/api/products/1

# Create a new product
curl -X POST http://localhost:7093/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Monitor",
    "description": "27 inch 4K monitor",
    "price": 399.99,
    "category": "Electronics"
  }'

# Update a product
curl -X PUT http://localhost:7093/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "Updated high-performance laptop",
    "price": 1099.99,
    "category": "Electronics"
  }'

# Delete a product
curl -X DELETE http://localhost:7093/api/products/1

# Search by category
curl "http://localhost:7093/api/products/search?category=Electronics"
```

### Using PowerShell

```powershell
# Get all products
Invoke-RestMethod -Uri http://localhost:7093/api/products

# Create a new product
$body = @{
    name = "Tablet"
    description = "10 inch tablet"
    price = 299.99
    category = "Electronics"
} | ConvertTo-Json

Invoke-RestMethod -Uri http://localhost:7093/api/products `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

## Production Considerations

When deploying to production:

1. **Security**: Update `AuthorizationLevel` from `Anonymous` to `Function` or `Admin` in the function attributes
2. **CORS**: Configure CORS settings in `host.json` if accessing from web applications
3. **API Management**: Consider using Azure API Management for additional features like rate limiting, caching, and analytics
4. **Monitoring**: Enable Application Insights for monitoring and diagnostics
5. **Versioning**: Implement API versioning for backward compatibility

## Additional Resources

- [Azure Functions OpenAPI Extension Documentation](https://github.com/Azure/azure-functions-openapi-extension)
- [Semantic Kernel Documentation](https://learn.microsoft.com/semantic-kernel/)
- [OpenAPI Specification](https://swagger.io/specification/)
- [LangChain Documentation](https://python.langchain.com/)
- [AutoGen Documentation](https://microsoft.github.io/autogen/)
