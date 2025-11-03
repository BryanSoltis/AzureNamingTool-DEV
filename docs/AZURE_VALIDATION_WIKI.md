# Azure Validation Feature

**Last Updated:** November 3, 2025  
**Version:** 5.0.0+

---

## Overview

The Azure Validation feature enables the Azure Naming Tool to validate generated resource names against your actual Azure tenant in real-time. This ensures that:

- **Names are unique** - Prevents naming conflicts before deployment
- **Resources don't exist** - Checks if a resource with the same name already exists
- **Compliance is maintained** - Validates against your organization's actual Azure environment
- **Deployment success** - Reduces deployment failures due to naming conflicts

### How It Works

When you generate a resource name with Azure Validation enabled:

1. **Name Generation** - The tool generates a name based on your naming convention
2. **Azure Query** - The tool queries your Azure tenant using Azure Resource Graph or CheckNameAvailability API
3. **Validation Result** - Returns whether the name exists in your Azure environment
4. **Conflict Resolution** - Optionally auto-increments the instance number if a conflict is found
5. **Metadata Returned** - Provides validation metadata including resource IDs if found

### Validation Methods

The Azure Validation feature uses two methods depending on resource type:

| Method | Used For | Description |
|--------|----------|-------------|
| **Azure Resource Graph** | Most resources | Queries existing resources across subscriptions using KQL |
| **CheckNameAvailability API** | Globally unique resources | Uses Azure Management API for storage accounts, Key Vaults, etc. |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure Naming Tool                        │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  1. User Generates Name (e.g., "vm-prod-eus2-app-001")│  │
│  └───────────────────────────────────────────────────────┘  │
│                          │                                   │
│                          ▼                                   │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  2. Azure Validation Service                          │  │
│  │     - Checks if validation enabled                    │  │
│  │     - Authenticates to Azure                          │  │
│  │     - Checks cache (if enabled)                       │  │
│  └───────────────────────────────────────────────────────┘  │
│                          │                                   │
│                          ▼                                   │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  3. Query Azure Tenant                                │  │
│  │     ┌─────────────────┬─────────────────┐             │  │
│  │     │ Resource Graph  │ Check Name API  │             │  │
│  │     │ (Most Resources)│ (Global Scope)  │             │  │
│  │     └─────────────────┴─────────────────┘             │  │
│  └───────────────────────────────────────────────────────┘  │
│                          │                                   │
│                          ▼                                   │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  4. Return Validation Result                          │  │
│  │     - Exists: true/false                              │  │
│  │     - Resource IDs (if found)                         │  │
│  │     - Validation metadata                             │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
                   ┌─────────────┐
                   │ Azure Tenant│
                   │ (Your Subs) │
                   └─────────────┘
```

---

## Authentication Methods

Azure Validation supports two authentication methods:

### 1. Managed Identity (Recommended)

**Best for:** Azure-hosted deployments (App Service, Container Apps, VMs, AKS)

**Advantages:**
- ✅ No secrets to manage
- ✅ Automatic credential rotation
- ✅ More secure (credentials never leave Azure)
- ✅ Easier to set up
- ✅ Integrated with Azure RBAC

**Disadvantages:**
- ❌ Only works when hosted in Azure
- ❌ Cannot be used for local development

### 2. Service Principal

**Best for:** On-premises deployments, local development, non-Azure hosting

**Advantages:**
- ✅ Works anywhere (Azure, on-prem, local)
- ✅ Good for CI/CD pipelines
- ✅ Can be used in development environments

**Disadvantages:**
- ❌ Requires managing secrets
- ❌ Secrets need periodic rotation
- ❌ Higher security risk if secrets are compromised

---

## Configuration Guide

### Prerequisites

Before configuring Azure Validation, ensure you have:

- [ ] Azure subscription(s) with resources to validate against
- [ ] Appropriate permissions to create identities or service principals
- [ ] Access to the Azure Naming Tool Admin section

---

## Option 1: Managed Identity Setup

### Step 1: Enable Managed Identity

Choose the appropriate method based on your hosting environment:

#### **Azure App Service**

```bash
# Enable System-Assigned Managed Identity
az webapp identity assign \
  --name <your-app-name> \
  --resource-group <your-resource-group>

