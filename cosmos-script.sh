# Login to Azure with an account that has AAD and Cosmos Owner/Admin role
az login

# create a service principal with reader RBAC permissions to access Cosmos DB - least privilege principle
az ad sp create-for-rbac \
--name http://cm-cosmos-reader \
--role reader \
--scope /subscriptions/e42acc2d-8462-4fb5-bf0d-d983c0017584/resourceGroups/identity/providers/Microsoft.DocumentDB/databaseAccounts/cm-cosmos-demo

# retrieve the ObjectId of the service principal - we'll use it later
az ad sp show \
--id 6e96c4ff-8b6b-4431-8d87-8f44a01a16f7 \
--query "objectId"

# list existing RBAC roles on the specifice Cosmos DB instance
az cosmosdb sql role definition show \
--account-name cm-cosmos-demo -g identity \
--id "/subscriptions/e42acc2d-8462-4fb5-bf0d-d983c0017584/resourceGroups/identity/providers/Microsoft.DocumentDB/databaseAccounts/cm-cosmos-demo/sqlRoleDefinitions/a5659812-fee7-4409-9214-ca9d53446e17"

# [Optional] if necessary, create a custom RBAC role to access Cosmos DB
az cosmosdb sql role definition create \
--account-name cm-cosmos-demo -g identity \ 
--body @cosmos-readonly-role.json

# create some variables - update values to match your environment
accountName='cm-cosmos-demo'
readOnlyRoleDefinitionId='/subscriptions/e42acc2d-8462-4fb5-bf0d-d983c0017584/resourceGroups/identity/providers/Microsoft.DocumentDB/databaseAccounts/cm-cosmos-demo/sqlRoleDefinitions/a5659812-fee7-4409-9214-ca9d53446e17'
principalId='934e8351-249a-4ae3-8a67-84603d8e7d71'
resourceGroupName='identity'

# Assigne the role to the service principal
az cosmosdb sql role assignment create \
--account-name $accountName \
--resource-group $resourceGroupName \
--scope "/" --principal-id $principalId \
--role-definition-id $readOnlyRoleDefinitionId

# On the local dev environment, sign in to the Azure CLI using the service principal
az login --service-principal \
-u <clientId> \
-p <secret> \
-t 72f988bf-86f1-41af-91ab-2d7cd011db47 # change this to your tenant Id
