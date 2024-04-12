#!/bin/bash

# Check if required environment variables are provided
if [[ -z "$TRINO_SERVER_URL" || -z "$SQL_STATEMENT" || -z "$ACCESS_TOKEN" ]]; then
    echo "Please provide TRINO_SERVER_URL and SQL_STATEMENT environment variables ACCESS_TOKEN."
    exit 1
fi


# Execute Trino CLI command to run SQL statement and output CSV
trino --server "$TRINO_SERVER_URL" --execute "$SQL_STATEMENT" --access-token "$ACCESS_TOKEN" --output-format CSV > ~/result.csv

ls -la ~/

