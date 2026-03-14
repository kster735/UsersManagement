# UsersManagement API

## Introduction

This is a simple **authenticated Bearer-token API** for managing users. It provides endpoints to:

- Register a user (`/auth/register`)
- Login and obtain a Bearer token (`/auth/login`) also sending a cookie for SPA (Single Page Application) sessions
- Use the token to access protected endpoints
- Checking for cookies for SPA sessions

After login, the user receives a token that must be sent in the `Authorization: Bearer <token>` header for protected routes.

### Rules

- Anyone can view the list of users (`GET /users`).
- A user can only **update** or **delete** their own profile.

## Architecture Overview

### Middleware

- **Auth middleware** (`Middlewares/AuthMiddleware.cs`) checks for a valid bearer token or API key for non-public routes.
- **Scalar middleware** (via `Scalar.AspNetCore` and `app.MapScalarApiReference()`) exposes a built-in API reference UI at `/scalar`.

### Logging

- Uses **NLog** (configured via `NLog.config`) for structured logging and error tracking.

### Validation

- Models are validated using extension methods (e.g., `User.Validate()`) and schema validation.

## Getting Started

### 1) Clone

```bash
git clone <repo-url>
cd UsersManagement
```

### 2) Setup / Restore

Restore NuGet packages:

```bash
dotnet restore
```

### 3) Run the app

```bash
dotnet run
```

By default, the app listens on `http://localhost:5094` (and HTTPS on `https://localhost:7247` if enabled).

### 4) Test with Postman

1. Import `UserManagmentApiCoursera.postman_collection.json` into Postman.
2. Register a user (`/auth/register`).
3. Login (`/auth/login`) and copy the `token` from the response.
4. In Postman, set a `Bearer Token` in the Authorization tab (use the token value).
5. Call protected endpoints like `/users`, `/users/{id}`, `/auth/me`.

### 5) Test with REST Client (`UsersManagement.http`)

1. Open `UsersManagement.http` in VS Code.
2. Run the requests in order (register, login, etc.).
3. After login, copy the token and set the `Authorization: Bearer {{token}}` header in subsequent requests.

### 6) Open Scalar API Reference UI

Scalar is exposed in development mode at:

- `http://localhost:5094/scalar`

If Scalar is working, you should see a UI that allows you to explore the API and send requests.
With scalar, you have to add the `Authorization: Bearer <token>` header manuall to access protected endpoints.
Some endpoints need the user.Id as a url parameter, you can get it from the token payload or from the response of the `/auth/login` or `/auth/me` endpoint.payload

---

## Notes

- The API uses an **in-memory user store**, so restarting the app clears users and tokens.
- Token expiration is currently set to **1 hour**.
- If you want to bypass authentication for testing, you can modify the middleware logic in `Middlewares/AuthMiddleware.cs`.

---

## Useful Files

- `Program.cs` - app startup and route definitions
- `Middlewares/AuthMiddleware.cs` - token validation logic
- `NLog.config` - logging configuration
- `UsersManagement.http` - REST Client test requests
- `UserManagmentApiCoursera.postman_collection.json` - Postman collection
