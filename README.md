# FormUp - Football Coaching API

**The backend powering FormUp’s football coaching platform.**

FormUp API is an ASP.NET Core backend for managing coaches, teams, players, training sessions and matches. It provides authenticated access to coaching data, supports retry-safe creation for mobile synchronisation and delivers scheduled match reminders through Firebase Cloud Messaging.

The repo [Coaching-Android-App](https://github.com/nafsampson/Coaching-Android-App) contains the mobile application, including the coaching interface, offline storage, tactical lineup editor and PDF exports.

## Features

- **Firebase authentication** - validates Firebase ID tokens before granting access to protected endpoints.
- **Account-scoped data** - organises coaching records under the authenticated coach’s Firebase UID.
- **Team and player management** - stores team information and player profiles.
- **Training and match management** - manages scheduled sessions, fixtures and match information.
- **Attendance recording** - stores attendance for training sessions and matches.
- **Lineups and formations** - saves squad selections, match formations and reusable formations.
- **Performance data** - stores player match statistics and provides season data to the Android app.
- **Retry-safe creation** - supports client-generated request IDs to prevent duplicate records when supported create requests are retried.
- **Match reminders** - registers device tokens, stores reminder preferences and sends notifications through Firebase Cloud Messaging.
- **Operational endpoints** - provides a startup health check and an authenticated client-capabilities endpoint.

## Technology

| Area | Technology |
| --- | --- |
| Language | C# |
| Framework | ASP.NET Core / .NET 10 |
| API style | Minimal APIs |
| Authentication | Firebase ID tokens with JWT Bearer validation |
| Database | Google Cloud Firestore |
| Push notifications | Firebase Admin SDK / Firebase Cloud Messaging |
| Local API documentation | Swagger / OpenAPI |
| Reminder scheduling | GitHub Actions |
| Hosting | MonsterASP.NET |

## Architecture

```text
FormUp Android application
          |
Firebase ID token + HTTPS request
          |
ASP.NET Core API
          |
Endpoint handlers and services
          |
Cloud Firestore
```

Reminder delivery follows a separate path:

```text
GitHub Actions schedule
          |
Secret-protected reminder endpoint
          |
Reminder service
          |
Firebase Cloud Messaging
          |
Registered Android devices
```

The Android application manages its local offline queue. The API supports subsequent synchronisation through authenticated endpoints and retry-safe creation for supported records.

## Authentication and Data Access

Protected endpoints expect a Firebase ID token:

```http
Authorization: Bearer <FIREBASE_ID_TOKEN>
```

The API validates the token’s issuer, audience, expiry and signature against the configured Firebase project.

Coaching data is scoped to the authenticated user’s Firebase UID. Backend Firestore access uses server credentials and Google Cloud IAM permissions; it does not rely on mobile Firestore security rules.

Two endpoints have separate access requirements:

- `GET /health` is publicly accessible.
- `POST /api/reminders/run` requires the configured `X-Reminder-Secret` header instead of a user token.

## API Overview

| Route | Purpose |
| --- | --- |
| `/health` | Startup health check |
| `/api/coach` | Coach information, device registration and reminder preferences |
| `/api/team` | Team information |
| `/api/players` | Player records |
| `/api/session` | Training sessions |
| `/api/match` | Matches |
| `/api/session/{id}/attendance` | Training attendance |
| `/api/match/{id}/attendance` | Match attendance |
| `/api/match/{id}/squad` | Match squad selection |
| `/api/match/{id}/formation` | Match formation |
| `/api/match/{id}/stats` | Player match statistics |
| `/api/formations` | Saved formations |
| `/api/season-data` | Coaching data used by the Android app |
| `/api/client-capabilities` | Supported client synchronisation capabilities |
| `/api/reminders/run` | Secret-protected reminder execution |

Available HTTP methods depend on the route. Request models are defined in `FormUp_API/Models/Requests`, and Swagger is available when running in the Development environment.

## Getting Started

### Requirements

- .NET 10 SDK.
- Visual Studio with ASP.NET development support, or another C# editor.
- A Firebase project with Cloud Firestore configured.
- Firebase Authentication configured for the companion Android app.
- Server credentials authorised to access the Firebase project.
- Internet connectivity for Firebase services and package restoration.

### Open and Build

Clone the repository and open `FormUp_API.slnx` in Visual Studio, or run these commands from the repository root:

```powershell
dotnet restore .\FormUp_API\FormUp_API.csproj
dotnet build .\FormUp_API\FormUp_API.csproj --configuration Release
```

### Configure the Environment

Provide the following configuration through environment variables or an appropriate secret-management mechanism:

| Variable | Purpose |
| --- | --- |
| `Firebase__ProjectId` | Firebase project ID used for token validation and database access |
| `Firebase__CredentialsJson` | Complete Firebase service-account JSON for server access |
| `Reminder__Secret` | A long, randomly generated secret protecting reminder execution |
| `ASPNETCORE_ENVIRONMENT` | `Development` locally or `Production` when hosted |

Double underscores map environment variables to nested .NET configuration keys.

`Firebase__CredentialsJson` must contain the complete service-account JSON document, not only its `private_key` or `private_key_id` field.

If explicit credentials are omitted, the Firestore service attempts to use Application Default Credentials. A valid Firebase project ID is always required.

The Android app and API must use the same Firebase project.

### Run Locally

After configuring the environment, run:

```powershell
dotnet run --project .\FormUp_API\FormUp_API.csproj --launch-profile https
```

Use the HTTPS address printed in the terminal.

In Development, API documentation is available at:

```text
https://localhost:<port>/swagger
```

Swagger is not enabled in Production.

## Health Check

Request:

```http
GET /health
```

Expected response:

```json
{
  "status": "Healthy"
}
```

This endpoint confirms that the application can serve requests. It does not verify Firestore permissions, Firebase authentication or notification delivery.

## Mobile Synchronisation Support

Supported create requests accept an optional `clientRequestId`.

When a client supplies a valid UUID:

1. The API derives a stable document ID within that coach’s collection.
2. Firestore creates the document atomically.
3. Repeating the request with the same ID returns the existing record rather than creating a duplicate.

Clients must reuse the same request ID when retrying the same operation. Requests without a client request ID retain the standard generated-ID behaviour.

The Android app can check support through:

```http
GET /api/client-capabilities
Authorization: Bearer <FIREBASE_ID_TOKEN>
```

Response:

```json
{
  "idempotentCreates": true
}
```

## Scheduled Match Reminders

The reminder workflow runs every five minutes and can also be triggered manually through GitHub Actions.

Configure these repository secrets under **Settings → Secrets and variables → Actions**:

| GitHub secret | Value |
| --- | --- |
| `REMINDER_URL` | Full HTTPS URL ending in `/api/reminders/run` |
| `REMINDER_SECRET` | The same value as the backend’s `Reminder__Secret` |

The workflow calls the API using the `X-Reminder-Secret` header. The reminder service checks eligible upcoming matches and uses delivery receipts to limit duplicate sends.

GitHub Actions schedules are best-effort and should not be treated as an exact-time delivery guarantee.

This workflow sends reminders; it does not build or deploy the API.

## Deployment

Publish a Release build:

```powershell
dotnet publish .\FormUp_API\FormUp_API.csproj --configuration Release --output .\publish
```

Deploy the published output to an ASP.NET Core host supporting .NET 10, or use Visual Studio Web Deploy.

Before running the hosted application:

1. Configure the Firebase project and server credentials.
2. Set `ASPNETCORE_ENVIRONMENT` to `Production`.
3. Configure `Reminder__Secret` if using scheduled reminders.
4. Enable HTTPS.
5. Restart the application after changing environment variables.

After deployment, check `/health`, then test authentication and database operations separately.

## Project Structure

```text
FormUp_API/
|-- Endpoints/             HTTP routes and request handling
|-- Models/                Coaching data models
|   `-- Requests/          Request payload models
|-- Services/              Firestore access, coach services,
|                          retry-safe creation and reminders
|-- Properties/            Local launch configuration
|-- Program.cs             Authentication, services and routing
|-- appsettings.json       Non-secret configuration defaults
`-- FormUp_API.csproj      Framework and package dependencies

.github/workflows/         Scheduled reminder automation
FormUp_API.slnx             Visual Studio solution
```

## Configuration and Security

Firebase service-account credentials provide privileged server access and must remain outside source control.

Never commit:

- Service-account JSON or private keys.
- Hosting passwords or deployment profiles containing credentials.
- Reminder secrets.
- Access tokens or other environment secrets.

Store server credentials in the hosting environment and workflow secrets in GitHub Actions secrets. Keep configuration files in the repository free of sensitive values.

For an independent deployment, use your own Firebase project, backend hosting and reminder configuration.

## Contributors

Nafees Sampson, Connor McClure and Thomas Newman.
