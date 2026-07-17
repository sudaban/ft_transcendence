import time
import os
import docker
import threading
from prometheus_client import start_http_server, Gauge, Counter

# Prometheus metrics
CPU_USAGE = Counter('container_cpu_usage_seconds_total', 'Total CPU time consumed in seconds', ['id', 'name', 'image'])
MEMORY_USAGE = Gauge('container_memory_usage_bytes', 'Current memory usage in bytes', ['id', 'name', 'image'])

# To keep track of previous CPU usage for correct Counter increments
# Counter can only go up, so we calculate the delta and add it.
last_cpu_usage = {}

def update_metrics(client):
    try:
        # Get all running containers
        containers = client.containers.list()
        
        for container in containers:
            try:
                # stream=False is slow per container, but acceptable for a small number of containers.
                # A more optimized version would use stream=True in background threads, but this is simple and robust.
                stats = container.stats(stream=False)
                
                c_id = f"/docker/{container.id}"
                c_name = container.name
                c_image = container.image.tags[0] if container.image.tags else container.image.id
                
                labels = {
                    'id': c_id,
                    'name': c_name,
                    'image': c_image
                }
                
                # --- CPU ---
                cpu_stats = stats.get('cpu_stats', {})
                cpu_usage_ns = cpu_stats.get('cpu_usage', {}).get('total_usage', 0)
                cpu_usage_sec = cpu_usage_ns / 1_000_000_000.0
                
                # Prometheus Counter must be incremented by delta
                if container.id in last_cpu_usage:
                    delta = cpu_usage_sec - last_cpu_usage[container.id]
                    if delta > 0:
                        CPU_USAGE.labels(**labels).inc(delta)
                else:
                    CPU_USAGE.labels(**labels).inc(cpu_usage_sec)
                
                last_cpu_usage[container.id] = cpu_usage_sec
                
                # --- Memory ---
                memory_stats = stats.get('memory_stats', {})
                mem_usage = memory_stats.get('usage', 0)
                if mem_usage > 0:
                    MEMORY_USAGE.labels(**labels).set(mem_usage)
                    
            except Exception as e:
                print(f"Error reading stats for container {container.name}: {e}")
                
    except Exception as e:
        print(f"Error communicating with Docker: {e}")

def main():
    # Start up the server to expose the metrics.
    port = int(os.environ.get("PORT", 8000))
    start_http_server(port)
    print(f"Starting docker-exporter on port {port}")
    
    # Initialize Docker client
    client = docker.from_env()
    
    # Infinite loop to update metrics
    while True:
        update_metrics(client)
        # Sleep to not overload the Docker API
        time.sleep(10)

if __name__ == '__main__':
    main()
