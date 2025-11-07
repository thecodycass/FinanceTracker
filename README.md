# FinanceTracker
(_This is my first C# project using .NET core functionality_)

A comprehensive bill tracking and management application built with ASP.NET Core, designed to help me manage my recurring and one-time financial obligations efficiently.

## Features

### Bill Management
- **Create, Read, Update, Delete (CRUD)** operations for bills
- Support for different bill types:
  - **Weekly**: Bills that recur every 7 days
  - **Monthly**: Bills that recur every month
  - **Partial**: One-time bills that get archived after the due date
  - **Cancelled**: Bills that are no longer active
- Comprehensive bill information including name, amount, due date, description, and status

### Financial Dashboard
- **Interactive Charts**: Visual representation of bills by amount using ApexCharts
- **Financial Summaries**:
  - Weekly spending overview
  - Monthly projections
  - Yearly cost calculations
  - Bills due this week and next week
- **Smart Calculations**: Automatically adjusts calculations based on bill frequency (weekly bills are multiplied appropriately for monthly/yearly views)

### Notification System
The application includes a robust notification system that sends reminders via Discord webhooks:

- **Day-Before Notifications**: Optional reminders sent one day before a bill is due
- **Day-Of Notifications**: Optional reminders sent on the day a bill is due
- **Smart Tracking**: The system tracks which notifications have been sent to avoid duplicate messages
- **Automatic Reset**: Notification tracking is reset when bill due dates are rotated

### Automated Background Processes

#### Notifier Worker
- Runs daily to check for upcoming bill due dates
- Sends Discord webhook notifications for bills with notifications enabled
- Tracks sent notifications to prevent duplicates
- Includes bill details (name, amount, due date) in notification embeds

#### Due Date Rotation Worker
- Automatically rotates due dates for recurring bills that were due yesterday
- **Weekly Bills**: Adds 7 days to the due date
- **Monthly Bills**: Adds 1 month to the due date
- **Partial Bills**: Archives the bill since they are one-time payments
- **Notification Cleanup**: Deletes notification tracking records when due dates rotate, allowing fresh notifications for the new due date

### API Endpoints
The application provides RESTful API endpoints for programmatic access:
- `GET /Bills` - Retrieve all bills
- `GET /Bills/{id}` - Retrieve a specific bill
- `POST /Bills` - Create a new bill
- `PUT /Bills/{id}` - Update an existing bill

### Technical Features
- **Database**: SQLite with Entity Framework Core
- **Frontend**: ASP.NET Core MVC with Razor views
- **Styling**: Tailwind CSS for responsive design
- **Charts**: ApexCharts for data visualization
- **API Documentation**: Swagger
- **Background Services**: Hosted services for automated tasks

## Getting Started

### Prerequisites
- .NET 8.0 or later
- SQLite (included with .NET)

### Installation
1. Clone the repository
2. Navigate to the project directory
3. Run the application:
   ```bash
   dotnet run
   ```
4. Access the application at `https://localhost:5001`

### Configuration
Configure the Discord webhook URL in `appsettings.json`:
```json
{
  "Discord": {
    "WebhookUrl": "your-discord-webhook-url-here"
  }
}
```

## Architecture

### Database Schema
- **Bills**: Main entity containing bill information
- **Notifications**: Tracks sent notifications to prevent duplicates

### Background Services
- **NotifierWorker**: Handles Discord notifications
- **RotateDueDateWorker**: Manages automatic due date rotation and archiving

### Controllers
- **HomeViewController**: Main dashboard
- **BillViewController**: Bill management interface
- **Bills API Controller**: RESTful API endpoints

## Usage

### Managing Bills
1. Navigate to the Bills section
2. Create new bills with appropriate status (Weekly/Monthly/Partial)
3. Enable notifications for important bills
4. View financial summaries and upcoming due dates

### Understanding Notifications
- Enable "Notify Day Before" to receive reminders 24 hours before due date
- Enable "Notify On Day" to receive reminders on the due date
- Notifications are sent via Discord webhooks with rich embeds containing bill details
- The system automatically resets notification tracking when due dates rotate

### Bill Lifecycle
- **Recurring Bills** (Weekly/Monthly): Due dates automatically rotate after the due date passes
- **One-time Bills** (Partial): Automatically archived after the due date, preserving historical data
- **Cancelled Bills**: Remain in the system but excluded from calculations and notifications

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License.
