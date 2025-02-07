using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(11)]
public class AddSkinsAndPetsToAllExistingPlayers : Migration
{
    public override void Up()
    {
        Execute.Sql(
            """
            CREATE OR REPLACE FUNCTION sp_add_player_skins_for_all()
            RETURNS void AS $$
            DECLARE
                rec RECORD;
            BEGIN
                -- Loop through each player ID that does not have skins
                FOR rec IN
                    SELECT P.id
                    FROM player P
                    LEFT JOIN player_skins PS ON (PS.player_id = P.id)
                    WHERE PS.id IS NULL
                LOOP
                    -- Call the fn_add_player_skins function for each player ID
                    PERFORM sp_add_player_skins(rec.id);
            		PERFORM sp_add_player_char_skins(rec.id);
                END LOOP;
            END;
            $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION sp_add_player_char_skins(
                player_id bigint
            ) RETURNS void AS $$
            BEGIN
            	INSERT INTO player_property (player_id, property_type, name, value)
            	VALUES
            		(player_id, 4, 'skinsAvailable', '[{"skin":"alpha","variation":""},{"skin":"arkship","variation":""},{"skin":"arkship","variation":"black"},{"skin":"arkship","variation":"bronze"},{"skin":"arkship","variation":"silver"},{"skin":"default","variation":""},{"skin":"earth","variation":""},{"skin":"earth","variation":"black"},{"skin":"earth","variation":"silver"},{"skin":"military","variation":""}]'),
            		(player_id, 4, 'emotesAvailable', '["emote_approve","emote_cry","emote_dance","emote_disapprove","emote_doit","emote_palm","emote_salute","emote_silly","emote_throat","emote_victory"]'),
            		(player_id, 4, 'petsAvailable', '[11,12,13,1,2,3,4,5]')
            		ON CONFLICT DO NOTHING;
            END;
            $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION sp_add_player_skins(
                player_id bigint
            ) RETURNS void AS $$
            BEGIN
            	INSERT INTO public.player_skins (player_id, item_type, name)
            	VALUES
            		(player_id, 297147615, 'Rust'),
            		(player_id, 297147615, 'Silver'),
            		(player_id, 297147615, 'Gold'),
            		(player_id, 297147615, 'Obsidian'),
            		(player_id, 1139773633, 'Rust'),
            		(player_id, 1139773633, 'Silver'),
            		(player_id, 1139773633, 'Gold'),
            		(player_id, 1139773633, 'Obsidian'),
            		(player_id, 1884031929, 'Rust'),
            		(player_id, 1884031929, 'Silver'),
            		(player_id, 1884031929, 'Gold'),
            		(player_id, 3686074288, 'Red'),
            		(player_id, 3686074288, 'Green'),
            		(player_id, 3686074288, 'Purple'),
            		(player_id, 3686074288, 'Gold'),
            		(player_id, 3686074288, 'Black'),
            		(player_id, 2737703104, 'Rust'),
            		(player_id, 2737703104, 'Silver'),
            		(player_id, 2737703104, 'Gold'),
            		(player_id, 2737703104, 'Obsidian'),
            		(player_id, 3415128439, 'Retro'),
            		(player_id, 3685998465, 'Red'),
            		(player_id, 3685998465, 'Green'),
            		(player_id, 3685998465, 'Purple'),
            		(player_id, 3685998465, 'Gold'),
            		(player_id, 3685998465, 'Black'),
            		(player_id, 3686006062, 'Red'),
            		(player_id, 3686006062, 'Green'),
            		(player_id, 3686006062, 'Purple'),
            		(player_id, 3686006062, 'Gold'),
            		(player_id, 3686006062, 'Black'),
            		(player_id, 3685982092, 'Red'),
            		(player_id, 3685982092, 'Green'),
            		(player_id, 3685982092, 'Purple'),
            		(player_id, 3685982092, 'Gold'),
            		(player_id, 3685982092, 'Black'),
            		(player_id, 3415128439, 'Secu'),
            		(player_id, 2667697870, 'Rust'),
            		(player_id, 2667697870, 'Silver'),
            		(player_id, 2667697870, 'Gold'),
            		(player_id, 2667697870, 'Obsidian'),
            		(player_id, 1899560165, 'Rust'),
            		(player_id, 1899560165, 'Silver'),
            		(player_id, 1899560165, 'Gold'),
            		(player_id, 1899560165, 'Obsidian'),
            		(player_id, 4078067869, 'Rust'),
            		(player_id, 4078067869, 'Silver'),
            		(player_id, 4078067869, 'Gold'),
            		(player_id, 4078067869, 'Obsidian'),
            		(player_id, 1856288931, 'Rust'),
            		(player_id, 1856288931, 'Silver'),
            		(player_id, 1856288931, 'Gold'),
            		(player_id, 1856288931, 'Obsidian'),
            		(player_id, 4017253256, 'Rust'),
            		(player_id, 4017253256, 'Silver'),
            		(player_id, 4017253256, 'Gold'),
            		(player_id, 4017253256, 'Obsidian'),
            		(player_id, 1923840124, 'Rust'),
            		(player_id, 1923840124, 'Silver'),
            		(player_id, 1923840124, 'Gold'),
            		(player_id, 1923840124, 'Obsidian'),
            		(player_id, 2334843027, 'Rust'),
            		(player_id, 2334843027, 'Silver'),
            		(player_id, 2334843027, 'Gold'),
            		(player_id, 2334843027, 'Obsidian'),
            		(player_id, 2292270972, 'Rust'),
            		(player_id, 2292270972, 'Silver'),
            		(player_id, 2292270972, 'Gold'),
            		(player_id, 2292270972, 'Obsidian'),
            		(player_id, 1109114394, 'Luxury')
            		ON CONFLICT DO NOTHING;
            END;
            $$ LANGUAGE plpgsql;
            """
        );

        Execute.Sql("SELECT sp_add_player_skins_for_all();");
    }

    public override void Down()
    {
        
    }
}