#!/bin/bash

# Function to display help message
usage() {
    echo "Usage: $0 -h <host_ip>"
    exit 1
}

# Parse command line options
while getopts ":h:" opt; do
    case ${opt} in
        h )
            ip4=$OPTARG
            ;;
        \? )
            echo "Invalid option: -$OPTARG" 1>&2
            usage
            ;;
        : )
            echo "Option -$OPTARG requires an argument." 1>&2
            usage
            ;;
    esac
done
shift $((OPTIND -1))

# install docker images
for image in images/*
do
   echo "Install $image service"
   docker load -i $image
   sleep 1
done

rm -rf .env
rm -rf ./settings/env.production.js
rm -rf ./settings/settings.dmpw-api.json
rm -rf ./settings/settings.dmpw-device-sdk.json
rm -rf ./settings/settings.dmpw-device-sdk.Production.json

sed "s/HOST_IP/${ip4}/g" ./settings/.env.example > .env
sed "s/HOST_IP/${ip4}/g" ./settings/.env.production.js.example > ./settings/env.production.js
sed "s/HOST_IP/${ip4}/g" ./settings/.settings.dmpw-api.json.example > ./settings/settings.dmpw-api.json
sed "s/HOST_IP/${ip4}/g" ./settings/.settings.dmpw-device-sdk.json.example > ./settings/settings.dmpw-device-sdk.json
sed "s/HOST_IP/${ip4}/g" ./settings/.settings.dmpw-device-sdk.Production.json.example > ./settings/settings.dmpw-device-sdk.Production.json

mkdir grafana-storage
chown 472 grafana-storage