# Get the Principal ID (you'll need this for RBAC)
az webapp identity show \
  --name <your-app-name> \
  --resource-group <your-resource-group> \
  --query principalId \
  --output tsv
```

#### **Azure Container Apps**

```bash
# Enable System-Assigned Managed Identity
az containerapp identity assign \
  --name <your-containerapp-name> \
  --resource-group <your-resource-group>

# Get the Principal ID
az containerapp identity show \
  --name <your-containerapp-name> \
  --resource-group <your-resource-group> \
  --query principalId \
  --output tsv
```

#### **Azure VM**

```bash
# Enable System-Assigned Managed Identity
az vm identity assign \
  --name <your-vm-name> \
  --resource-group <your-resource-group>

# Get the Principal ID
az vm identity show \
  --name <your-vm-name> \
  --resource-group <your-resource-group> \
  --query principalId \
  --output tsv
```

#### **Azure Kubernetes Service (AKS)**

```bash
# Create a user-assigned managed identity
az identity create \
  --name naming-tool-identity \
  --resource-group <your-resource-group>

# Get the identity details
az identity show \
  --name naming-tool-identity \
  --resource-group <your-resource-group> \
  --query "{clientId: clientId, principalId: principalId}" \
  --output json

# Configure your pod to use the identity (via Azure Workload Identity or aad-pod-identity)
```

### Step 2: Assign RBAC Permissions

The managed identity needs **Reader** permissions to query Azure resources.

#### **Single Subscription**

```bash
# Assign Reader role at subscription scope
az role assignment create \
  --assignee <principal-id-from-step-1> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id>"
```

#### **Multiple Subscriptions**

```bash
# Repeat for each subscription
az role assignment create \
  --assignee <principal-id-from-step-1> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id-1>"

az role assignment create \
  --assignee <principal-id-from-step-1> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id-2>"
```

#### **Management Group (Recommended for Large Organizations)**

```bash
# Assign Reader role at management group scope (inherited by all subscriptions)
az role assignment create \
  --assignee <principal-id-from-step-1> \
  --role "Reader" \
  --scope "/providers/Microsoft.Management/managementGroups/<management-group-id>"
```

#### **Verify Role Assignment**

```bash
# Check role assignments for the managed identity
az role assignment list \
  --assignee <principal-id-from-step-1> \
  --output table
```

### Step 3: Configure in Azure Naming Tool

1. Navigate to **Admin** → **Site Configuration** → **Azure Validation**
2. Click **Edit Configuration**
3. Set the following:
   - **Enable Azure Validation**: ✅ Checked
   - **Authentication Mode**: `Managed Identity`
   - **Tenant ID**: Your Azure AD tenant ID
   - **Subscription IDs**: Add all subscriptions to query (comma-separated or one per line)
4. Click **Save**

#### Finding Your Tenant ID

```bash
# Get your tenant ID
az account show --query tenantId --output tsv
```

#### Getting Subscription IDs

```bash
# List all subscriptions you have access to
az account list --query "[].{Name:name, SubscriptionId:id}" --output table

# Get current subscription ID
az account show --query id --output tsv
```

### Step 4: Test the Configuration

1. Navigate to **Generate** page
2. Generate a name for any resource type
3. Enable **Azure Validation** toggle
4. Generate the name
5. Check the validation result below the generated name

**Expected Result:**
```
✅ Validation Performed: Yes
✅ Exists in Azure: No (or Yes with Resource IDs)
✅ Timestamp: [current time]
```

---

## Option 2: Service Principal Setup

### Step 1: Create Service Principal

```bash
# Create a service principal with Reader role
az ad sp create-for-rbac \
  --name "naming-tool-sp" \
  --role "Reader" \
  --scopes "/subscriptions/<subscription-id>" \
  --output json

