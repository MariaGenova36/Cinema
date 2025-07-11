# Cinema

**Cinema** is a simple ASP.NET Core MVC web application for managing movies and genres, including features for uploading movie posters and searching movies by title.

## Features

- CRUD operations for Movies and Genres
- Upload and display movie posters
- Search movies by title
- Uses Entity Framework Core with SQL Server LocalDB
- Responsive UI with Bootstrap

## Technologies

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core
- SQL Server LocalDB
- Bootstrap 5

## Usage

- Navigate to Movies to add, edit, delete movies.

- Upload poster images when creating or editing a movie.

- Use the search bar to filter movies by title.

- Manage genres under the Genres section.

## Project Structure

 Models — Contains data models (Movie, Genre, etc.)

 Controllers — MVC controllers for handling requests

 Views — Razor views for UI

 wwwroot/images — Folder for uploaded movie poster images
 
 Data — Contains ApplicationDbContext for database access

