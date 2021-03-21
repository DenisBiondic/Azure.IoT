# Azure.IoT

End to end example of main IoT Hub functionality, using Docker containers. Infrastructure as code included.


# Getting started

### Tools required

- VSCode
- Azure CLI
- IoT extension for Azure CLI: `az extension add --name azure-iot`

### Setup

- Copy the .env.template to .env file (set on .gitignore) and fill out the values required. 
- `az login` & `az account set --subscription ...` where you want to deploy
- Run the `deploy.sh` script
- Start the docker-compose


# Cleanup

- simply remove the created resource group in Azure