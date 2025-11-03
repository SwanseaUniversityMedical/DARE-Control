#!/bin/bash

# Check if required environment variables are provided
if [[ -z "$TRINO_SERVER_URL" || -z "$SQL_STATEMENT" || -z "$USER_NAME" || -z "$SCHEMA" || -z "$PASSWORD" || -z "$CATALOG" ]]; then
    echo "Please provide TRINO_SERVER_URL, ACCESS_TOKEN, SCHEMA, CATALOG, SQL_STATEMENT and USER_NAME environment variables ."
    exit 1
fi


# Execute Trino CLI command to run SQL statement and output CSV
trino --server "$TRINO_SERVER_URL" --execute "$SQL_STATEMENT" --password "$PASSWORD" --user "$USER_NAME" --schema "$SCHEMA" --catalog "$CATALOG" --insecure --output-format CSV > ~/result.csv

ls -la ~/

