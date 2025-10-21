using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Threading.Tasks;

namespace AzureNamingTool.Controllers.V2
{
    /// <summary>
    /// API controller for managing Azure Policy definitions (V2).
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;
        private readonly IAdminLogService _adminLogService;

        /// <summary>
        /// Initializes a new instance of the PolicyController.
        /// </summary>
        /// <param name="policyService">Service for policy operations</param>
        /// <param name="adminLogService">Service for admin logging</param>
        public PolicyController(IPolicyService policyService, IAdminLogService adminLogService)
        {
            _policyService = policyService;
            _adminLogService = adminLogService;
        }

        /// <summary>
        /// Gets the Azure Policy definition for resource naming validation.
        /// </summary>
        /// <returns>ApiResponse containing the policy definition</returns>
        /// <response code="200">Returns the policy definition wrapped in ApiResponse</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetPolicyDefinition()
        {
            try
            {
                var serviceResponse = await _policyService.GetPolicyAsync();
                var response = new ApiResponse<object>
                {
                    Success = serviceResponse.Success,
                    Data = serviceResponse.ResponseObject,
                    Metadata = new ApiMetadata { CorrelationId = HttpContext.TraceIdentifier, Timestamp = System.DateTime.UtcNow, Version = "2.0" }
                };
                if (!serviceResponse.Success)
                {
                    response.Error = new ApiError { Code = "POLICY_GENERATION_FAILED", Message = "Failed to generate policy definition" };
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                var response = ApiResponse<object>.ErrorResponse("INTERNAL_SERVER_ERROR", $"Error generating policy: {ex.Message}", "PolicyController.GetPolicyDefinition");
                response.Error.InnerError = new ApiInnerError { Code = ex.GetType().Name };
                response.Metadata.CorrelationId = HttpContext.TraceIdentifier;
                return StatusCode(500, response);
            }
        }
    }
}
