#!/bin/bash

sed -i "3s/.*/\"Api\": \"Version ${DMPW_API_VERSION}\",/" appsettings.json
sed -i "4s/.*/\"WebApp\": \"Version ${WEBAPP_VERSION}\",/" appsettings.json