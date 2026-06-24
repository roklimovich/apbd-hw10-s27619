# UserPanelApp

ASP.NET Core MVC application demonstrating user registration, cookie authentication, password hashing with BCrypt, and role-based authorization.

---

## How to Run

### Prerequisites
- .NET 8 SDK

### Steps

```bash
git clone <repo-url>
cd UserPanelApp
dotnet run
```

Open your browser at `https://localhost:5001` (or `http://localhost:5000`).

The SQLite database (`userpanel.db`) is created automatically on first run. The admin user is seeded during startup.

---

## Test Accounts

### Admin (seeded automatically)
| Field    | Value              |
|----------|--------------------|
| Email    | admin@example.com  |
| Password | Admin@1234!        |

> This is a **local demo password only**. In production, set `AdminSeed:Password` via user secrets or an environment variable — never commit real credentials.

### Regular User
Register at `/Account/Register` with any email and a password of at least 8 characters.

---

## How to Log in as Admin

1. Navigate to `/Account/Login`
2. Enter `admin@example.com` / `Admin@1234!`
3. You are redirected to `/Dashboard`
4. Navigate to `/Admin` — the admin panel is visible

---

## Where Password Hashing Is

**File:** `Services/AuthService.cs`

- **Registration:** `BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12)`
- **Verification:** `BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)`

BCrypt automatically generates a unique cryptographic salt per password and embeds it in the resulting hash string. The work factor 12 means ~2^12 hashing rounds, making brute-force attacks computationally expensive.

---

## Where Authentication Is Configured

**File:** `Program.cs`

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        // ...
    });
```

After login, `AccountController.cs` creates a `ClaimsPrincipal` and calls `HttpContext.SignInAsync`. Logout calls `HttpContext.SignOutAsync`.


## Questions

**Why must passwords not be stored as plain text?**
If the database is compromised, attackers get every password immediately and can use them on other services (credential stuffing). Plain-text storage gives zero protection after a breach.

**Why is raw SHA-256 not a good choice for passwords?**
SHA-256 is a fast, general-purpose hash — it can compute billions of hashes per second on modern hardware or GPUs. Attackers can precompute tables (rainbow tables) or brute-force huge dictionaries in seconds. Password hashing requires slow, iterated algorithms (BCrypt, Argon2, scrypt) specifically designed to be computationally expensive.

**Why do we use salt?**
Salt is a unique random value added to each password before hashing. It ensures that two users with the same password produce different hashes, defeats precomputed rainbow tables, and means an attacker must crack each hash individually.

**What is the difference between salt and pepper?**
A *salt* is unique per user, stored alongside the hash in the database, and automatically managed by the hashing library. A *pepper* is a secret constant shared across all passwords, stored **outside** the database (e.g. in environment variables / user secrets). Even if an attacker steals the database, they cannot crack passwords without also knowing the pepper.

**What is the difference between authentication and authorization?**
*Authentication* answers "who are you?" — it verifies identity (login with credentials, issuing a session cookie). *Authorization* answers "what are you allowed to do?" — it checks permissions once identity is confirmed (e.g. only Admins can open `/Admin`). In ASP.NET Core: `UseAuthentication` identifies the user; `UseAuthorization` enforces `[Authorize]` policies.

**Why is hiding a link in a view not enough as security?**
The link is only HTML. Anyone can type the URL directly, send a crafted HTTP request, or use developer tools. Authorization must be enforced server-side on the controller action — that is what `[Authorize(Roles = "Admin")]` does.

**Why can a "there is no such user" login message be a problem?**
It leaks whether a given email is registered. Attackers can use this to enumerate valid accounts, then target them with phishing or credential-stuffing attacks. The safe approach (used here) is a single generic message: *"Invalid login attempt."*