# Save the output - YOU WILL NEED THESE VALUES:
# {
#   "appId": "00000000-0000-0000-0000-000000000000",       # CLIENT_ID
#   "displayName": "naming-tool-sp",
#   "password": "your-secret-here",                        # CLIENT_SECRET
#   "tenant": "00000000-0000-0000-0000-000000000000"       # TENANT_ID
# }
```

⚠️ **IMPORTANT:** Save the `password` (client secret) immediately - it cannot be retrieved later!

### Step 2: Assign Additional Subscriptions (if needed)

```bash
# Get the service principal's App ID (if you didn't save it)
az ad sp list --display-name "naming-tool-sp" --query "[0].appId" --output tsv

# Assign to additional subscriptions
az role assignment create \
  --assignee <app-id-from-above> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id-2>"
```

### Step 3: Store Client Secret

#### **Option A: Direct Configuration (Not Recommended for Production)**

Store the client secret directly in the Azure Naming Tool configuration.

⚠️ **Security Risk:** Secret is stored in the database/configuration file.

#### **Option B: Azure Key Vault (Recommended)**

Store the client secret in Azure Key Vault for enhanced security.

```bash
# Create a Key Vault (if you don't have one)
az keyvault create \
  --name <your-keyvault-name> \
  --resource-group <your-resource-group> \
  --location <location>

# Store the client secret
az keyvault secret set \
  --vault-name <your-keyvault-name> \
  --name "naming-tool-client-secret" \
  --value "<client-secret-from-step-1>"

# Grant the naming tool's managed identity access to Key Vault
az keyvault set-policy \
  --name <your-keyvault-name> \
  --object-id <naming-tool-managed-identity-principal-id> \
  --secret-permissions get

# Get the Key Vault URI
az keyvault show \
  --name <your-keyvault-name> \
  --query properties.vaultUri \
  --output tsv
```

### Step 4: Configure in Azure Naming Tool

1. Navigate to **Admin** → **Site Configuration** → **Azure Validation**
2. Click **Edit Configuration**
3. Set the following:
   - **Enable Azure Validation**: ✅ Checked
   - **Authentication Mode**: `Service Principal`
   - **Tenant ID**: `<tenant-id-from-step-1>`
   - **Subscription IDs**: Add all subscriptions to query
   - **Client ID**: `<appId-from-step-1>`
   - **Client Secret**: (Choose one)
     - **Direct**: Enter `<password-from-step-1>`
     - **Key Vault**: Leave blank and configure Key Vault settings below
   - **Key Vault Settings** (if using Key Vault):
     - **Key Vault URI**: `https://<your-keyvault-name>.vault.azure.net/`
     - **Secret Name**: `naming-tool-client-secret`
4. Click **Save**

### Step 5: Test the Configuration

Same as Managed Identity Step 4 above.

---

## Advanced Configuration

### Conflict Resolution Strategies

When a name already exists in Azure, the tool can handle it in different ways:

| Strategy | Behavior | Use Case |
|----------|----------|----------|
| **Notify Only** (Default) | Returns name with a warning | User decides what to do |
| **Auto Increment** | Automatically increments instance number | Automatic unique name generation |
| **Fail** | Returns an error | Strict naming enforcement |
| **Suffix Random** | Adds random characters | When instance numbers aren't used |

**Example - Auto Increment:**
```
Requested: vm-prod-eus2-app-001
Exists in Azure: vm-prod-eus2-app-001
Auto-incremented: vm-prod-eus2-app-002
Exists in Azure: vm-prod-eus2-app-002
Auto-incremented: vm-prod-eus2-app-003
✅ Available: vm-prod-eus2-app-003
```

### Cache Settings

Validation results are cached to improve performance and reduce Azure API calls.

**Default Settings:**
- **Enabled**: Yes
- **Duration**: 5 minutes

**Configuration:**
```json
{
  "Cache": {
    "Enabled": true,
    "DurationMinutes": 5
  }
}
```

**Considerations:**
- ✅ Reduces Azure Resource Graph query costs
- ✅ Improves response time for repeated queries
- ⚠️ May return stale results for recently created/deleted resources
- 💡 Use shorter durations (1-2 minutes) for highly dynamic environments

### Resource Type Exclusions

Exclude specific resource types from validation:

