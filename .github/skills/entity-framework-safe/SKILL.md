---
name: entity-framework-safe
description: Safely add or modify Entity Framework models, relationships, DbSet registrations, OnModelCreating configuration, and migrations in ASP.NET Core apps. Use when changing schema, introducing new entities, updating foreign keys, or applying migrations without breaking seed data, repositories, or existing routes.
argument-hint: Describe the EF change (entity/relationship/migration) and any data compatibility constraints.
---

# Entity Framework Safe Change Skill

## Goal
Apply EF Core model and schema changes with minimal risk to existing data flow, repository behavior, and application startup.

## Use When
- Adding a new EF entity/table.
- Modifying entity properties, data annotations, or keys.
- Adding or changing one-to-one, one-to-many, or many-to-many relationships.
- Updating DbContext with DbSet properties or fluent mapping in OnModelCreating.
- Generating/applying migrations where existing data or seed assumptions must be preserved.

## Safety Rules
1. Never remove or rename columns/tables blindly when data may exist.
2. Prefer additive migrations first, then cleanup migrations after data has been moved.
3. Keep foreign key and navigation property pairs aligned.
4. Preserve nullable/reference-type compatibility and validation annotations.
5. Keep repository Includes/ThenIncludes consistent with relationship changes.
6. Do not change seed identifiers unexpectedly if relationships depend on fixed keys.
7. Build and verify after each structural change.

## Standard Workflow

### 1. Analyze Current Model Surface
- Identify affected entity classes and current navigation graph.
- Identify repositories/controllers/views that consume changed properties.
- Identify existing migration history and current database state.

### 2. Update Entity Classes Safely
- For new entities, define:
  - Primary key.
  - Required/optional fields.
  - Length/range constraints.
  - Navigation properties with matching FK properties.
- For changed entities:
  - Prefer adding new nullable fields before making them required.
  - Avoid destructive renames; use explicit migration rename operations if needed.
  - Keep enum changes backward compatible where possible.

### 3. Update DbContext
- Add or update DbSet<TEntity> registrations.
- Update OnModelCreating for:
  - Explicit join-table names.
  - Composite keys.
  - Delete behavior.
  - Precision/index constraints when needed.
- Keep fluent API and annotations non-conflicting.

### 4. Update Data Access Layer
- Update repository queries:
  - Include/ThenInclude paths for new relationships.
  - Ordering/filtering logic if key properties changed.
- Ensure constructor injection and service registration remain valid.

### 5. Generate Migration
- Create migration with clear name describing intent.
- Review Up/Down methods before applying:
  - Confirm FK constraints, indexes, and rename semantics.
  - Confirm no accidental drops.

### 6. Apply Migration Carefully
- Use development environment first.
- Validate schema and startup.
- Validate key routes/pages using changed entities.

### 7. Verify Compatibility
- Run build.
- Confirm app starts without DbContext resolution errors.
- Confirm repository-backed pages return data and do not null-reference on navigations.
- Confirm seed/bootstrap logic still matches new required fields.

## Relationship Checklist
- One-to-many:
  - Child has FK and reference navigation.
  - Parent has collection navigation.
- Many-to-many:
  - Both sides expose collections.
  - Join table configured or explicit join entity modeled.
- One-to-one:
  - Principal/dependent clarified.
  - Unique index/FK mapping validated.

## Migration Risk Controls
- If changing requiredness:
  - Add nullable column.
  - Backfill values.
  - Make required in later migration.
- If renaming:
  - Use migration rename operations, not drop-and-add.
- If splitting/merging tables:
  - Preserve data with SQL/backfill steps in migration.
- If seed data exists:
  - Keep stable primary keys.
  - Update dependent FK values in same migration.

## Commands Template
Run from project directory:
- dotnet build
- dotnet ef migrations add <MeaningfulName> --output-dir Migrations
- dotnet ef database update

If using startup/project split:
- dotnet ef migrations add <MeaningfulName> --project <project.csproj> --startup-project <startup.csproj> --output-dir Migrations
- dotnet ef database update --project <project.csproj> --startup-project <startup.csproj>

## Output Expectations
For every EF change, return:
1. Entities changed and why.
2. DbContext changes (DbSet and OnModelCreating).
3. Repository impact and updates.
4. Migration name and reviewed high-risk operations.
5. Post-migration verification results.

## Do Not
- Do not silently remove properties used by repositories/views.
- Do not apply destructive migration operations without explicit acknowledgment.
- Do not leave model annotations and fluent configuration contradictory.
