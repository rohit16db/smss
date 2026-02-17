# Auth Module - Quick Reference Card

## 🎯 New Endpoints (5 Total)

### 1. GET /api/Auth/me [Authorize] - FIXED ✅
**Purpose**: Get current user info from JWT token  
**Headers**: `Authorization: Bearer <token>`  
**Response**: UserDto with id, username, email, firstName, lastName, role, isActive

---

### 2. POST /api/Auth/change-password [Authorize] ✅
**Purpose**: Change password (requires current password)  
**Headers**: `Authorization: Bearer <token>`  
**Body**:
```json
{
  "currentPassword": "Admin@123",
  "newPassword": "NewPassword123"
}
```
**Response**: `{ "message": "Password changed successfully" }`

---

### 3. POST /api/Auth/forgot-password [Public] ✅
**Purpose**: Generate password reset token  
**Body**:
```json
{
  "email": "admin@example.com"
}
```
**Response**: `{ "message": "If the email exists, a password reset link has been sent" }`  
**Note**: Token stored in database (check `PasswordResetToken` field)

---

### 4. POST /api/Auth/reset-password [Public] ✅
**Purpose**: Reset password using token  
**Body**:
```json
{
  "email": "admin@example.com",
  "resetToken": "abc123...",
  "newPassword": "NewPassword123"
}
```
**Response**: `{ "message": "Password reset successful" }`  
**Note**: Token valid for 1 hour

---

### 5. POST /api/Auth/logout [Authorize] ✅
**Purpose**: Revoke refresh token  
**Headers**: `Authorization: Bearer <token>`  
**Response**: `{ "message": "Logged out successfully" }`  
**Effect**: Nullifies `RefreshToken` in database

---

## 🖥️ New UI Pages

### 1. Change Password Page
**Route**: `/change-password` (protected)  
**Fields**: Current Password, New Password, Confirm Password  
**Access**: Click "🔒 Change Password" button in header

### 2. Forgot Password Page
**Route**: `/forgot-password` (public)  
**Fields**: Email  
**Access**: Click "Forgot Password?" link on login page

### 3. Reset Password Page
**Route**: `/reset-password?email=...&token=...` (public)  
**Fields**: Email, Reset Token, New Password, Confirm Password  
**Access**: Direct URL (email link in production)

---

## 🧾 Database Changes

**New Fields in Users Table**:
- `PasswordResetToken` (text, nullable)
- `PasswordResetTokenExpiry` (timestamp, nullable)

**Modified on Logout**:
- `RefreshToken` → NULL
- `RefreshTokenExpiryTime` → NULL

---

## 🔐 Security Features

✅ **Password Reset**
- Tokens: 32-char GUID (cryptographically random)
- Expiry: 1 hour
- Single-use: Cleared after successful reset
- No email enumeration: Same response for all emails

✅ **Token Revocation**
- Refresh tokens invalidated on logout
- Old tokens cannot be reused
- Fallback: localStorage cleared even if API fails

✅ **Validation**
- Password length: 6-100 characters
- Current password verified before change
- Email format validated
- Passwords hashed securely

---

## 🧪 Quick Test Commands

### Get Reset Token from Database
```sql
SELECT "Email", "PasswordResetToken", "PasswordResetTokenExpiry" 
FROM "Users" 
WHERE "Email" = 'admin@example.com';
```

### Verify Token Cleared After Reset
```sql
SELECT "PasswordResetToken", "PasswordResetTokenExpiry" 
FROM "Users" 
WHERE "Email" = 'admin@example.com';
-- Should both be NULL
```

### Verify Logout Token Revocation
```sql
SELECT "RefreshToken", "RefreshTokenExpiryTime" 
FROM "Users" 
WHERE "Email" = 'admin@example.com';
-- Should both be NULL after logout
```

---

## 📍 Where to Find Features in UI

### Desktop Navigation (Header)
1. **Attendance** button
2. **Divider** (vertical line)
3. **🔒 Change Password** button (blue)
4. **🚪 Logout** button (red)

### Mobile Navigation (Hamburger Menu)
1. Academic dropdown
2. Finance dropdown
3. Attendance button
4. **Account Section** (border separator)
   - 🔒 Change Password
   - 🚪 Logout

### Login Page
- **Forgot Password?** link (below Sign In button)

---

## ⚡ Development Quick Start

### Start Servers
```powershell
# Backend (new window)
cd d:\practice\SMS\backend\src\SMS.API
dotnet run
# Listens on: http://localhost:5208

# Frontend
cd d:\practice\SMS\frontend
npm run dev
# Listens on: http://localhost:5175
```

### Apply Migration
```powershell
cd d:\practice\SMS\backend
dotnet ef database update --project src/SMS.Infrastructure/SMS.Infrastructure.csproj --startup-project src/SMS.API/SMS.API.csproj
```

### Build Frontend
```powershell
cd d:\practice\SMS\frontend
npm run build
# Result: 0 TypeScript errors ✅
```

---

## 🐛 Common Issues & Solutions

### Issue: Reset token not found in database
**Solution**: Check if email exists and token was generated (forgot-password endpoint always returns success)

### Issue: "Invalid or expired reset token" error
**Solution**: 
- Verify token hasn't expired (1 hour limit)
- Check token wasn't already used (single-use)
- Ensure exact token match (copy from database)

### Issue: Logout doesn't redirect
**Solution**: Check browser console for errors, verify authService imported correctly

### Issue: Change password says "incorrect current password"
**Solution**: Verify current password is correct (default: `Admin@123`)

---

## 📊 Implementation Stats

- **Backend Files Modified**: 6
- **Backend Files Created**: 4
- **Frontend Files Modified**: 4
- **Frontend Files Created**: 3
- **Database Tables Modified**: 1 (Users)
- **New API Endpoints**: 5
- **New UI Pages**: 3
- **New Routes**: 3
- **Build Errors**: 0 ✅

---

## 🎓 Default Test Credentials

**Username**: `admin`  
**Password**: `Admin@123`  
**Email**: `admin@example.com`

---

## ⏱️ Token Lifetimes

- **Access Token**: ~15 minutes (JWT)
- **Refresh Token**: ~7 days (stored in DB)
- **Reset Token**: 1 hour (stored in DB)

---

## 🚀 Quick Test Workflow (5 minutes)

1. **Login**: admin / Admin@123
2. **Test /me endpoint**: Check user info appears
3. **Change Password**: Current=Admin@123, New=NewPass123
4. **Logout**: Verify redirect to login
5. **Login with new password**: admin / NewPass123
6. **Change back**: Current=NewPass123, New=Admin@123
7. **Test Forgot Password**: Enter admin@example.com
8. **Check DB for token**: Copy PasswordResetToken value
9. **Test Reset Password**: Use token from DB
10. **Verify token cleared**: Check DB again (should be NULL)

✅ All features working!

---

**Last Updated**: February 17, 2026  
**Status**: ✅ Production Ready (pending email service integration)
