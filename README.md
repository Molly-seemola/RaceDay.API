# RaceDay – Portfolio of Evidence Part 2

**Module:** PROG6212  
**Project:** RaceDay – API Development  
**Student:** Molly Seemola  
**Programming Language:** C#  
**Framework:** ASP.NET Core Web API  
**Database:** Microsoft SQL Server  
**Repository:** https://github.com/Molly-seemola/RaceDay.API

---

## 1. Project Overview

RaceDay is an event management system designed for the South African road running, walking, and cycling community. The system helps event organisers manage sporting events while allowing participants to register, browse events, enter races, and view their results.

The project is divided into three parts:

- **Part 1 – Planning and Database:** Designed the Entity Relationship Diagram (ERD), planned the API endpoints, and created the SQL database script.
- **Part 2 – RESTful API:** Develops the backend using C#, connects to SQL Server, implements authentication and role-based access, and includes automated unit testing.
- **Part 3 – MVC Web Application:** Builds the website that communicates with the API, integrates Azure Blob Storage, and uses Docker for containerisation.

Part 2 focuses on building the backend engine of RaceDay. It allows the future web application to communicate with the database through API endpoints.

## 2. Objectives of Part 2

The main objectives are to:

- Develop an ASP.NET Core Web API using C#.
- Connect the API to SQL Server using Entity Framework Core.
- Use the Code-First approach to create and manage database tables.
- Implement registration, login, and session-based authentication.
- Secure passwords using hashing.
- Enforce Organiser and Participant roles.
- Implement the planned API endpoints.
- Integrate Swagger for viewing and testing endpoints.
- Write unit tests to check that the application behaves correctly.
- Configure GitHub Actions to build the project and run tests automatically.

## 3. System Roles

RaceDay supports two user roles.

### 3.1 Organiser

An Organiser manages sporting events and their information.

Organisers should be able to:

- Register and log in.
- Create, view, update, and delete their events.
- Create and manage event categories.
- View enrolments for their events.
- Record participant finishing times and positions.
- Manage relevant event information.
- View and update their own profile.

### 3.2 Participant

A Participant uses the system to find and enter sporting events.

Participants should be able to:

- Register and log in.
- Browse upcoming events.
- View event details and categories.
- Enrol in an event by selecting a category.
- View their own enrolments.
- View their own race results.
- View and update their own profile.

**Security requirement:** Participants must not be allowed to perform Organiser-only actions. The API must enforce these permissions on the server, not just hide options in the interface.

## 4. Project Architecture

RaceDay uses several components that work together.

| Component | Purpose |
|---|---|
| Models | Represent database entities such as Users, Events, and Results. |
| DTOs | Define the information accepted by and returned from API endpoints. |
| Controllers | Receive HTTP requests and return responses. |
| Services | Contain business rules and application logic, if implemented in the project. |
| DbContext | Connects Entity Framework Core to the database. |
| SQL Server | Stores the system's data. |
| Swagger | Provides an interactive interface for testing API endpoints. |
| Unit tests | Check whether application functionality behaves as expected. |
| GitHub Actions | Automates building and testing the project. |

### How the components work together

A typical request follows this process:

1. A user sends a request to an API endpoint.
2. The controller receives the request.
3. Authentication and authorisation rules are checked where required.
4. The application processes the request.
5. Entity Framework Core communicates with SQL Server when database access is needed.
6. The API returns an HTTP response to the user.

For example, when an Organiser creates an event, the API checks the user's permissions, validates the event information, saves the event to the database, and returns a response.

## 5. Technology Stack

| Technology | Purpose |
|---|---|
| C# | Main programming language |
| ASP.NET Core Web API | Builds the RESTful API |
| .NET SDK | Builds and runs the application |
| Entity Framework Core | Connects C# models to database tables |
| SQL Server | Stores RaceDay information |
| BCrypt.Net-Next | Can be used to hash passwords |
| Swagger / OpenAPI | Documents and tests API endpoints |
| xUnit | Unit testing framework |
| FluentAssertions | Makes test assertions easier to read, if used |
| Moq | Creates mock objects for tests, if used |
| GitHub Actions | Automates builds and tests |

The versions and libraries in this table should match the packages installed in the submitted solution.

## 6. Database Integration

Part 2 builds on the database design from Part 1.

The planned database contains the following eight entities:

