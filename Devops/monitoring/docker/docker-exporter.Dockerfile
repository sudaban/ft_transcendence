FROM python:3.11-alpine

WORKDIR /app

# Install dependencies
COPY Devops/monitoring/docker-exporter/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy the script
COPY Devops/monitoring/docker-exporter/exporter.py .

# Expose the prometheus port
EXPOSE 8000

# Run the exporter
CMD ["python", "exporter.py"]
