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

Update-Database
Run the project

## Features
User registration and login

Browse available movies and screenings

Ticket reservations

Admin panel for managing:

Movies

Screenings

Halls

Users

## Screenshots
Home Page

Ticket Reservation

Login & Registration

Admin Panel
