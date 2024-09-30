using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

// [Migration(37)]
// public class AddItemsExtendedProperties : Migration
// {
//     public const string TraitTable = "mod_trait";
//     public const string TraitPropertiesTable = "mod_trait_properties";
//     public const string ElementTraitTable = "mod_element_trait";
//     public const string ElementTraitPropertiesTable = "mod_element_trait_properties";
//
//     public override void Up()
//     {
//         Create.Table(TraitTable)
//             .InSchema("public")
//             .WithColumn("id").AsGuid().NotNullable().PrimaryKey().WithDefault(SystemMethods.NewGuid)
//             .WithColumn("name").AsString().NotNullable()
//             .WithColumn("description").AsString().NotNullable();
//
//         Create.Table(TraitPropertiesTable)
//             .InSchema("public")
//             .WithColumn("id").AsGuid().NotNullable().PrimaryKey().WithDefault(SystemMethods.NewGuid)
//             .WithColumn("trait_id").AsGuid().ForeignKey(TraitTable, "id")
//             .WithColumn("name").AsString().NotNullable().Indexed()
//             .WithColumn("type").AsString(20).NotNullable().WithDefaultValue("string")
//             .WithColumn("default_value").AsString().Nullable();
//
//         Create.Table(ElementTraitTable)
//             .InSchema("public")
//             .WithColumn("id").AsGuid().NotNullable().PrimaryKey().WithDefault(SystemMethods.NewGuid)
//             .WithColumn("element_name").AsString().NotNullable()
//             .WithColumn("trait_id").AsGuid().NotNullable().ForeignKey(TraitTable, "id");
//
//         Execute.Sql(
//             """
//             CREATE UNIQUE INDEX idx_element_trait_unique ON mod_element_trait (element_name, trait_id);
//             """
//         );
//
//         Create.Table(ElementTraitPropertiesTable)
//             .WithColumn("id").AsGuid().NotNullable().PrimaryKey().WithDefault(SystemMethods.NewGuid)
//             .WithColumn("element_trait_id").AsGuid().ForeignKey(ElementTraitTable, "id").NotNullable()
//             .WithColumn("trait_property_id").AsGuid().NotNullable().ForeignKey(TraitPropertiesTable, "id")
//             .WithColumn("value").AsString().Nullable();
//     }
//
//     public override void Down()
//     {
//         Delete.Index("idx_element_trait_unique");
//         Delete.Table(ElementTraitTable);
//         Delete.Table(TraitPropertiesTable);
//         Delete.Table(TraitTable);
//         Delete.Table(ElementTraitPropertiesTable);
//     }
// }