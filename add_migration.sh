#!/bin/sh

dotnet tool restore

dotnet ef migrations add $1 \
 -p "KhCommand.Data" \
 -s "KhCommandViewer" \
 -c CommandDbContext
 