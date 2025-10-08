#!/bin/bash
docker stop lmbackend
docker rm lmbackend
docker run -v /home/tenny/models:/app/models -p 8080:8080 --name lmbackend --restart unless-stopped -d lmbackend:latest

