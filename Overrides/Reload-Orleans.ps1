$containerName = "mydu-server-orleans-1"

# Navigate to the directory where docker-compose.yml is located
$composeDirectory = "../../../"
Set-Location $composeDirectory

# Stop the container
Write-Host "Stopping container: $containerName"
docker-compose stop $containerName

# Remove the container (optional, this removes the stopped container but not the data volumes)
# Write-Host "Removing container: $containerName"
# docker-compose rm -f $containerName

# Start the container again
Write-Host "Starting container: $containerName"
docker-compose up -d $containerName

Write-Host "Container reload complete!"
