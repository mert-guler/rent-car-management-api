using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarManagement.Infrastructure.Data.Migrations;

/// <summary>Converts legacy integer keys to GUIDs while retaining all existing rows and relationships.</summary>
public partial class ConvertEntityKeysToGuid : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Keep each stage in a separate SQL command. SQL Server compiles a whole batch
        // before running it, so later statements cannot safely reference columns added
        // earlier in that same batch.
        migrationBuilder.Sql("ALTER TABLE [Rentals] ADD [NewId] uniqueidentifier NULL, [NewUserId] uniqueidentifier NULL, [NewVehicleId] uniqueidentifier NULL;");
        migrationBuilder.Sql("ALTER TABLE [Users] ADD [NewId] uniqueidentifier NULL;");
        migrationBuilder.Sql("ALTER TABLE [Vehicles] ADD [NewId] uniqueidentifier NULL;");
        migrationBuilder.Sql("ALTER TABLE [DamageReports] ADD [NewId] uniqueidentifier NULL, [NewRentalId] uniqueidentifier NULL;");
        migrationBuilder.Sql("ALTER TABLE [SupportRequests] ADD [NewId] uniqueidentifier NULL;");
        migrationBuilder.Sql("ALTER TABLE [PasswordResetTokens] ADD [NewId] uniqueidentifier NULL, [NewUserId] uniqueidentifier NULL;");

        migrationBuilder.Sql("UPDATE [Users] SET [NewId] = NEWID();");
        migrationBuilder.Sql("UPDATE [Vehicles] SET [NewId] = NEWID();");
        migrationBuilder.Sql("UPDATE [Rentals] SET [NewId] = NEWID();");
        migrationBuilder.Sql("UPDATE [DamageReports] SET [NewId] = NEWID();");
        migrationBuilder.Sql("UPDATE [SupportRequests] SET [NewId] = NEWID();");
        migrationBuilder.Sql("UPDATE [PasswordResetTokens] SET [NewId] = NEWID();");
        migrationBuilder.Sql("UPDATE r SET [NewUserId] = u.[NewId], [NewVehicleId] = v.[NewId] FROM [Rentals] r JOIN [Users] u ON u.[Id] = r.[UserId] JOIN [Vehicles] v ON v.[Id] = r.[VehicleId];");
        migrationBuilder.Sql("UPDATE d SET [NewRentalId] = r.[NewId] FROM [DamageReports] d JOIN [Rentals] r ON r.[Id] = d.[RentalId];");
        migrationBuilder.Sql("UPDATE p SET [NewUserId] = u.[NewId] FROM [PasswordResetTokens] p JOIN [Users] u ON u.[Id] = p.[UserId];");

        migrationBuilder.Sql("ALTER TABLE [DamageReports] DROP CONSTRAINT [FK_DamageReports_Rentals_RentalId];");
        migrationBuilder.Sql("ALTER TABLE [PasswordResetTokens] DROP CONSTRAINT [FK_PasswordResetTokens_Users_UserId];");
        migrationBuilder.Sql("ALTER TABLE [Rentals] DROP CONSTRAINT [FK_Rentals_Users_UserId];");
        migrationBuilder.Sql("ALTER TABLE [Rentals] DROP CONSTRAINT [FK_Rentals_Vehicles_VehicleId];");
        migrationBuilder.Sql("DROP INDEX [IX_DamageReports_RentalId] ON [DamageReports];");
        migrationBuilder.Sql("DROP INDEX [IX_PasswordResetTokens_TokenHash] ON [PasswordResetTokens];");
        migrationBuilder.Sql("DROP INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens];");
        migrationBuilder.Sql("DROP INDEX [IX_Rentals_UserId] ON [Rentals];");
        migrationBuilder.Sql("DROP INDEX [IX_Rentals_VehicleId] ON [Rentals];");
        migrationBuilder.Sql("DROP INDEX [IX_Users_Email] ON [Users];");
        migrationBuilder.Sql("DROP INDEX [IX_Users_NationalIdNumber] ON [Users];");
        migrationBuilder.Sql("ALTER TABLE [DamageReports] DROP CONSTRAINT [PK_DamageReports];");
        migrationBuilder.Sql("ALTER TABLE [PasswordResetTokens] DROP CONSTRAINT [PK_PasswordResetTokens];");
        migrationBuilder.Sql("ALTER TABLE [Rentals] DROP CONSTRAINT [PK_Rentals];");
        migrationBuilder.Sql("ALTER TABLE [SupportRequests] DROP CONSTRAINT [PK_SupportRequests];");
        migrationBuilder.Sql("ALTER TABLE [Users] DROP CONSTRAINT [PK_Users];");
        migrationBuilder.Sql("ALTER TABLE [Vehicles] DROP CONSTRAINT [PK_Vehicles];");

        migrationBuilder.Sql("ALTER TABLE [DamageReports] DROP COLUMN [Id], [RentalId];");
        migrationBuilder.Sql("ALTER TABLE [PasswordResetTokens] DROP COLUMN [Id], [UserId];");
        migrationBuilder.Sql("ALTER TABLE [Rentals] DROP COLUMN [Id], [UserId], [VehicleId];");
        migrationBuilder.Sql("ALTER TABLE [SupportRequests] DROP COLUMN [Id];");
        migrationBuilder.Sql("ALTER TABLE [Users] DROP COLUMN [Id];");
        migrationBuilder.Sql("ALTER TABLE [Vehicles] DROP COLUMN [Id];");

        migrationBuilder.Sql("EXEC sp_rename 'Users.NewId', 'Id', 'COLUMN';");
        migrationBuilder.Sql("EXEC sp_rename 'Vehicles.NewId', 'Id', 'COLUMN';");
        migrationBuilder.Sql("EXEC sp_rename 'Rentals.NewId', 'Id', 'COLUMN';");
        migrationBuilder.Sql("EXEC sp_rename 'Rentals.NewUserId', 'UserId', 'COLUMN';");
        migrationBuilder.Sql("EXEC sp_rename 'Rentals.NewVehicleId', 'VehicleId', 'COLUMN';");
        migrationBuilder.Sql("EXEC sp_rename 'DamageReports.NewId', 'Id', 'COLUMN';");
        migrationBuilder.Sql("EXEC sp_rename 'DamageReports.NewRentalId', 'RentalId', 'COLUMN';");
        migrationBuilder.Sql("EXEC sp_rename 'SupportRequests.NewId', 'Id', 'COLUMN';");
        migrationBuilder.Sql("EXEC sp_rename 'PasswordResetTokens.NewId', 'Id', 'COLUMN';");
        migrationBuilder.Sql("EXEC sp_rename 'PasswordResetTokens.NewUserId', 'UserId', 'COLUMN';");

        migrationBuilder.Sql("ALTER TABLE [Users] ALTER COLUMN [Id] uniqueidentifier NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE [Vehicles] ALTER COLUMN [Id] uniqueidentifier NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE [Rentals] ALTER COLUMN [Id] uniqueidentifier NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE [Rentals] ALTER COLUMN [UserId] uniqueidentifier NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE [Rentals] ALTER COLUMN [VehicleId] uniqueidentifier NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE [DamageReports] ALTER COLUMN [Id] uniqueidentifier NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE [DamageReports] ALTER COLUMN [RentalId] uniqueidentifier NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE [SupportRequests] ALTER COLUMN [Id] uniqueidentifier NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE [PasswordResetTokens] ALTER COLUMN [Id] uniqueidentifier NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE [PasswordResetTokens] ALTER COLUMN [UserId] uniqueidentifier NOT NULL;");

        migrationBuilder.Sql("ALTER TABLE [Users] ADD CONSTRAINT [PK_Users] PRIMARY KEY ([Id]);");
        migrationBuilder.Sql("ALTER TABLE [Vehicles] ADD CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id]);");
        migrationBuilder.Sql("ALTER TABLE [Rentals] ADD CONSTRAINT [PK_Rentals] PRIMARY KEY ([Id]);");
        migrationBuilder.Sql("ALTER TABLE [DamageReports] ADD CONSTRAINT [PK_DamageReports] PRIMARY KEY ([Id]);");
        migrationBuilder.Sql("ALTER TABLE [SupportRequests] ADD CONSTRAINT [PK_SupportRequests] PRIMARY KEY ([Id]);");
        migrationBuilder.Sql("ALTER TABLE [PasswordResetTokens] ADD CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id]);");
        migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);");
        migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_Users_NationalIdNumber] ON [Users] ([NationalIdNumber]) WHERE [NationalIdNumber] IS NOT NULL;");
        migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_PasswordResetTokens_TokenHash] ON [PasswordResetTokens] ([TokenHash]);");
        migrationBuilder.Sql("CREATE INDEX [IX_Rentals_UserId] ON [Rentals] ([UserId]);");
        migrationBuilder.Sql("CREATE INDEX [IX_Rentals_VehicleId] ON [Rentals] ([VehicleId]);");
        migrationBuilder.Sql("CREATE INDEX [IX_DamageReports_RentalId] ON [DamageReports] ([RentalId]);");
        migrationBuilder.Sql("CREATE INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens] ([UserId]);");
        migrationBuilder.Sql("ALTER TABLE [Rentals] ADD CONSTRAINT [FK_Rentals_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;");
        migrationBuilder.Sql("ALTER TABLE [Rentals] ADD CONSTRAINT [FK_Rentals_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION;");
        migrationBuilder.Sql("ALTER TABLE [DamageReports] ADD CONSTRAINT [FK_DamageReports_Rentals_RentalId] FOREIGN KEY ([RentalId]) REFERENCES [Rentals] ([Id]) ON DELETE CASCADE;");
        migrationBuilder.Sql("ALTER TABLE [PasswordResetTokens] ADD CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => throw new NotSupportedException("GUID key migration is intentionally irreversible because the original integer IDs are not retained. Restore a pre-migration backup to roll back.");
}
