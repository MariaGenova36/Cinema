# Cinema - ASP.NET MVC Project

## Description
Cinema is a web application for managing movie screenings and ticket reservations.  
The project is built with **ASP.NET MVC** and **Entity Framework**, using **SQL Server** as a database.  

Its goal is to demonstrate building a complete MVC web application with database integration, user authentication, and CRUD functionalities.

---

## Technologies
- C# / ASP.NET MVC
- Entity Framework (Code First)
- SQL Server
- HTML, CSS, JavaScript, Bootstrap
- Visual Studio + IIS Express

---

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/MariaGenova36/Cinema.git
   
2. Open in Visual Studio

3. Open the solution file Cinema.sln.

4. Database setup

5. Update the connection string in Web.config if needed:

<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=CinemaDB;Integrated Security=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
    
6. Run the migrations:

- Update-Database
- Run the project

## Features
1. User registration and login

2. Browse available movies and screenings

3. Ticket reservations

4. Admin panel for managing:

- Movies

- Screenings

- Halls

- Users

5. Account

6. Staff Panel for Validating Tickets by scanning the qr from the ticket
   
## Screenshots

### Home Page
![Home Page](screenshots/Screenshot_2025-09-29_081834.png)

### Privacy Page
![Privacy Page](screenshots/Screenshot_2025-09-29_081922.png)

### About Us Page
![About Us Page 1](screenshots/Screenshot_2025-09-29_081946.png)
![About Us Page 2](screenshots/Screenshot_2025-09-29_081959.png)
![About Us Page 3](screenshots/Screenshot_2025-09-29_082015.png)

### Movies Page
![Movies Page](screenshots/Screenshot_2025-09-29_082041.png)

### Projections Page
![Projections Page 1](screenshots/Screenshot_2025-09-29_082058.png)
![Projections Page 2](screenshots/Screenshot_2025-09-29_082122.png)

### Admin Panel

![Admin Panel 1](screenshots/Screenshot_2025-09-29_082146.png)
![Admin Panel 2](screenshots/Screenshot_2025-09-29_082212.png)
![Admin Panel 3](screenshots/Screenshot_2025-09-29_082224.png)
![Admin Panel 4](screenshots/Screenshot_2025-09-29_082247.png)
![Admin Panel 5](screenshots/Screenshot_2025-09-29_082310.png)
![Admin Panel 6](screenshots/Screenshot_2025-09-29_082355.png)

### Login & Registration

![Login & Registration](screenshots/Screenshot_2025-09-29_082422.png)

### Account

![Account](screenshots/Screenshot_2025-09-29_082452.png)

### Ticket Validation

![Ticket Validation](screenshots/Screenshot_2025-09-29_082530.png)
