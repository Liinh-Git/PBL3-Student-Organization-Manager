# Auth Mappings

## Overview
This folder will contain mapping logic between domain entities and DTOs for authentication.

## Planned Mappings

### 1. User Entity → UserDto
**Purpose**: Map User domain entity to UserDto for API responses

**Mapping Rules**:
- `Id` → `Id`
- `FullName` → `FullName`
- `Email` → `Email`
- `PhoneNumber` → `PhoneNumber`
- `Dob` → `Dob`
- `Gender` → `Gender`
- `Address` → `Address`
- `AvatarUrl` → `AvatarUrl`
- `Bio` → `Bio`
- `SocialLinks` → `SocialLinks` (parse JSONB if needed)
- `Status` → `Status` (enum to string)
- `ProfileVisibility` → `ProfileVisibility` (enum to string)
- `LastLoginAt` → `LastLoginAt`
- `EmailConfirmed` → `EmailConfirmed`
- `CreatedAt` → `CreatedAt`
- `UpdatedAt` → `UpdatedAt`

**CRITICAL**: NEVER map `PasswordHash` to DTO

### 2. RegisterRequest → User Entity
**Purpose**: Map registration request to new User entity

**Mapping Rules**:
- `FullName` → `FullName`
- `Email` → `Email` (normalize to lowercase)
- `Password` → `PasswordHash` (hash before mapping)
- Set defaults:
  - `Status` = `UserStatus.Active`
  - `EmailConfirmed` = `false`
  - `ProfileVisibility` = `ProfileVisibility.Public`
  - `CreatedAt` = `DateTime.UtcNow` (handled by BaseEntity)

## Implementation Approaches

### Option 1: Manual Mapping
- Create static mapper classes
- Explicit property mapping
- Full control over mapping logic

### Option 2: AutoMapper
- Use AutoMapper library
- Configure mapping profiles
- Automatic property mapping

### Option 3: Extension Methods
- Create extension methods on entities
- `user.ToDto()` pattern
- Clean and readable

## Recommended Approach
Use **Extension Methods** for simplicity and readability in base prototype.

## NOT Implemented in Phase 3C
- ❌ No real mapping implementations
- ❌ Only README with structure notes
