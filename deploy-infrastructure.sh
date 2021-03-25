#!/bin/bash
set -e

# NOTE: Azure CLI code below is for simple demonstrative purposes. Consider using a tool like Terraform instead for writing your
#       Infrastructure as Code

function read_parameters {
  set -a
  [ -f .env ] && . .env
  set +a

  echo "IMPORTANT: make sure you are logged in to the azure subscription where you want to deploy the resources!"

  if [ -z "$ENVIRONMENT_TAG" ]; then
    echo "ENVIRONMENT_TAG missing. Fill our the .env file"
    exit 1 
  fi

  if [ -z "$BACKEND_IMAGE_NAME" ]; then
    echo "BACKEND_IMAGE_NAME missing. Fill our the .env file"
    exit 1 
  fi
}

read_parameters

source ./naming-conventions.sh

# we will group everything into a single group, it will be easier to remove everything later
echo "Creating the resource group..."
az group create --location northeurope --name $resource_group -o table

echo "Creating the IoT Hub and the Device Provisioning Service..."
az iot hub create --name $hub --resource-group $resource_group --sku S1 -o table

# dps Azure CLI command is not idempotent yet, so we have to do a check 
dpsExists=$(az iot dps list --query "[?name=='$dps']")
enrollment_id=simulator-enrollment-id

if [ "$dpsExists" == "[]" ]; then
  az iot dps create --name $dps --resource-group $resource_group --location northeurope -o table

  echo "Linking the IoT Hub and the Device Provisioning Service..."
  hubConnectionString=$(az iot hub connection-string show --name $hub --key primary --query connectionString -o tsv)
  az iot dps linked-hub create --dps-name $dps --resource-group $resource_group --connection-string $hubConnectionString --location northeurope -o table

  echo "Creating the device enrollment which can be used to register a device..."
  az iot dps enrollment create -g $resource_group --dps-name $dps --enrollment-id $enrollment_id --attestation-type symmetrickey
fi

echo "Reading out the device enrollment credentials..."
enrollment_primary_key=$(az iot dps enrollment show --enrollment-id $enrollment_id --dps-name $dps --resource-group $resource_group --keys --query attestation.symmetricKey.primaryKey)
enrollment_id_scope=$(az iot dps show --name $dps --resource-group $resource_group --query properties.idScope)

# for cloud endpoint, we need the connection credentials
iot_hub_backend_connection_string=$(az iot hub connection-string show -n $hub --policy-name service --query connectionString --eh)

# setup the app service for optional backend service hosting
echo "Creating the Container Registry..."
az acr create --name $registry --resource-group $resource_group --sku Basic --admin-enabled true -o table

# remove prefix and suffix "
enrollment_primary_key=$(sed -e 's/^"//' -e 's/"$//' <<<$enrollment_primary_key)
enrollment_id_scope=$(sed -e 's/^"//' -e 's/"$//' <<<$enrollment_id_scope)
iot_hub_backend_connection_string=$(sed -e 's/^"//' -e 's/"$//' <<<$iot_hub_backend_connection_string)

echo "Successfully completed!"
echo "Copy the following values to your .env file"
echo "===================================================================="
echo "ENROLLMENT_ID=$enrollment_id"
echo "ENROLLMENT_PRIMARY_KEY=$enrollment_primary_key"
echo "ENROLLMENT_ID_SCOPE=$enrollment_id_scope"
echo "BACKEND_IOT_HUB_CONNECTION_STRING=$iot_hub_backend_connection_string"
echo "BACKEND_IOT_HUB_EVENT_HUB_ENDPOINT_NAME=$hub"
echo "===================================================================="