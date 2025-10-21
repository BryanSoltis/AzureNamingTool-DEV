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
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiKey]
    [Produces("application/json")]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;
        private readonly IAdminLogService _adminLogService;

        public PolicyController(IPolicyService policyService, IAdminLogService adminLogService)
        {
            _policyService = policyService;
            _adminLogService = adminLogService;
        }

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
