# SkillyAPI 🎯

**SkillyAPI** is a RESTful Web API built with **ASP.NET Core** that connects service providers and users. It enables providers to showcase their skills, users to discover and message providers, and allows both parties to rate each other. The API supports authentication, skill management, messaging, order tracking, payments, and rating features, making it ideal for freelance or skill-sharing platforms.

---

## Project Overview

SkillyAPI is developed as the backend for a graduation project at the **Faculty of Computer and Artificial Intelligence**.  
The API serves as the foundation for a skill-based platform designed to:

- Help users register and authenticate securely  
- Allow service providers to manage and categorize their skills  
- Facilitate service ordering, price negotiation, and order tracking  
- Support real-time messaging and notifications  
- Enable users to rate services after completion  
- Integrate payments with discounts and points system  
- Provide an emergency request feature to prioritize urgent services  
- Provide an admin dashboard for system management

The goal is to create a scalable, maintainable, and secure backend using best practices such as **Clean Architecture**, **Repository Pattern**, and **JWT-based Authentication**.

---

## Features

- 🧑‍💼 **User Registration & Authentication**  
  - Secure signup and login using JWT tokens  
  - Role-based authorization (providers, users, admins)  

- 🧠 **Skill Management & Categorization**  
  - Providers can add, update, and categorize their skills  
  - Skill browsing and filtering for users  

- 💬 **Real-time Messaging & Negotiation**  
  - In-app chat between users and service providers  
  - Support for price negotiation within chat before order confirmation  

- ⭐ **Rating System**  
  - Users can rate and review services after delivery  
  - Aggregate ratings displayed on provider profiles  

- 🔔 **Notification System**  
  - Push and in-app notifications for messages, order status updates, ratings, and emergency alerts  

- 🎯 **Order Management & Tracking**  
  - Users can request services for themselves  
  - Track order progress and status updates in real-time  
  - Emergency request feature to prioritize urgent services  

- 💸 **Payment & Discounts System**  
  - Integrated payment processing  
  - Point-based discount system rewarding user activity and loyalty  

- 👨‍💻 **Admin Dashboard & Management**  
  - Manage users, services, orders, payments, and disputes  
  - Monitor system reports and handle emergencies  

---

## Technologies Used
- **ASP.NET Core** – For building the RESTful Web API  
- **Entity Framework Core** – For data access and ORM  
- **SQL Server** – Relational database for storing structured data  
- **JWT Authentication** – For secure user login and role-based access  
- **ASP.NET Identity** – For user and role management  
- **SignalR** – For real-time chat 
- **Firebase Cloud Messaging (FCM)** – For push notification delivery to mobile devices  
- **Paymob Payment Gateway** – For secure online payment processing  
- **Clean Architecture** – For modular, scalable project structure  
- **Repository Pattern & Unit of Work** – For maintainable and testable data access  
- **AutoMapper** – For mapping between domain models and DTOs  
- **FluentValidation** – For validating request models and inputs  
- **Swagger / Swashbuckle** – For generating interactive API documentation  
- **LINQ** – For elegant and efficient data querying  
---
