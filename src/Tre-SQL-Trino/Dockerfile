# Use a base image with Trino CLI installed
#FROM openjdk:11-jre-slim
FROM trinodb/trino:422

# Install Trino CLI
#RUN apt-get update && \
#    apt-get install -y wget && \
#    wget https://repo1.maven.org/maven2/io/trino/trino-cli/440/trino-cli-440-executable.jar -O /usr/local/bin/trino && \
#    chmod +x /usr/local/bin/trino && \
#    apt-get remove -y wget && \
#    apt-get clean

# Set working directory
WORKDIR /workspace

# Copy entrypoint script
#COPY entrypoint.sh /usr/local/bin/entrypoint.sh

# Make entrypoint script executable
#RUN chmod +x /usr/local/bin/entrypoint.sh

# Set entrypoint
#ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]


# Copy entrypoint script
COPY entrypoint.sh /home/trino/entrypoint.sh

# Make entrypoint script executable
RUN chmod +x /home/trino/entrypoint.sh

# Set entrypoint
ENTRYPOINT ["/home/trino/entrypoint.sh"]
