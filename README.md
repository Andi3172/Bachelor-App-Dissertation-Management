# Dissertation Management Web Application  

## Overview  
This project is a web-based platform designed to support the management of dissertation papers and supervision processes in higher education. It provides tools for students, professors, and administrators to streamline communication, track progress, and organize dissertation sessions in a centralized system.  

## Features  
- **Role-Based Access Control** – separate dashboards for students, professors, and administrators.  
- **Dissertation Proposal Submission** – students can submit and track their proposals.  
- **Supervision Management** – professors can oversee multiple students and provide structured feedback.  
- **Development Models** – Active, Semi-Active, and Passive project development tracking.  
- **Session Registration** – manage registration for thesis sessions.  
- **Video Call Scheduling** – option to schedule one-on-one or group calls.  
- **User Management** – administrators can manage accounts and permissions.  

## Technology Stack  
- **Frontend:** Vue 3 + TypeScript + Vuetify  
- **Backend:** ASP.NET Core (C#)  
- **Database:** PostgreSQL  

## Purpose  
This application was developed as part of my bachelor’s thesis to demonstrate the design and implementation of a lightweight, modular, and scalable dissertation management system tailored to the needs of academic institutions.  

## How to Run Locally  

### Prerequisites  
- **Node.js** (v18 or later)  
- **.NET 8 SDK** (or version used in project)  
- **PostgreSQL** (running locally or remote instance)  

### Steps  

1. **Clone the repository**  
   ```bash
   git clone https://github.com/yourusername/dissertation-management.git
   cd dissertation-management
   ```

2. **Setup the database**  
   - Create a PostgreSQL database (e.g., `dissertation_db`).  
   - Update the connection string in `appsettings.json` (Backend project).  
   - Run Entity Framework migrations:  
     ```bash
     dotnet ef database update
     ```

3. **Run the backend**  
   ```bash
   cd Backend
   dotnet run
   ```
   Backend will run on `https://localhost:5001` (default).  

4. **Run the frontend**  
   ```bash
   cd Frontend
   npm install
   npm run dev
   ```
   Frontend will run on `http://localhost:5173` (default).  

5. **Access the application**  
   Open your browser and go to `http://localhost:5173` to use the platform.  
