#!/bin/bash
mkdir -p lib/config/routes
mkdir -p lib/config/theme
mkdir -p lib/core/constants
mkdir -p lib/core/errors
mkdir -p lib/core/network
mkdir -p lib/core/utils
mkdir -p lib/core/usecases
mkdir -p lib/di

FEATURES=(auth dashboard orders users)
for feature in "${FEATURES[@]}"; do
  mkdir -p lib/features/$feature/data/datasources
  mkdir -p lib/features/$feature/data/models
  mkdir -p lib/features/$feature/data/repositories
  mkdir -p lib/features/$feature/domain/entities
  mkdir -p lib/features/$feature/domain/repositories
  mkdir -p lib/features/$feature/domain/usecases
  mkdir -p lib/features/$feature/presentation/bloc
  mkdir -p lib/features/$feature/presentation/pages
  mkdir -p lib/features/$feature/presentation/widgets
done
echo "Architecture folders created successfully!"
