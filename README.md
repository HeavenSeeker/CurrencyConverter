# CurrencyConverter
This project is a currency conversion API that consumes [Frankfurter](https://frankfurter.dev/) using ASP.NET Core and .Net 9.

For now it consists of 2 layers: a webapi project and a service layer project.

# Setup instructions
1. Instal Visual studio 2022
2. Install .Net 9 sdk.
3. Install Docker.
4. Clone the repo.
5. Open visual studio and select docker-compose project as startup project from solution explorer.
6. Hit F5 (make sure docker is running).

# Navigation
- The app swagger page should be available at https://localhost:5001/swagger/index.html
- Seq should be available at http://localhost:8081/#/events?range=1d

# How to use
- First you login using *api/auth/login* endpoint using one of two users available: (username: admin@gmail.com, password: admin **OR** username: user@gmail.com, password: user). 
- You copy the access token from the login response header without the word bearer at the begining. You click authorize button at the right top angle in swagger and paste the access token.
- The currency converter controller has 3 endpoints: *GetExchangeRate*, *Convert* and *GetExchangeRateHistory*. While the last one is **ONLY** accesible for users with admin role.

# Possible future enhancements
- Add unit and integration tests.
- Add API versioning.
- Ensure the API supports deployment in multiple environments (Dev, Test, Prod) ==> *can be achieved threw configuring the env using appsettings and environment variables*.
- Implement distributed tracing

