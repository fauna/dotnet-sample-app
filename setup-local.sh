#!/bin/bash

fauna eval --url=http://localhost:8443 --secret=secret "Database.create({ name: 'ECommerceDotnet' })"
FAUNA_SECRET=$(fauna eval --url=http://localhost:8443 --secret=secret --format=json "Key.create({ role: 'admin', database: 'ECommerceDotnet' }).secret" | jq -r)

# Check if .fauna-project does NOT exist in the current directory
if [ ! -f ".fauna-project" ]; then
  # Perform your action here, e.g., print a message
  echo ".fauna-project does not exist. Creating..."
  cat << EOF > .fauna-project
schema_directory=schema
default=local

[environment.local]
endpoint=local
database=ECommerceDotnet
EOF
fi

fauna schema push -y --url=http://localhost:8443 --secret="$FAUNA_SECRET"
fauna schema commit -y --url=http://localhost:8443 --secret="$FAUNA_SECRET"

echo "Secret: $FAUNA_SECRET"
