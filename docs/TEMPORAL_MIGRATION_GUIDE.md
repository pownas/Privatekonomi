# Creating Database Migration for Temporal Tracking

This guide explains how to create and apply the database migration for adding temporal tracking fields.

## Prerequisites

Ensure you have the Entity Framework Core tools installed:

```bash
dotnet tool install --global dotnet-ef
# or update if already installed
dotnet tool update --global dotnet-ef
```

## Creating the Migration

Navigate to the project directory and create the migration:

```bash
cd src/Privatekonomi.Core
dotnet ef migrations add AddTemporalTracking --startup-project ../Privatekonomi.Web
```

This will create a new migration file in the `Migrations` folder with:
- `Up()` method: Adds ValidFrom and ValidTo columns to temporal entities
- `Down()` method: Removes these columns (rollback)

## Migration Contents

The migration will automatically generate SQL to:

1. Add `ValidFrom` (DATETIME2, NOT NULL) column to:
   - Transactions
   - Assets
   - Loans
   - Investments
   - BankSources
   - Budgets
   - Goals
   - Pockets

2. Add `ValidTo` (DATETIME2, NULL) column to the same tables

3. Create indexes:
   - IX_Transactions_ValidFrom
   - IX_Transactions_ValidTo
   - IX_Transactions_ValidFrom_ValidTo
   - (and similar for other entities)

4. Set default values:
   - ValidFrom = CreatedAt (or current UTC time)
   - ValidTo = NULL (all existing records are current/active)

## Example Migration Code

The generated migration will look similar to this:

```csharp
public partial class AddTemporalTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add ValidFrom column (required)
        migrationBuilder.AddColumn<DateTime>(
            name: "ValidFrom",
            table: "Transactions",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        // Add ValidTo column (nullable)
        migrationBuilder.AddColumn<DateTime>(
            name: "ValidTo",
            table: "Transactions",
            type: "datetime2",
            nullable: true);

        // Create indexes
        migrationBuilder.CreateIndex(
            name: "IX_Transactions_ValidFrom",
            table: "Transactions",
            column: "ValidFrom");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_ValidTo",
            table: "Transactions",
            column: "ValidTo");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_ValidFrom_ValidTo",
            table: "Transactions",
            columns: new[] { "ValidFrom", "ValidTo" });

        // Repeat for Assets, Loans, Investments, etc.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop indexes
        migrationBuilder.DropIndex(
            name: "IX_Transactions_ValidFrom_ValidTo",
            table: "Transactions");

        migrationBuilder.DropIndex(
            name: "IX_Transactions_ValidTo",
            table: "Transactions");

        migrationBuilder.DropIndex(
            name: "IX_Transactions_ValidFrom",
            table: "Transactions");

        // Drop columns
        migrationBuilder.DropColumn(
            name: "ValidTo",
            table: "Transactions");

        migrationBuilder.DropColumn(
            name: "ValidFrom",
            table: "Transactions");

        // Repeat for other tables
    }
}
```

## Applying the Migration

### For Development (InMemory Database)
If using InMemory database, migrations are not needed. The schema is created automatically.

### For SQLite
```bash
cd src/Privatekonomi.Web
dotnet ef database update --project ../Privatekonomi.Core
```

### For SQL Server
```bash
cd src/Privatekonomi.Web
dotnet ef database update --project ../Privatekonomi.Core
```

## Verification

After applying the migration, verify the schema:

### SQLite
```bash
sqlite3 privatekonomi.db
.schema Transactions
```

### SQL Server
```sql
USE Privatekonomi;
GO

SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Transactions'
    AND COLUMN_NAME IN ('ValidFrom', 'ValidTo');

SELECT 
    name, 
    type_desc
FROM sys.indexes
WHERE object_id = OBJECT_ID('Transactions')
    AND name LIKE '%Valid%';
```

## Initializing Existing Data

After migration, all existing records will have:
- `ValidFrom` = Current UTC time (or CreatedAt if available)
- `ValidTo` = NULL (indicating they are all current/active versions)

This is correct because:
1. We're starting temporal tracking "now"
2. All existing records are the current versions
3. Future updates will create new versions with proper temporal tracking

## Troubleshooting

### Migration fails due to NULL constraint
If you get an error about NULL values in ValidFrom:

1. Ensure defaultValueSql is set: `defaultValueSql: "GETUTCDATE()"`
2. Or manually set ValidFrom before adding constraint:

```csharp
// In the migration Up() method
migrationBuilder.Sql(@"
    UPDATE Transactions 
    SET ValidFrom = COALESCE(CreatedAt, GETUTCDATE())
    WHERE ValidFrom IS NULL
");
```

### Wrong default value in SQLite
SQLite uses different function name:

```csharp
defaultValueSql: "datetime('now')"  // SQLite
defaultValueSql: "GETUTCDATE()"     // SQL Server
```

## Rolling Back

To rollback this migration:

```bash
cd src/Privatekonomi.Web
dotnet ef database update PreviousMigrationName --project ../Privatekonomi.Core
```

Or remove the migration entirely:

```bash
cd src/Privatekonomi.Core
dotnet ef migrations remove
```

## Alternative: Manual SQL Script

If you prefer to apply migrations manually:

```bash
cd src/Privatekonomi.Core
dotnet ef migrations script --startup-project ../Privatekonomi.Web --output migration.sql
```

Then review and execute the SQL script using your preferred database tool.

## Next Steps

After applying the migration:

1. Restart the application
2. Verify temporal queries work:
   ```csharp
   var current = await _context.Transactions.CurrentOnly().ToListAsync();
   var historical = await _context.Transactions.AsOf(DateTime.UtcNow.AddDays(-30)).ToListAsync();
   ```
3. Test create/update/delete operations with temporal tracking
4. Monitor database performance with the new indexes
