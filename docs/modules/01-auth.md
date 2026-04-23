# Module: Authentication & Users

## Overview
Handles user registration, JWT login (access + refresh tokens), password management, and user profile retrieval.

## User Roles (Enum: `UserRole`)
```csharp
Admin = 1, Accountant = 2, Clerk = 3, Teacher = 4
```

---

## Domain Entities

### User (`SMS.Domain.Entities.User` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Username | string | Required, unique |
| Email | string | Required, unique |
| PasswordHash | string | BCrypt hashed |
| FirstName | string | Required |
| LastName | string | Required |
| Role | UserRole (enum) | Admin/Accountant/Clerk/Teacher |
| IsActive | bool | Default true |
| RefreshToken | string? | JWT refresh token |
| RefreshTokenExpiryTime | DateTime? | |
| LastLoginAt | DateTime? | |
| PasswordResetToken | string? | |
| PasswordResetTokenExpiry | DateTime? | |

---

## API Endpoints

**Controller**: `AuthController` — Route: `api/auth`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login → JWT access + refresh token |
| POST | `/api/auth/refresh` | No | Refresh access token |
| GET | `/api/auth/me` | Yes | Get current user profile |
| POST | `/api/auth/change-password` | Yes | Change password |
| POST | `/api/auth/forgot-password` | No | Request password reset |
| POST | `/api/auth/reset-password` | No | Reset password with token |
| POST | `/api/auth/logout` | Yes | Revoke refresh token |

---

## CQRS Commands & Queries

### Commands (`SMS.Application.Auth.Commands.AuthCommands`)
- `RegisterCommand` → `AuthResponse` — Fields: Username, Email, Password, FirstName, LastName, Role
- `LoginCommand` → `AuthResponse` — Fields: Username, Password
- `RefreshTokenCommand` → `AuthResponse` — Fields: AccessToken, RefreshToken
- `ChangePasswordCommand` → `Unit` — Fields: UserId, CurrentPassword, NewPassword
- `ForgotPasswordCommand` → `Unit` — Fields: Email
- `ResetPasswordCommand` → `Unit` — Fields: Email, ResetToken, NewPassword
- `LogoutCommand` → `Unit` — Fields: UserId

### Queries (`SMS.Application.Features.Users.Queries.UserQueries`)
- `GetUserByIdQuery` → `UserDto` — Fields: UserId

### DTOs (`SMS.Application.Auth.DTOs.AuthDTOs`)
- `AuthResponse` — Token, RefreshToken, Expiration, User info
- `LoginRequest` — Username, Password
- `RegisterRequest` — Username, Email, Password, FirstName, LastName, Role
- `RefreshTokenRequest` — AccessToken, RefreshToken
- `ChangePasswordRequest` — CurrentPassword, NewPassword
- `ForgotPasswordRequest` — Email
- `ResetPasswordRequest` — Email, ResetToken, NewPassword

---

## File Map

### Backend
| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/User.cs` |
| Enum | `backend/src/SMS.Domain/Enums/UserRole.cs` |
| Commands | `backend/src/SMS.Application/Auth/Commands/AuthCommands.cs` |
| DTOs | `backend/src/SMS.Application/Auth/DTOs/AuthDTOs.cs` |
| Handlers | `backend/src/SMS.Application/Auth/Handlers/AuthHandlers.cs` |
| Validators | `backend/src/SMS.Application/Auth/Validators/AuthValidators.cs` |
| Queries | `backend/src/SMS.Application/Features/Users/Queries/UserQueries.cs` |
| Query Handler | `backend/src/SMS.Application/Features/Users/Handlers/UserQueryHandlers.cs` |
| Controller | `backend/src/SMS.API/Controllers/AuthController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/UserConfiguration.cs` |

### Frontend
| Layer | File |
|-------|------|
| Login Page | `frontend/src/pages/LoginPage.tsx` |
| Change Password | `frontend/src/pages/ChangePasswordPage.tsx` |
| Forgot Password | `frontend/src/pages/ForgotPasswordPage.tsx` |
| Reset Password | `frontend/src/pages/ResetPasswordPage.tsx` |
| Auth Service | `frontend/src/services/authService.ts` |
| Protected Route | `frontend/src/components/auth/ProtectedRoute.tsx` |

---

## Business Rules
- Passwords hashed with BCrypt
- JWT tokens have configurable expiry (default 8 hours)
- Refresh tokens stored in database, revoked on logout
- Forgot-password always returns success (prevents email enumeration)
- Role-based access enforced via `ProtectedRoute` component on frontend and `[Authorize]` on backend
