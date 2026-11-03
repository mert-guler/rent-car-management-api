using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarManagement.Infrastructure.Data.Migrations;

/// <summary>Converts legacy integer keys to GUIDs while retaining all existing rows and relationships.</summary>
public partial class ConvertEntityKeysToGuid : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        ALTER TABLE [Rentals] ADD [NewId] uniqueidentifier NULL, [NewUserId] uniqueidentifier NULL, [NewVehicleId] uniqueidentifier NULL;
        ALTER TABLE [Users] ADD [NewId] uniqueidentifier NULL;
        ALTER TABLE [Vehicles] ADD [NewId] uniqueidentifier NULL;
        ALTER TABLE [DamageReports] ADD [NewId] uniqueidentifier NULL, [NewRentalId] uniqueidentifier NULL;
        ALTER TABLE [SupportRequests] ADD [NewId] uniqueidentifier NULL;
        ALTER TABLE [PasswordResetTokens] ADD [NewId] uniqueidentifier NULL, [NewUserId] uniqueidentifier NULL;

        UPDATE [Users] SET [NewId] = NEWID();
        UPDATE [Vehicles] SET [NewId] = NEWID();
        UPDATE [Rentals] SET [NewId] = NEWID();
        UPDATE [SupportRequests] SET [NewId] = NEWID();
        UPDATE [PasswordResetTokens] SET [NewId] = NEWID();
        UPDATE r SET [NewUserId] = u.[NewId], [NewVehicleId] = v.[NewId]
          FROM [Rentals] r JOIN [Users] u ON u.[Id] = r.[UserId] JOIN [Vehicles] v ON v.[Id] = r.[VehicleId];
        UPDATE d SET [NewRentalId] = r.[NewId] FROM [DamageReports] d JOIN [Rentals] r ON r.[Id] = d.[RentalId];
        UPDATE p SET [NewUserId] = u.[NewId] FROM [PasswordResetTokens] p JOIN [Users] u ON u.[Id] = p.[UserId];

        ALTER TABLE [DamageReports] DROP CONSTRAINT [FK_DamageReports_Rentals_RentalId];
        ALTER TABLE [PasswordResetTokens] DROP CONSTRAINT [FK_PasswordResetTokens_Users_UserId];
        ALTER TABLE [Rentals] DROP CONSTRAINT [FK_Rentals_Users_UserId];
        ALTER TABLE [Rentals] DROP CONSTRAINT [FK_Rentals_Vehicles_VehicleId];
        DROP INDEX [IX_DamageReports_RentalId] ON [DamageReports];
        DROP INDEX [IX_PasswordResetTokens_TokenHash] ON [PasswordResetTokens];
        DROP INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens];
        DROP INDEX [IX_Rentals_UserId] ON [Rentals];
        DROP INDEX [IX_Rentals_VehicleId] ON [Rentals];
        DROP INDEX [IX_Users_Email] ON [Users];
        DROP INDEX [IX_Users_NationalIdNumber] ON [Users];
        ALTER TABLE [DamageReports] DROP CONSTRAINT [PK_DamageReports];
        ALTER TABLE [PasswordResetTokens] DROP CONSTRAINT [PK_PasswordResetTokens];
        ALTER TABLE [Rentals] DROP CONSTRAINT [PK_Rentals];
        ALTER TABLE [SupportRequests] DROP CONSTRAINT [PK_SupportRequests];
        ALTER TABLE [Users] DROP CONSTRAINT [PK_Users];
        ALTER TABLE [Vehicles] DROP CONSTRAINT [PK_Vehicles];

        ALTER TABLE [DamageReports] DROP COLUMN [Id], [RentalId];
        ALTER TABLE [PasswordResetTokens] DROP COLUMN [Id], [UserId];
        ALTER TABLE [Rentals] DROP COLUMN [Id], [UserId], [VehicleId];
        ALTER TABLE [SupportRequests] DROP COLUMN [Id];
        ALTER TABLE [Users] DROP COLUMN [Id];
        ALTER TABLE [Vehicles] DROP COLUMN [Id];

        EXEC sp_rename 'Users.NewId', 'Id', 'COLUMN';
        EXEC sp_rename 'Vehicles.NewId', 'Id', 'COLUMN';
        EXEC sp_rename 'Rentals.NewId', 'Id', 'COLUMN';
        EXEC sp_rename 'Rentals.NewUserId', 'UserId', 'COLUMN';
        EXEC sp_rename 'Rentals.NewVehicleId', 'VehicleId', 'COLUMN';
        EXEC sp_rename 'DamageReports.NewId', 'Id', 'COLUMN';
        EXEC sp_rename 'DamageReports.NewRentalId', 'RentalId', 'COLUMN';
        EXEC sp_rename 'SupportRequests.NewId', 'Id', 'COLUMN';
        EXEC sp_rename 'PasswordResetTokens.NewId', 'Id', 'COLUMN';
        EXEC sp_rename 'PasswordResetTokens.NewUserId', 'UserId', 'COLUMN';

        ALTER TABLE [Users] ALTER COLUMN [Id] uniqueidentifier NOT NULL;
        ALTER TABLE [Vehicles] ALTER COLUMN [Id] uniqueidentifier NOT NULL;
        ALTER TABLE [Rentals] ALTER COLUMN [Id] uniqueidentifier NOT NULL;
        ALTER TABLE [Rentals] ALTER COLUMN [UserId] uniqueidentifier NOT NULL;
        ALTER TABLE [Rentals] ALTER COLUMN [VehicleId] uniqueidentifier NOT NULL;
        ALTER TABLE [DamageReports] ALTER COLUMN [Id] uniqueidentifier NOT NULL;
        ALTER TABLE [DamageReports] ALTER COLUMN [RentalId] uniqueidentifier NOT NULL;
        ALTER TABLE [SupportRequests] ALTER COLUMN [Id] uniqueidentifier NOT NULL;
        ALTER TABLE [PasswordResetTokens] ALTER COLUMN [Id] uniqueidentifier NOT NULL;
        ALTER TABLE [PasswordResetTokens] ALTER COLUMN [UserId] uniqueidentifier NOT NULL;

        ALTER TABLE [Users] ADD CONSTRAINT [PK_Users] PRIMARY KEY ([Id]);
        ALTER TABLE [Vehicles] ADD CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id]);
        ALTER TABLE [Rentals] ADD CONSTRAINT [PK_Rentals] PRIMARY KEY ([Id]);
        ALTER TABLE [DamageReports] ADD CONSTRAINT [PK_DamageReports] PRIMARY KEY ([Id]);
        ALTER TABLE [SupportRequests] ADD CONSTRAINT [PK_SupportRequests] PRIMARY KEY ([Id]);
        ALTER TABLE [PasswordResetTokens] ADD CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id]);
        CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
        CREATE UNIQUE INDEX [IX_Users_NationalIdNumber] ON [Users] ([NationalIdNumber]) WHERE [NationalIdNumber] IS NOT NULL;
        CREATE UNIQUE INDEX [IX_PasswordResetTokens_TokenHash] ON [PasswordResetTokens] ([TokenHash]);
        CREATE INDEX [IX_Rentals_UserId] ON [Rentals] ([UserId]);
        CREATE INDEX [IX_Rentals_VehicleId] ON [Rentals] ([VehicleId]);
        CREATE INDEX [IX_DamageReports_RentalId] ON [DamageReports] ([RentalId]);
        CREATE INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens] ([UserId]);
        ALTER TABLE [Rentals] ADD CONSTRAINT [FK_Rentals_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
        ALTER TABLE [Rentals] ADD CONSTRAINT [FK_Rentals_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION;
        ALTER TABLE [DamageReports] ADD CONSTRAINT [FK_DamageReports_Rentals_RentalId] FOREIGN KEY ([RentalId]) REFERENCES [Rentals] ([Id]) ON DELETE CASCADE;
        ALTER TABLE [PasswordResetTokens] ADD CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => throw new NotSupportedException("GUID key migration is intentionally irreversible because the original integer IDs are not retained. Restore a pre-migration backup to roll back.");
}
