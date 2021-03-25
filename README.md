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
- Run the `deploy-infrastructure.sh` script

### Run

If you want to run both device and the backend locally, simply run `docker-compose up --build`.

If you want to deploy the backend to azure, then:
- Run the `deploy-apps.sh` script
- Run `docker-compose up --build device` to only run the device locally

# Cleanup

- simply remove the created resource group in Azure