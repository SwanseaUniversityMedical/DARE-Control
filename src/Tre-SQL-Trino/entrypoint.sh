#!/bin/bash

# Check if required environment variables are provided
if [[ -z "$TRINO_SERVER_URL" || -z "$SQL_STATEMENT" || -z "$LOCATION" ]]; then
    echo "Please provide TRINO_SERVER_URL, SQL_STATEMENT and LOCATION environment variables."
    exit 1
fi

whoami

pwd


# Execute Trino CLI command to run SQL statement and output CSV
trino --server "$TRINO_SERVER_URL" --execute "$SQL_STATEMENT" --output-format CSV > $LOCATION

cd ~/
pwd
ls -la ~/
