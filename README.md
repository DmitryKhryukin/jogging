# Jogging Traker API

* API Users are able to create an account and log in.
* All API calls are authenticated using JWT token-based authentication.
* Three roles are implemented: a regular user can only CRUD on their owned records, a user manager can CRUD only users, and an admin can CRUD all records and users.
* Each time entry when entered has a date, distance, time, and location.
* Based on the provided date and location, API connects to a weather API provider (https://darksky.net/dev) and gets the weather conditions for the run, and store that with each run.
* The API creates a report on average speed & distance per week.
* The API returns data in the JSON format.
* The API provides filter capabilities for all endpoints that return a list of elements, as well should be able to support pagination.
* The API filtering allows using parenthesis for defining operations precedence and use any combination of the available fields. The supported operations include or, and, eq (equals), ne (not equals), gt (greater than), lt (lower than). 
Example -> (date eq '2016-05-01') AND ((distance gt 20) OR (distance lt 10)).
* API covered by unit and integration/e2e tests.

* API supports versioning through URI path. Current API version: 1
* API supports Swagger for documentation/testing purposes.
* Serilog is used for logging

## Prerequisite
.NET CORE 3.1, MS SQL Server

## Open Endpoints

Open endpoints require no authentication.

* `GET api/v1/auth/token` - Get token.
* `POST /api/v1/user` - Create User - register a regular user

## Endpoints that require Authentication

Closed endpoints require a valid token to be included in the header of the request. 
`Authorization : Bearer cn389ncoiwuencr`

A token can be acquired from the Get token call above.

### Current User related

Each endpoint manipulates or displays information related to the User whose
Token is provided with the request:

To manage current user's profile:

* `GET /api/v1/users/me` - Get current user record
* `PUT /api/v1/users/me` - Update current user record
* `DELETE /api/v1/users/me` - Delete current user record

To manage current user's run record:

* `GET /api/v1/users/me/runs` - Get current user's run records
* `GET /api/v1/users/me/runs/:runId` - Get current user's run record
* `GET /api/v1/users/:userId/runs/report` - Get current user's week runs report
* `POST /api/v1/users/me/runs` - Create a run record for current user
* `PUT /api/v1/users/me/runs/:runId` - Update a run record for current user
* `DELETE /api/v1/users/me/runs/:runId` - Delete a run record for current user

### User management

Endpoints accessable for users with Admin or UserManager roles to manage user records.

* `GET /api/v1/users/:userId` - Get User Record
* `GET /api/v1/users/` - Get User Records
* `PUT /api/v1/users/:userId` - Update User Record
* `DELETE /api/v1/users/:userId` - Delete User Record
* `GET /api/v1/users/roles` - Get user role list. This endpoint required for user management.


### User run recods management

Endpoints accessable for users with Admin roles to CRUD run records.

* `GET /api/v1/users/:userId/runs` - Get user's run records
* `GET /api/v1/users/:userId/runs/report` - Get user's week runs report
* `GET /api/v1/users/:userId/runs/:runId` - Get user's run record
* `POST /api/v1/users/:userId/runs` - Create a run record for user
* `PUT /api/v1/users/:userId/runs/:runId` - Update a run record for user
* `DELETE /api/v1/users/:userId/runs/:runId` - Delete a run record for user

more information about endpoints: `https://localhost:5001/swagger/index.html`



