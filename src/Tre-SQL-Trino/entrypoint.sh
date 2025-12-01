#!/bin/bash

# Check if required environment variables are provided
if [[ -z "$trinoURL" || -z "$SQL_STATEMENT" || -z "$trinoPassword" || -z "$trinoUsername" || -z "$SCHEMA" || -z "$CATALOG"   ]]; then
    echo "Please provide trinoURL, trinoPassword, SCHEMA, CATALOG, SQL_STATEMENT and trinoUsername environment variables ."
    exit 1
fi
export TRINO_PASSWORD="$trinoPassword"

# Execute Trino CLI command to run SQL statement and output CSV
trino --server="$trinoURL" --execute="$SQL_STATEMENT" --password="true" --user="$trinoUsername" --schema="$SCHEMA" --catalog="$CATALOG" --insecure --output-format CSV > ~/result.csv 2>&1

ls -la ~/

