#!/bin/bash
set -e

# NOTE: only deploys and pushes the backend applications
# NOTE: Azure CLI code below is for simple demonstrative purposes, not a particurarlly good infrastructure as code approach ;)

function read_parameters {
  set -a
  [ -f .env ] && . .env
  set +a

  echo "IMPORTANT: run the deploy-infrastructure script first, follow the instructions, set the .env variables!"

  if [ -z "$ENVIRONMENT_TAG" ]; then
    echo "ENVIRONMENT_TAG missing. Fill our the .env file"
    exit 1 
  fi
}

read_parameters

source ./naming-conventions.sh

VERSION=$(date +%s)

az acr login -n $registry

echo "Building images..."
# we only build and push the backend services. The simulators and other stuff has to be started locally!
docker-compose build

echo "Tagging images..."
full_image_name=$registry.azurecr.io/$BACKEND_IMAGE_NAME:$VERSION
docker tag $BACKEND_IMAGE_NAME:latest $full_image_name

echo "Pushing images..."
docker push $full_image_name

# switch to managed identity once running stable with azure container instances
registry_username=$(az acr credential show -n $registry --query "username" -o tsv)
registry_password=$(az acr credential show -n $registry --query "passwords[0].value" -o tsv)

echo "Deploying to Azure Container Instances..."
az container create --resource-group $resource_group --name $container_instance_name --image $full_image_name \
  --cpu 1 --memory 1 --dns-name-label $container_dns \
  --registry-login-server $registry.azurecr.io \
  --registry-username $registry_username \
  --registry-password $registry_password \
  --environment-variables BACKEND_IOT_HUB_CONNECTION_STRING="$BACKEND_IOT_HUB_CONNECTION_STRING" BACKEND_IOT_HUB_EVENT_HUB_ENDPOINT_NAME="$BACKEND_IOT_HUB_EVENT_HUB_ENDPOINT_NAME"

echo "Successfully completed!"
echo "===================================================================="
echo "If you want to tail the logs of the container, run: "
echo "az container logs --resource-group $resource_group --name $container_instance_name --follow"
echo "===================================================================="