# AcomTracker

AcomTracker is a small accommodation management **ledger/note** app. Staff/landlords manually record payments and get a clear overview of payment history and outstanding balance per lease.



## Problem
We currently store rent payments and expenses in a spreadsheet. This approach causes recurring issues:

- **Hard to query and report:** statements per tenant/lease and monthly summaries are manual work.
- **Data quality problems:** inconsistent date formats, typos, duplicate entries, and name variations.
- **Fragile calculations:** outstanding balance depends on spreadsheet formulas and layout.
- **Not scalable:** adding more properties/units/users makes the spreadsheet increasingly error prone.

The goal is to move the core workflow into a database backed web service so data becomes structured, reliable, and scalable.


## Tech stack (current plan)
- Backend: .NET (ASP.NET Core Web API)
- Database: PostgreSQL (local dev via Docker)
- Frontend: React (after backend flows work)
- Cheap deployment (later): managed hosting for API + hosted Postgres

---

## Future work
- **PostgreSQL migrations workflow:** documented process for schema changes (migrations, rollbacks, environments).
- **Expenses module:** record operational expenses (separate entity), categories, and monthly summaries.
- **Excel import pipeline:** staging → cleaning → validation → import (e.g., Power Query/OpenRefine + CSV staging).
- **Automated monthly charges:** generate rent charges per month (Model B) instead of relying on month count logic.
- **Audit trail:** createdBy/editedBy timestamps, change history, and admin logs.
- **Roles & permissions:** Admin/Manager/ReadOnly policies.
- **Dashboards & exports:** CSV/Excel export, monthly reports, and property level KPIs.
- **Deployment & CI/CD:** GitHub Actions for build/test/deploy, environment variable management, and secrets handling.
- **Monitoring & backups:** log aggregation, health checks, and database backup/restore plan.
- **Multi property support:** multiple buildings/properties, units per property, and aggregated reporting.
