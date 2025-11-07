using System.Net;
using AIReadyRestApi.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace AIReadyRestApi.Functions;

/// <summary>
/// Azure Functions for managing products with OpenAPI documentation.
/// </summary>
public class ProductFunctions
{
    // In-memory storage for demonstration purposes
    private static readonly List<Product> Products = new()
    {
        new Product { Id = "1", Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Category = "Electronics" },
        new Product { Id = "2", Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, Category = "Electronics" },
        new Product { Id = "3", Name = "Keyboard", Description = "Mechanical keyboard", Price = 79.99m, Category = "Electronics" }
    };

    /// <summary>
    /// Get all products.
    /// </summary>
    [Function(nameof(GetProducts))]
    [OpenApiOperation(operationId: "GetProducts", tags: new[] { "Products" }, Summary = "Get all products", Description = "Retrieves a list of all available products.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Product>), Description = "List of products")]
    public async Task<HttpResponseData> GetProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(Products);
        return response;
    }

    /// <summary>
    /// Get a product by ID.
    /// </summary>
    [Function(nameof(GetProductById))]
    [OpenApiOperation(operationId: "GetProductById", tags: new[] { "Products" }, Summary = "Get product by ID", Description = "Retrieves a specific product by its unique identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The product ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Product), Description = "The requested product")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Product not found")]
    public async Task<HttpResponseData> GetProductById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id}")] HttpRequestData req,
        string id,
        FunctionContext executionContext)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        
        if (product == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(product);
        return response;
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    [Function(nameof(CreateProduct))]
    [OpenApiOperation(operationId: "CreateProduct", tags: new[] { "Products" }, Summary = "Create a new product", Description = "Creates a new product in the catalog.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Product), Required = true, Description = "The product to create")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(Product), Description = "The created product")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Invalid product data")]
    public async Task<HttpResponseData> CreateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var product = await req.ReadFromJsonAsync<Product>();
        
        if (product == null || string.IsNullOrEmpty(product.Name))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        // Generate a new ID if not provided
        if (string.IsNullOrEmpty(product.Id))
        {
            product.Id = Guid.NewGuid().ToString();
        }

        Products.Add(product);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(product);
        return response;
    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    [Function(nameof(UpdateProduct))]
    [OpenApiOperation(operationId: "UpdateProduct", tags: new[] { "Products" }, Summary = "Update a product", Description = "Updates an existing product by its ID.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The product ID")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Product), Required = true, Description = "The updated product data")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Product), Description = "The updated product")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Product not found")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Invalid product data")]
    public async Task<HttpResponseData> UpdateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{id}")] HttpRequestData req,
        string id,
        FunctionContext executionContext)
    {
        var existingProduct = Products.FirstOrDefault(p => p.Id == id);
        
        if (existingProduct == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var updatedProduct = await req.ReadFromJsonAsync<Product>();
        
        if (updatedProduct == null || string.IsNullOrEmpty(updatedProduct.Name))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        // Update the product
        existingProduct.Name = updatedProduct.Name;
        existingProduct.Description = updatedProduct.Description;
        existingProduct.Price = updatedProduct.Price;
        existingProduct.Category = updatedProduct.Category;

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(existingProduct);
        return response;
    }

    /// <summary>
    /// Partially update a product.
    /// </summary>
    [Function(nameof(PatchProduct))]
    [OpenApiOperation(operationId: "PatchProduct", tags: new[] { "Products" }, Summary = "Partially update a product", Description = "Partially updates an existing product by its ID. Only provided fields will be updated.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The product ID")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ProductPatch), Required = true, Description = "The partial product data to update")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Product), Description = "The updated product")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Product not found")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Invalid product data")]
    public async Task<HttpResponseData> PatchProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "products/{id}")] HttpRequestData req,
        string id,
        FunctionContext executionContext)
    {
        var existingProduct = Products.FirstOrDefault(p => p.Id == id);
        
        if (existingProduct == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var patch = await req.ReadFromJsonAsync<ProductPatch>();
        
        if (patch == null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        // Apply partial updates only for properties that are provided
        if (patch.Name != null)
        {
            existingProduct.Name = patch.Name;
        }
        
        if (patch.Description != null)
        {
            existingProduct.Description = patch.Description;
        }
        
        if (patch.Price.HasValue)
        {
            existingProduct.Price = patch.Price.Value;
        }
        
        if (patch.Category != null)
        {
            existingProduct.Category = patch.Category;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(existingProduct);
        return response;
    }

    /// <summary>
    /// Delete a product.
    /// </summary>
    [Function(nameof(DeleteProduct))]
    [OpenApiOperation(operationId: "DeleteProduct", tags: new[] { "Products" }, Summary = "Delete a product", Description = "Deletes a product by its ID.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The product ID")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "Product deleted successfully")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Product not found")]
    public HttpResponseData DeleteProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id}")] HttpRequestData req,
        string id,
        FunctionContext executionContext)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        
        if (product == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        Products.Remove(product);
        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Search products by category.
    /// </summary>
    [Function(nameof(SearchProductsByCategory))]
    [OpenApiOperation(operationId: "SearchProductsByCategory", tags: new[] { "Products" }, Summary = "Search products by category", Description = "Searches for products in a specific category.")]
    [OpenApiParameter(name: "category", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The product category to search for")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Product>), Description = "List of products in the category")]
    public async Task<HttpResponseData> SearchProductsByCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/search")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var category = query["category"];

        var filteredProducts = string.IsNullOrEmpty(category)
            ? Products
            : Products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(filteredProducts);
        return response;
    }
}
