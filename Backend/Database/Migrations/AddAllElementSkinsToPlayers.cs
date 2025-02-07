using System;
using FluentMigrator;
using Mod.DynamicEncounters.Common.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

/// <summary>
/// Adds all element skins to new players
/// </summary>
[Migration(2)]
public class AddAllElementSkinsToPlayers : Migration
{
    private const string PlayerSkinsTable = "player_skins";

    public override void Up()
    {
        var enabled = EnvironmentVariableHelper.GetEnvironmentVarOrDefault("ENABLE_GIVE_DEFAULT_MIGRATION", "");
        if (string.IsNullOrEmpty(enabled))
        {
            return;
        }
        
        Execute.Sql($"""
                    CREATE OR REPLACE FUNCTION fn_add_player_skins()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        INSERT INTO public.{PlayerSkinsTable} (player_id, item_type, name)
                        VALUES
                            (NEW.id, 297147615, 'Rust'),
                            (NEW.id, 297147615, 'Silver'),
                            (NEW.id, 297147615, 'Gold'),
                            (NEW.id, 297147615, 'Obsidian'),
                            (NEW.id, 1139773633, 'Rust'),
                            (NEW.id, 1139773633, 'Silver'),
                            (NEW.id, 1139773633, 'Gold'),
                            (NEW.id, 1139773633, 'Obsidian'),
                            (NEW.id, 1884031929, 'Rust'),
                            (NEW.id, 1884031929, 'Silver'),
                            (NEW.id, 1884031929, 'Gold'),
                            (NEW.id, 3686074288, 'Red'),
                            (NEW.id, 3686074288, 'Green'),
                            (NEW.id, 3686074288, 'Purple'),
                            (NEW.id, 3686074288, 'Gold'),
                            (NEW.id, 3686074288, 'Black'),
                            (NEW.id, 2737703104, 'Rust'),
                            (NEW.id, 2737703104, 'Silver'),
                            (NEW.id, 2737703104, 'Gold'),
                            (NEW.id, 2737703104, 'Obsidian'),
                            (NEW.id, 3415128439, 'Retro'),
                            (NEW.id, 3685998465, 'Red'),
                            (NEW.id, 3685998465, 'Green'),
                            (NEW.id, 3685998465, 'Purple'),
                            (NEW.id, 3685998465, 'Gold'),
                            (NEW.id, 3685998465, 'Black'),
                            (NEW.id, 3686006062, 'Red'),
                            (NEW.id, 3686006062, 'Green'),
                            (NEW.id, 3686006062, 'Purple'),
                            (NEW.id, 3686006062, 'Gold'),
                            (NEW.id, 3686006062, 'Black'),
                            (NEW.id, 3685982092, 'Red'),
                            (NEW.id, 3685982092, 'Green'),
                            (NEW.id, 3685982092, 'Purple'),
                            (NEW.id, 3685982092, 'Gold'),
                            (NEW.id, 3685982092, 'Black'),
                            (NEW.id, 3415128439, 'Secu'),
                            (NEW.id, 2667697870, 'Rust'),
                            (NEW.id, 2667697870, 'Silver'),
                            (NEW.id, 2667697870, 'Gold'),
                            (NEW.id, 2667697870, 'Obsidian'),
                            (NEW.id, 1899560165, 'Rust'),
                            (NEW.id, 1899560165, 'Silver'),
                            (NEW.id, 1899560165, 'Gold'),
                            (NEW.id, 1899560165, 'Obsidian'),
                            (NEW.id, 4078067869, 'Rust'),
                            (NEW.id, 4078067869, 'Silver'),
                            (NEW.id, 4078067869, 'Gold'),
                            (NEW.id, 4078067869, 'Obsidian'),
                            (NEW.id, 1856288931, 'Rust'),
                            (NEW.id, 1856288931, 'Silver'),
                            (NEW.id, 1856288931, 'Gold'),
                            (NEW.id, 1856288931, 'Obsidian'),
                            (NEW.id, 4017253256, 'Rust'),
                            (NEW.id, 4017253256, 'Silver'),
                            (NEW.id, 4017253256, 'Gold'),
                            (NEW.id, 4017253256, 'Obsidian'),
                            (NEW.id, 1923840124, 'Rust'),
                            (NEW.id, 1923840124, 'Silver'),
                            (NEW.id, 1923840124, 'Gold'),
                            (NEW.id, 1923840124, 'Obsidian'),
                            (NEW.id, 2334843027, 'Rust'),
                            (NEW.id, 2334843027, 'Silver'),
                            (NEW.id, 2334843027, 'Gold'),
                            (NEW.id, 2334843027, 'Obsidian'),
                            (NEW.id, 2292270972, 'Rust'),
                            (NEW.id, 2292270972, 'Silver'),
                            (NEW.id, 2292270972, 'Gold'),
                            (NEW.id, 2292270972, 'Obsidian'),
                            (NEW.id, 1109114394, 'Luxury');
                    
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DROP TRIGGER IF EXISTS trigger_add_player_skins ON player;
                    
                    CREATE TRIGGER trigger_add_player_skins
                    AFTER INSERT ON player
                    FOR EACH ROW
                    EXECUTE FUNCTION fn_add_player_skins();
                    """
        );
    }

    public override void Down()
    {
        Execute.Sql($"DROP TRIGGER IF EXISTS trigger_add_player_skins ON public.{PlayerSkinsTable}");
    }
}