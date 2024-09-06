using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(3)]
public class AddAllPetsEmotesAndSkinsToPlayers : Migration
{
    public override void Up()
    {
        const string petsJson = "[11,12,13,1,2,3,4,5]";
        const string emotesJson = """["emote_approve","emote_cry","emote_dance","emote_disapprove","emote_doit","emote_palm","emote_salute","emote_silly","emote_throat","emote_victory"]""";
        const string skinsJson = """[{"skin":"alpha","variation":""},{"skin":"arkship","variation":""},{"skin":"arkship","variation":"black"},{"skin":"arkship","variation":"bronze"},{"skin":"arkship","variation":"silver"},{"skin":"default","variation":""},{"skin":"earth","variation":""},{"skin":"earth","variation":"black"},{"skin":"earth","variation":"silver"},{"skin":"military","variation":""}]""";
        
        Execute.Sql($"""
                     CREATE OR REPLACE FUNCTION fn_add_player_properties()
                     RETURNS TRIGGER AS $$
                     BEGIN
                         INSERT INTO player_property (player_id, property_type, name, value)
                         VALUES
                             (NEW.id, 4, 'skinsAvailable', '{skinsJson}'),
                             (NEW.id, 4, 'emotesAvailable', '{emotesJson}'),
                             (NEW.id, 4, 'petsAvailable', '{petsJson}');
                     
                         RETURN NEW;
                     END;
                     $$ LANGUAGE plpgsql;

                     DROP TRIGGER IF EXISTS trigger_add_player_properties ON public.player;
                     
                     CREATE TRIGGER trigger_add_player_properties
                     AFTER INSERT ON player
                     FOR EACH ROW
                     EXECUTE FUNCTION fn_add_player_properties();
                     """
        );
    }

    public override void Down()
    {
        Execute.Sql("DROP TRIGGER IF EXISTS trigger_add_player_properties ON public.player");
    }
}