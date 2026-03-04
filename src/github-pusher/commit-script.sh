#!/bin/sh

# Automated Heartbeat Commit Script
# Pushes a timestamp update to GitHub every 2 minutes

while true; do
    echo "[$(date)] Starting heartbeat sync..."

    # 1. Get current file SHA (if it exists)
    API_URL="https://api.github.com/repos/${REPO_OWNER}/${REPO_NAME}/contents/heartbeat.txt"
    RESPONSE=$(curl -s -H "Authorization: token ${GITHUB_TOKEN}" "${API_URL}")
    
    SHA=$(echo "${RESPONSE}" | jq -r '.sha // empty')
    
    # 2. Prepare content
    CONTENT_STRING="Heartbeat at $(date)"
    CONTENT_BASE64=$(echo "${CONTENT_STRING}" | base64 | tr -d '\n')
    
    # 3. Push update
    DATA=$(jq -n \
        --arg msg "chore: heartbeat update [$(date)]" \
        --arg content "${CONTENT_BASE64}" \
        --arg sha "${SHA}" \
        '{"message": $msg, "content": $content} + (if $sha != "" then {"sha": $sha} else {} end)')

    RESULT=$(curl -s -X PUT \
        -H "Authorization: token ${GITHUB_TOKEN}" \
        -H "Content-Type: application/json" \
        -d "${DATA}" \
        "${API_URL}")

    if echo "${RESULT}" | jq -e '.content.sha' > /dev/null; then
        echo "[$(date)] Heartbeat push successful."
    else
        echo "[$(date)] Heartbeat push failed: $(echo "${RESULT}" | jq -c '.')"
    fi

    echo "[$(date)] Sleeping for 2 minutes..."
    sleep 120
done
