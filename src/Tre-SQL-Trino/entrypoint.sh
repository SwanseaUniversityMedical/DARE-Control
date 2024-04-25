#!/bin/bash

if [ -z "$TRINO_SERVER_URL" ]; then
    export TRINO_SERVER_URL="http://192.168.70.92:8090"
fi

if [ -z "$LOCATION" ]; then
    export LOCATION="/data"
fi

# Check if required environment variables are provided
if [[ -z "$SQL_STATEMENT" ]]; then
    echo "SQL_STATEMENT environment variables."
    exit 1
fi

# Execute Trino CLI command to run SQL statement and output CSV
trino --server "$TRINO_SERVER_URL" --execute "$SQL_STATEMENT" --output-format CSV > $LOCATION
