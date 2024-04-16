#!/bin/bash

# Check if required environment variables are provided
if [[ -z "$TRINO_SERVER_URL" || -z "$SQL_STATEMENT" || -z "$ACCESS_TOKEN" || -z "$USER_NAME"  ]]; then
    echo "Please provide TRINO_SERVER_URL, ACCESS_TOKEN, SQL_STATEMENT and USER_NAME environment variables ."
    exit 1
fi


# Execute Trino CLI command to run SQL statement and output CSV
trino --server "$TRINO_SERVER_URL" --execute "$SQL_STATEMENT" --access-token "$ACCESS_TOKEN" --user "$USER_NAME" --insecure --output-format CSV > ~/result.csv

ls -la ~/

