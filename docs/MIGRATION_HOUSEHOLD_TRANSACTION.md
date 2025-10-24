# Database Migration Guide - Household Transaction Linkage

## Overview
This guide explains how to apply the database changes required for the household transaction linkage feature.

## Changes Summary

### New Fields in Transaction Table
- `HouseholdId` (int, nullable) - Foreign key to Households table
- Foreign key constraint to Household table with ON DELETE SET NULL
- Index on `HouseholdId`
- Composite index on `(HouseholdId, Date)`

## Migration Steps

### For InMemory Database (Development)
The InMemory database will automatically reflect the model changes on application restart.

**No action required** - the changes will be applied automatically.

### For SQL Server / PostgreSQL (Production)

#### Step 1: Create Migration

Navigate to the Core project directory:
```bash
cd src/Privatekonomi.Core
```

Create the migration:
```bash
dotnet ef migrations add AddHouseholdToTransaction --context PrivatekonomyContext
```

This will create a new migration file in the `Migrations` folder.

#### Step 2: Review Migration

Review the generated migration file to ensure it includes:

```csharp
// Up migration
migrationBuilder.AddColumn<int>(
    name: "HouseholdId",
    table: "Transactions",
    type: "int",
    nullable: true);

migrationBuilder.CreateIndex(
    name: "IX_Transactions_HouseholdId",
    table: "Transactions",
    column: "HouseholdId");

migrationBuilder.CreateIndex(
    name: "IX_Transactions_HouseholdId_Date",
    table: "Transactions",
    columns: new[] { "HouseholdId", "Date" });

migrationBuilder.AddForeignKey(
    name: "FK_Transactions_Households_HouseholdId",
    table: "Transactions",
    column: "HouseholdId",
    principalTable: "Households",
    principalColumn: "HouseholdId",
    onDelete: ReferentialAction.SetNull);
```

#### Step 3: Apply Migration

**Development Environment:**
```bash
dotnet ef database update --context PrivatekonomyContext
```

**Production Environment (recommended approach):**

Generate SQL script for review:
```bash
dotnet ef migrations script --context PrivatekonomyContext --output migration.sql
```

Review the SQL script and apply it using your database management tools:
```sql
-- Review and execute the generated migration.sql file
-- using SQL Server Management Studio, pgAdmin, or other tools
```

#### Step 4: Verify Migration

After applying the migration, verify the changes:

```sql
-- SQL Server
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Transactions' AND COLUMN_NAME = 'HouseholdId';

-- Verify indexes
EXEC sp_helpindex 'Transactions';

-- PostgreSQL
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'Transactions' AND column_name = 'HouseholdId';

-- Verify indexes
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'Transactions';
```

## Rollback Instructions

If you need to rollback the migration:

### Remove Last Migration (before applying to database)
```bash
dotnet ef migrations remove --context PrivatekonomyContext
```

### Rollback Applied Migration
```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName --context PrivatekonomyContext

# Or generate rollback script
dotnet ef migrations script CurrentMigration PreviousMigration --context PrivatekonomyContext
```

## Data Considerations

### Existing Data
- All existing transactions will have `HouseholdId` set to NULL
- This is by design - existing transactions remain as "personal" transactions
- Users can manually assign households to existing transactions via the UI

### Performance Impact
- The new indexes will improve query performance for household-filtered queries
- Initial index creation may take a few moments on large datasets
- No negative performance impact expected

### Data Integrity
- Foreign key constraint ensures referential integrity
- If a household is deleted, related transactions will have their `HouseholdId` set to NULL
- This preserves transaction history while allowing household cleanup

## Troubleshooting

### Migration Creation Fails

**Error:** "Build failed"
**Solution:** Ensure the solution builds successfully before creating migration
```bash
dotnet build
```

### Migration Apply Fails

**Error:** "Foreign key constraint failure"
**Solution:** Ensure Households table exists and has data
```bash
# Check if Households table exists
SELECT * FROM Households;
```

**Error:** "Index creation failed - duplicate key"
**Solution:** Drop existing index if it exists
```sql
DROP INDEX IF EXISTS IX_Transactions_HouseholdId;
```

### Connection String Issues

**Error:** "Unable to connect to database"
**Solution:** Verify connection string in appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Privatekonomi;Trusted_Connection=True;"
  }
}
```

## Testing After Migration

After applying the migration, test the following:

1. **Create Transaction with Household:**
   ```
   POST /api/transactions
   {
     "description": "Test Transaction",
     "amount": 100,
     "householdId": 1,
     ...
   }
   ```

2. **Query Transactions by Household:**
   ```
   GET /api/transactions?household_id=1
   GET /api/transactions/household/1
   ```

3. **Update Transaction Household:**
   ```
   PUT /api/transactions/{id}
   {
     "householdId": 2,
     ...
   }
   ```

4. **Remove Household from Transaction:**
   ```
   PUT /api/transactions/{id}
   {
     "householdId": null,
     ...
   }
   ```

## Production Deployment Checklist

- [ ] Backup database before migration
- [ ] Test migration on staging environment
- [ ] Review generated SQL script
- [ ] Schedule migration during maintenance window
- [ ] Apply migration to production
- [ ] Verify migration completed successfully
- [ ] Test application functionality
- [ ] Monitor application logs for errors
- [ ] Have rollback plan ready

## Support

For issues or questions:
- Check application logs for detailed error messages
- Review Entity Framework migration documentation
- Open an issue in the repository with migration error details
