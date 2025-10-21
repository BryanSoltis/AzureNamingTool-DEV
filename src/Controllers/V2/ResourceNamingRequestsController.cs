#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Threading.Tasks;
using AzureNamingTool.Services.Interfaces;
using AzureNamingTool.Attributes;

namespace AzureNamingTool.Controllers.V2
{
    /// <summary>
    /// Controller for handling resource naming requests (API v2.0).
    /// This version uses standardized ApiResponse wrapper with enhanced error handling.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class ResourceNamingRequestsController : ControllerBase
    {
        private readonly IResourceNamingRequestService _resourceNamingRequestService;
        private readonly IResourceTypeService _resourceTypeService;
        private readonly IAdminLogService _adminLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResourceNamingRequestsController(
            IResourceNamingRequestService resourceNamingRequestService,
            IResourceTypeService resourceTypeService,
            IAdminLogService adminLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            _resourceNamingRequestService = resourceNamingRequestService;
            _resourceTypeService = resourceTypeService;
            _adminLogService = adminLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        }

        /// <summary>
        /// Generate a resource type name with full component definition.
        /// This function requires complete definition for all components.
        /// It is recommended to use the RequestName API function for simplified name generation.
        /// </summary>
        /// <param name="request">ResourceNameRequestWithComponents (json) - Complete resource name request data with all components</param>
        /// <returns>Standardized API response with name generation result</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<ResourceNameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestNameWithComponents([FromBody] ResourceNameRequestWithComponents request)
        {
            try
            {
                if (request == null)
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INVALID_REQUEST",
                        "Request body cannot be null",
                        "ResourceNameRequestWithComponents"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                var resourceNameRequestResponse = await _resourceNamingRequestService.RequestNameWithComponentsAsync(request);
                
                if (resourceNameRequestResponse.Success)
                {
                    var response = ApiResponse<ResourceNameResponse>.SuccessResponse(
                        resourceNameRequestResponse,
                        "Resource name generated successfully"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "NAME_GENERATION_FAILED",
                        resourceNameRequestResponse.Message ?? "Failed to generate resource name",
                        "RequestNameWithComponents"
                    );
                    
                    // Include validation details if available
                    if (!string.IsNullOrEmpty(resourceNameRequestResponse.Message))
                    {
                        response.Error!.Details = new System.Collections.Generic.List<ApiError>
                        {
                            new ApiError
                            {
                                Code = "VALIDATION_ERROR",
                                Message = resourceNameRequestResponse.Message
                            }
                        };
                    }
                    
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() 
                { 
                    Title = "ERROR", 
                    Message = $"V2 API - RequestNameWithComponents failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while generating resource name: {ex.Message}",
                    "ResourceNamingRequestsController.RequestNameWithComponents"
                );
                response.Error!.InnerError = new ApiInnerError
                {
                    Code = ex.GetType().Name
                };
                response.Metadata.CorrelationId = GetCorrelationId();
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Generate a resource type name using simplified data format.
        /// This is the recommended method for name generation as it uses a simpler request structure.
        /// </summary>
        /// <param name="request">ResourceNameRequest (json) - Simplified resource name request data</param>
        /// <returns>Standardized API response with name generation result</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<ResourceNameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestName([FromBody] ResourceNameRequest request)
        {
            try
            {
                if (request == null)
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INVALID_REQUEST",
                        "Request body cannot be null",
                        "ResourceNameRequest"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                request.CreatedBy = "API-V2";
                var resourceNameRequestResponse = await _resourceNamingRequestService.RequestNameAsync(request);
                
                if (resourceNameRequestResponse.Success)
                {
                    await _adminLogService.PostItemAsync(new AdminLogMessage() 
                    { 
                        Title = "INFORMATION", 
                        Message = $"V2 API - Generated name: {resourceNameRequestResponse.ResourceName}" 
                    });
                    
                    var response = ApiResponse<ResourceNameResponse>.SuccessResponse(
                        resourceNameRequestResponse,
                        "Resource name generated successfully"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return Ok(response);
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "NAME_GENERATION_FAILED",
                        resourceNameRequestResponse.Message ?? "Failed to generate resource name",
                        "RequestName"
                    );
                    
                    // Include validation details if available
                    if (!string.IsNullOrEmpty(resourceNameRequestResponse.Message))
                    {
                        response.Error!.Details = new System.Collections.Generic.List<ApiError>
                        {
                            new ApiError
                            {
                                Code = "VALIDATION_ERROR",
                                Message = resourceNameRequestResponse.Message
                            }
                        };
                    }
                    
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() 
                { 
                    Title = "ERROR", 
                    Message = $"V2 API - RequestName failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while generating resource name: {ex.Message}",
                    "ResourceNamingRequestsController.RequestName"
                );
                response.Error!.InnerError = new ApiInnerError
                {
                    Code = ex.GetType().Name
                };
                response.Metadata.CorrelationId = GetCorrelationId();
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Validate a resource name against the regex pattern for the specified resource type.
        /// NOTE: This function validates using the resource type regex only, not the full tool configuration.
        /// Use the RequestName function to validate against the complete tool configuration.
        /// </summary>
        /// <param name="validateNameRequest">ValidateNameRequest (json) - Name validation request data</param>
        /// <returns>Standardized API response with validation result</returns>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApiResponse<ValidateNameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateName([FromBody] ValidateNameRequest validateNameRequest)
        {
            try
            {
                if (validateNameRequest == null)
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "INVALID_REQUEST",
                        "Request body cannot be null",
                        "ValidateNameRequest"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                if (string.IsNullOrEmpty(validateNameRequest.Name))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "MISSING_NAME",
                        "Name is required for validation",
                        "ValidateNameRequest.Name"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                if (string.IsNullOrEmpty(validateNameRequest.ResourceType))
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "MISSING_RESOURCE_TYPE",
                        "ResourceType is required for validation",
                        "ValidateNameRequest.ResourceType"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }

                var serviceResponse = await _resourceTypeService.ValidateResourceTypeNameAsync(validateNameRequest);
                
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        var validateNameResponse = (ValidateNameResponse)serviceResponse.ResponseObject!;
                        var response = ApiResponse<ValidateNameResponse>.SuccessResponse(
                            validateNameResponse,
                            validateNameResponse.Valid 
                                ? "Resource name is valid" 
                                : "Resource name validation failed"
                        );
                        response.Metadata.CorrelationId = GetCorrelationId();
                        return Ok(response);
                    }
                    else
                    {
                        var response = ApiResponse<object>.ErrorResponse(
                            "VALIDATION_RESULT_NULL",
                            "There was a problem validating the name - no result returned",
                            "ValidateName"
                        );
                        response.Metadata.CorrelationId = GetCorrelationId();
                        return BadRequest(response);
                    }
                }
                else
                {
                    var response = ApiResponse<object>.ErrorResponse(
                        "VALIDATION_FAILED",
                        serviceResponse.ResponseObject?.ToString() ?? "There was a problem validating the name",
                        "ValidateName"
                    );
                    response.Metadata.CorrelationId = GetCorrelationId();
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() 
                { 
                    Title = "ERROR", 
                    Message = $"V2 API - ValidateName failed: {ex.Message}" 
                });
                
                var response = ApiResponse<object>.ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    $"An unexpected error occurred while validating resource name: {ex.Message}",
                    "ResourceNamingRequestsController.ValidateName"
                );
                response.Error!.InnerError = new ApiInnerError
                {
                    Code = ex.GetType().Name
                };
                response.Metadata.CorrelationId = GetCorrelationId();
                return StatusCode(500, response);
            }
        }
    }
}

#pragma warning restore CS1591
