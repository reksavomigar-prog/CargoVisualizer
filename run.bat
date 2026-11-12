@echo off
chcp 65001 > nul
dotnet run --project src/CargoVisualizer -- %*