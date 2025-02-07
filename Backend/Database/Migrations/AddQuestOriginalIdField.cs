using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(42)]
public class AddQuestOriginalIdField : Migration
{
    private const string PlayerQuestTable = "mod_player_quest";
    
    public override void Up()
    {
        Alter.Table(PlayerQuestTable)
            .AddColumn("original_quest_id").AsGuid().NotNullable();

        Execute.Sql(
            """
            CREATE UNIQUE INDEX idx_mod_player_quest_unique_quest_player 
            ON public.mod_player_quest (original_quest_id, player_id);
            """
        );
    }

    public override void Down()
    {
        Delete.Column("original_quest_id")
            .FromTable(PlayerQuestTable);

        Delete.Index("idx_mod_player_quest_unique_quest_player");
    }
}