**Use Cases:**
- Resources not yet supported by Azure Resource Graph
- Resources you don't want to validate
- Performance optimization

**Example Configuration:**
```
Excluded Resource Types:
- Microsoft.Network/trafficManagerProfiles
- Microsoft.Cdn/profiles
```

### Subscription Filtering

**Best Practices:**
- ✅ Include only subscriptions where you deploy resources
- ✅ Use Management Groups for large organizations
- ⚠️ More subscriptions = longer query time
- 💡 Group subscriptions by environment (dev, test, prod)

---

## Troubleshooting

### Common Issues

#### 1. "Failed to authenticate to Azure"

**Possible Causes:**
- Managed Identity not enabled
- Service Principal credentials incorrect
- Key Vault access denied

**Solutions:**
```bash
# Verify managed identity exists
az webapp identity show --name <app-name> --resource-group <rg-name>

# Verify service principal exists
az ad sp show --id <client-id>

# Test Key Vault access
az keyvault secret show --vault-name <vault-name> --name <secret-name>
```

#### 2. "Validation performed but no results"

**Possible Causes:**
- No Reader role assigned
- Subscription IDs incorrect
- Resource doesn't exist in specified subscriptions

**Solutions:**
```bash
# Verify role assignments
az role assignment list --assignee <principal-id> --output table

# Verify subscription IDs
az account list --query "[].{Name:name, ID:id}" --output table

# Test manual query
az graph query -q "Resources | where type == 'microsoft.compute/virtualmachines' | project name"
```

#### 3. "Rate limiting or quota errors"

**Possible Causes:**
- Too many Azure Resource Graph queries
- Cache disabled
- High-frequency validation requests

**Solutions:**
- Enable caching
- Increase cache duration
- Reduce validation frequency
- Use batch validation (API)

#### 4. "Global scope resources not validated correctly"

**Possible Causes:**
- CheckNameAvailability API permissions missing
- Resource provider not registered

**Solutions:**
```bash
# Register required resource providers
az provider register --namespace Microsoft.Storage
az provider register --namespace Microsoft.KeyVault

# Verify registration
az provider show --namespace Microsoft.Storage --query "registrationState"
```

---

## Security Best Practices

### Managed Identity (Recommended)

✅ **DO:**
- Use system-assigned managed identity when possible
- Assign least-privilege permissions (Reader only)
- Use management group scope for multi-subscription access
- Monitor and audit role assignments regularly

❌ **DON'T:**
- Grant more permissions than needed (e.g., Contributor)
- Share managed identities across multiple applications
- Use user-assigned identities unless required

### Service Principal

✅ **DO:**
- Store secrets in Azure Key Vault
- Rotate secrets regularly (every 90 days)
- Use separate service principals per environment
- Enable secret expiration dates
- Monitor service principal sign-ins

❌ **DON'T:**
- Store secrets in plain text
- Commit secrets to source control
- Use the same service principal for multiple apps
- Create secrets without expiration dates

### Network Security

✅ **DO:**
- Use Private Endpoints for Key Vault access
- Restrict Key Vault network access
- Enable Azure Defender for Key Vault

---

## Performance Optimization

### Caching Strategy

| Environment | Cache Duration | Rationale |
|-------------|----------------|-----------|
| Development | 1-2 minutes | Frequently changing resources |
| Testing | 5 minutes (default) | Moderate change frequency |
| Production | 10-15 minutes | Stable environment |

### Subscription Filtering

**Optimize query performance:**
```
✅ Good: 2-5 subscriptions per environment
⚠️ Acceptable: 6-10 subscriptions
❌ Poor: 10+ subscriptions (consider Management Groups)
```

### Resource Type Exclusions

Exclude resource types that are:
- Not deployed in your environment
- Not managed by your naming convention
- High-volume but low-priority for validation

---

## API Integration

### REST API Endpoint

```http
POST /api/v1/generate
Content-Type: application/json

{
  "resourceEnvironment": "prod",
  "resourceInstance": "001",
  "resourceLocation": "eastus2",
  "resourceProjAppSvc": "app",
  "resourceType": "virtualmachines",
  "validateAzure": true
}
```

