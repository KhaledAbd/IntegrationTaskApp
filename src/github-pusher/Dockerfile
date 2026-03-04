FROM alpine:latest

# Install curl and jq
RUN apk add --no-cache curl jq

WORKDIR /app

# Copy script and set permissions
COPY commit-script.sh .
RUN chmod +x commit-script.sh

# Run the script
ENTRYPOINT ["./commit-script.sh"]