1. Users
2. Organisers
3. Participants
4. Events
5. Categories
6. Enrolments
7. Results
8. Weather

Entity Framework Core is used to connect the C# application to SQL Server.

### What is the Code-First approach?

Code-First means defining the database structure in C# models and then using Entity Framework Core migrations to create or update the database schema.

For example:

- A User model represents user information.
- An Event model represents sporting events.
- An Enrolment model records which participant entered an event category.

The database schema must remain consistent with the approved Part 1 ERD and SQL script. Any differences should be explained.

### Creating the database

If the project is configured to use Entity Framework Core migrations, the following commands can be run through Visual Studio's Package Manager Console:

```powershell
Add-Migration InitialCreate
Update-Database
```

Run these commands only if the project does not already have the required migration. If migrations already exist, use the appropriate existing migration instead of creating a duplicate.

## 7. Authentication and Security

Authentication identifies the user making a request. Authorisation determines what that user is allowed to do.

### Registration and login

The planned authentication process is:

1. A new user registers with their personal details and role.
2. The application validates the submitted information.
3. The password is hashed before being stored.
4. The user logs in using their email and password.
5. The application creates a session containing the authenticated user's identity and role.
6. Protected endpoints check the session and the user's permissions.

Passwords must never be stored as plain text.

### Role-based access

Examples of the expected access rules include:

- An Organiser can create and manage permitted events.
- A Participant can browse events and enrol in a category.
- An Organiser can capture results for their events.
- A Participant can view their own results.
- An unauthenticated user cannot access protected functionality.

A request made by an authenticated user without the required permission should normally return `403 Forbidden`. A request without valid authentication should normally return `401 Unauthorized`.

The final implementation must be checked to confirm that sessions, password hashing, and all role restrictions work correctly.

## 8. API Endpoints

The API exposes resources through HTTP methods. The routes below summarise the intended functionality; the final list must match the actual controller routes in the submitted code.

### 8.1 Authentication and profiles

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Log in and establish a session |
| GET | `/api/auth/me` | View the current user's profile |
| PUT | `/api/auth/me` | Update the current user's profile |
| POST | `/api/auth/logout` | End the current session |

### 8.2 Events

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/events` | View available events |
| GET | `/api/events/{id}` | View a specific event |
| GET | `/api/events/upcoming` | View upcoming events |
| GET | `/api/events/search?q=` | Search for events |
| POST | `/api/events` | Create an event |
| PUT | `/api/events/{id}` | Update an event |
| DELETE | `/api/events/{id}` | Delete an event |

Creating, updating, and deleting events should be restricted to authorised Organisers.

### 8.3 Categories

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/events/{eventId}/categories` | View an event's categories |
| POST | `/api/events/{eventId}/categories` | Add a category |
| PUT | `/api/categories/{id}` | Update a category |
| DELETE | `/api/categories/{id}` | Delete a category |

### 8.4 Enrolments

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/enrolments` | View enrolments permitted for the current role |
| POST | `/api/events/{eventId}/enrol` | Enter an event category |
| PUT | `/api/enrolments/{id}/cancel` | Cancel an enrolment |

Participants should only be able to manage their own enrolments, while Organisers should be able to view enrolments belonging to their events.

### 8.5 Results

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/results` | View results permitted for the current role |
| POST | `/api/results` | Record a participant's result |
| PUT | `/api/results/{id}` | Update a result |
| GET | `/api/participants/{id}/results` | Retrieve the permitted participant results |

Organisers should be restricted to recording or updating results for their events. Participants should only be able to access their own results.