### Response with Validation

```json
{
  "success": true,
  "resourceName": "vm-prod-eus2-app-001",
  "validation": {
    "validationPerformed": true,
    "existsInAzure": false,
    "validationTimestamp": "2025-11-03T10:30:00Z",
    "resourceIds": [],
    "cacheHit": false
  }
}
```

---

## Monitoring & Logging

### Admin Logs

Azure Validation operations are logged in the Admin Log:

**Log Entries:**
- Authentication successes/failures
- Validation queries performed
- Cache hits/misses
- Configuration changes
- Errors and warnings

**Accessing Logs:**
1. Navigate to **Admin** → **Admin Log**
2. Filter by "Azure Validation" or "INFORMATION"
3. Review timestamps and messages

### Application Insights (Optional)

If using Azure Application Insights:

**Key Metrics:**
- Validation request count
- Cache hit rate
- Query duration
- Authentication failures
- API throttling events

**Sample KQL Query:**
```kql
traces
| where message contains "Azure validation"
| summarize count() by bin(timestamp, 1h), severityLevel
| render timechart
```

---

## Migration from JSON to SQLite

If you configured Azure Validation in FileSystem mode (JSON files) and then migrated to SQLite:

### ✅ v5.0.0+ (Fixed)

Azure Validation settings are now included in the storage migration.

**What happens:**
1. You configure Azure Validation in FileSystem mode
2. You migrate to SQLite storage
3. ✅ Azure Validation settings are automatically migrated
4. ✅ No reconfiguration needed

### ❌ Pre-v5.0.0 (Bug)

Azure Validation settings were **not** migrated automatically.

**Workaround:**
1. Export configuration before migration (Admin → Configuration → Export)
2. Migrate to SQLite
3. Manually re-enter Azure Validation settings
4. Or: Rollback to JSON, upgrade to v5.0.0+, re-migrate

---

## FAQ

### Q: Do I need Azure Resource Graph permissions?

**A:** No explicit Resource Graph permissions are needed. The **Reader** role at subscription scope provides sufficient access to query Azure Resource Graph.

### Q: Can I use Azure Validation with on-premises deployments?

**A:** Yes, using Service Principal authentication. Managed Identity only works when hosted in Azure.

### Q: How much does Azure Resource Graph cost?

**A:** Azure Resource Graph has a [free tier](https://azure.microsoft.com/en-us/pricing/details/azure-resource-graph/) of 1,000 queries per tenant per month. Beyond that, queries cost $0.001 per query. Caching helps reduce query costs.

### Q: Can I validate names across multiple tenants?

**A:** No, Azure Validation is scoped to a single tenant. You would need separate configurations for each tenant.

### Q: What happens if Azure is unavailable?

**A:** Validation is skipped and the name is returned without validation metadata. The tool does not block name generation.

### Q: Can I validate custom resource types?

**A:** Yes, if the resource type exists in Azure Resource Graph or has a CheckNameAvailability API.

### Q: How do I know which resources are validated?

**A:** Check the validation metadata in the response. `ValidationPerformed: true` indicates validation was attempted.

### Q: Can I disable validation for specific resource types?

**A:** Yes, use the Resource Type Exclusions feature in the configuration.

---

## Support & Resources

### Documentation

- [Azure Resource Graph Overview](https://learn.microsoft.com/en-us/azure/governance/resource-graph/overview)
- [Managed Identities for Azure Resources](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)
- [Azure Service Principals](https://learn.microsoft.com/en-us/entra/identity-platform/app-objects-and-service-principals)
- [Azure Key Vault Secrets](https://learn.microsoft.com/en-us/azure/key-vault/secrets/about-secrets)

### Community

- [Azure Naming Tool GitHub Issues](https://github.com/mspnp/AzureNamingTool/issues)
- [Azure Naming Tool Discussions](https://github.com/mspnp/AzureNamingTool/discussions)

### Contact

For questions or issues, please open an issue on GitHub or contact your Azure administrator.

---

**Document Version:** 1.0  
**Last Updated:** November 3, 2025  
**Applies To:** Azure Naming Tool v5.0.0 and later
