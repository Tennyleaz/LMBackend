#!/bin/bash
docker stop lmbackend
docker rm lmbackend
docker run -v /home/tenny/models:/app/models -v /home/tenny/lmbakcend_wwwroot:/app/wwwroot -p 8080:8080 --name lmbackend --restart unless-stopped -d lmbackend:latest