### 8.6 Weather

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/weather/event/{eventId}` | View weather information for an event |
| POST | `/api/weather/event/{eventId}` | Add weather information |

The endpoint list must be reconciled with the approved Part 1 API endpoint plan before submission.

## 9. Swagger Integration

Swagger provides an interactive page where developers can inspect and test the API.

To use Swagger:

1. Start the API project in Visual Studio.
2. Check the local URL shown when the application starts.
3. Open the Swagger URL displayed by the application, commonly `/swagger`.
4. Expand an endpoint to view its request format and expected responses.
5. Select **Try it out**.
6. Enter the required information.
7. Select **Execute**.
8. Review the HTTP status code and returned response.

For endpoints requiring authentication, the correct session cookie or authentication mechanism must be established first.

Swagger should document the endpoints, their inputs, their purposes, and their expected responses.

## 10. Unit Testing

Unit testing checks whether individual parts of an application behave correctly.

For RaceDay, tests should cover both successful operations and situations where the application must reject a request.

### Areas to test

**Authentication**
- Successful registration.
- Registration with invalid information.
- Duplicate email addresses.
- Successful login.
- Incorrect login credentials.
- Password hashing.

**Event management**
- An Organiser can create an event.
- An Organiser can update or delete an event they are permitted to manage.
- A Participant cannot perform Organiser-only operations.
- An unauthenticated user cannot perform protected operations.

**Enrolments**
- A Participant can enrol in a valid event category.
- The enrolment is correctly saved.
- Duplicate enrolments are rejected where prohibited.
- Invalid event or category references are rejected.
- Users cannot access another participant's private enrolment information.

**Results**
- An authorised Organiser can record results.
- A Participant can access their own results.
- Unauthorised result operations are rejected.

### Running the tests

In Visual Studio:

1. Open the solution.
2. Select **Test → Run All Tests**.
3. Open **Test Explorer**.
4. Review the number of passed, failed, and skipped tests.

Alternatively, run the following command from the directory containing the solution:

```bash
dotnet test
```

The submitted test count and test results must reflect the actual output. A successful test run should be demonstrated in the video presentation.

## 11. GitHub Actions and CI/CD

GitHub Actions can automatically build the solution and run unit tests whenever code is pushed to the repository.

The workflow should be stored in:

`.github/workflows/dotnet.yml`

The workflow should perform the following steps:

1. Check out the repository.
2. Install the required .NET SDK.
3. Restore NuGet packages.
4. Build the solution.
5. Run the unit tests.
6. Report whether the workflow succeeded or failed.

A green check mark indicates that the configured workflow completed successfully. It does not, by itself, prove that every application requirement has been implemented.

### Green build screenshot

Add a screenshot of the successful Part 2 GitHub Actions run to the repository, for example:

`docs/ci-cd-screenshot-part2.png`

Once the image exists in the repository, include it here:

`![Successful Part 2 CI/CD build](docs/ci-cd-screenshot-part2.png)`

The screenshot must be a real screenshot of the successful workflow.

## 12. Setup and Run Instructions

### Prerequisites

Install the tools required by the actual solution:

- Visual Studio 2022 or 2026, with the ASP.NET and web development workload.
- The .NET SDK version targeted by the project.
- SQL Server or SQL Server Express.
- SQL Server Management Studio (SSMS).
- Git.

### Step 1: Clone the repository

Open a terminal and run:

```bash
git clone https://github.com/Molly-seemola/RaceDay.API
cd missing
```

### Step 2: Open the solution

Open Visual Studio and select **Open a project or solution**.

Locate the RaceDay solution file in the repository. If the solution is stored at `RaceDay.API/RaceDay.API.sln`, open that file.

### Step 3: Configure the database

Locate the API project's `appsettings.json` file and configure the database connection string for your local SQL Server instance.

Example for a local SQL Server Express installation:

```json
{
   "ConnectionStrings": {
   "DefaultConnection": "Server=MOLLY_SEEMOLA\\SQLEXPRESS;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
 }
}
```

Replace `YOUR_SERVER_NAME\\SQLEXPRESS` with the actual SQL Server instance name. The connection string key must match the one configured in the application.

Do not commit real passwords, production credentials, or other secrets to GitHub.

### Step 4: Restore packages

From the solution directory, run:

```bash
dotnet restore
```

### Step 5: Apply database migrations

If the project uses Entity Framework Core migrations, apply the existing migrations using the appropriate project and startup project configuration.

For a simple solution, the command may be:

```bash
dotnet ef database update
```

If the `dotnet ef` command is unavailable, install or configure the required Entity Framework Core tools and packages.

### Step 6: Build the solution

Run:

```bash
dotnet build
```

Resolve any compilation errors before continuing.

### Step 7: Run the API

Run the API project from Visual Studio or use the appropriate project path:

```bash
dotnet run --project RaceDay.API/RaceDay.API.Server
```

Adjust the path if your project structure differs.

### Step 8: Open Swagger

Use the application URL displayed in the terminal or Visual Studio output. Open its Swagger page, usually:
http://localhost:5000/swagger/index.html


Replace `<port>` with the actual port assigned to your application.

You can now inspect and test the available endpoints.

## 13. Repository Structure

The intended repository structure may look like this:

```text
Race-dayDB-PROG-1/
│
├── RaceDay.API/
│   ├── RaceDay.API.Server/
│   │   ├── Controllers/
│   │   ├── Data/
│   │   ├── DTOs/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── RaceDay.Tests/
│   │   ├── AuthServiceTests.cs
│   │   ├── EventServiceTests.cs
│   │   ├── EnrolmentServiceTests.cs
│   │   └── TestDbContextFactory.cs
│   │
│   └── RaceDay.API.sln
│
├── docs/
│   ├── ERD.png
│   ├── API_Endpoint_Plan.md
│   ├── RaceDay_Schema.sql
│   └── ci-cd-screenshot-part2.png
│
├── .github/
│   └── workflows/
│       ├── validate-docs.yml
│       └── dotnet.yml
│
└── README.md
```

This is an example structure. Keep the names and folders consistent with the files that actually exist in your repository.

## 14. GitHub Commit Requirements

Part 2 requires at least **20 meaningful commits** made through your own GitHub account.

Commits should represent real development progress, such as:

- Creating the API project.
- Adding database models.
- Configuring Entity Framework Core.
- Implementing authentication.
- Adding role-based access restrictions.
- Creating event endpoints.
- Creating category endpoints.
- Implementing enrolments.
- Implementing results.
- Integrating Swagger.
- Adding unit tests.
- Fixing test failures.
- Configuring GitHub Actions.
- Updating setup documentation.

Each commit should describe the change made. Do not create artificial commits just to increase the count.

## 15. Video Presentation

An unlisted YouTube video must be included in the README.

The presentation should demonstrate and explain:

1. The RaceDay project structure.
2. How the API connects to SQL Server.
3. How registration and login work.
4. How sessions and roles are handled.
5. How the endpoints work in Swagger.
6. How Organiser and Participant permissions are enforced.
7. How the unit tests are executed.
8. How the GitHub Actions workflow builds and tests the solution.

The video must show the running application and include your own voice explaining the code and design decisions. AI-generated voices are not permitted.

**Video link:** https://youtu.be/0RHx9yq1Cw0

## 16. Part 2 Submission Checklist

- [ ] ASP.NET Core Web API builds successfully.
- [ ] Entity Framework Core is configured.
- [ ] SQL Server database connection works.
- [ ] Database schema matches the approved Part 1 design.
- [ ] Registration and login work.
- [ ] Passwords are hashed.
- [ ] Sessions and role-based access are enforced.
- [ ] Required API endpoints are implemented.
- [ ] Swagger endpoints can be inspected and tested.
- [ ] Unit tests run successfully.
- [ ] GitHub Actions builds the solution and runs the tests.
- [ ] At least 20 meaningful commits are present.
- [ ] README includes setup and run instructions.
- [ ] A real green-build screenshot is included.
- [ ] An unlisted YouTube video link is included.
- [ ] The GitHub repository link is submitted through ARC.

## 17. Conclusion

Part 2 focuses on developing the backend of the RaceDay event management system. It builds on the database design and API plan from Part 1 and prepares the system for the MVC web application in Part 3.

The API provides the functionality needed to manage users, events, categories, enrolments, and results. Entity Framework Core supports database operations, authentication and role-based access protect restricted functionality, Swagger supports API testing, and unit tests and GitHub Actions help verify the application during development.

The final implementation must be built, tested, and demonstrated before the project is submitted.

## 18. References

- Microsoft. *ASP.NET Core documentation*. https://learn.microsoft.com/aspnet/core/
- Microsoft. *Entity Framework Core documentation*. https://learn.microsoft.com/ef/core/
- Microsoft. *.NET documentation*. https://learn.microsoft.com/dotnet/
- OpenAPI Initiative. *OpenAPI Specification*. https://spec.openapis.org/oas/latest.html
- xUnit. *Documentation*. https://xunit.net/
- GitHub. *GitHub Actions documentation*. https://docs.github.com/actions

---

**Project:** RaceDay – PROG6212 PoE Part 2  
**Student:** Molly Seemola  
**Repository:** https://github.com/Molly-seemola/RaceDay.API
