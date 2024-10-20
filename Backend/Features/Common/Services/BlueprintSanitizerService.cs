using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Common.Services
{
    public class BlueprintSanitizerService : IBlueprintSanitizerService
    {
        public async Task<BlueprintSanitationResult> SanitizeAsync(IGameplayBank bank, byte[] blueprintBytes, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream(blueprintBytes);
            using var streamReader = new StreamReader(memoryStream);
#pragma warning disable CAC001
            await using var textReader = new JsonTextReader(streamReader);
#pragma warning restore CAC001

            // ReSharper disable once AccessToStaticMemberViaDerivedType
            var bp = await JObject.ReadFromAsync(textReader, cancellationToken).ConfigureAwait(false);

            var model = bp["Model"];
            if (model == null)
            {
                return BlueprintSanitationResult.Failed("Not a valid BP");
            }

            model["FreeDeploy"] = false;

            if (bp["Model"]?["JsonProperties"] == null)
            {
                return BlueprintSanitationResult.Failed("BP is Missing JsonProperties");
            }

            var jsonPropObj = bp["Model"]?["JsonProperties"] !;
            jsonPropObj["isNPC"] = false;
            jsonPropObj["isUntargetable"] = false;
            jsonPropObj["planetProperties"] = null;

            var serverProps = bp["Model"]?["JsonProperties"]?["serverProperties"];
            if (serverProps != null)
            {
                serverProps["isFixture"] = null;
                serverProps["isBase"] = null;
                serverProps["isFlaggedForModeration"] = null;
                serverProps["isDynamicWreck"] = false;
                serverProps["fuelType"] = null;
                serverProps["fuelAmount"] = null;
                serverProps["compacted"] = false;
                serverProps["dynamicFixture"] = null;
                serverProps["constructCloneSource"] = null;
                serverProps["rdmsTags"] = JObject.FromObject(new
                {
                    constructTags = Array.Empty<object>(),
                    elementsTags = Array.Empty<object>(),
                });
            }

            if (bp["Elements"] == null)
            {
                return BlueprintSanitationResult.Succeeded(blueprintBytes);
            }

            var elementsToken = bp["Elements"] !;

            foreach (var item in elementsToken)
            {
                var elementType = item["elementType"] !;
                var elementTypeULong = elementType.Value<ulong>();

                if (item["properties"] is not JArray properties)
                {
                    continue;
                }

                foreach (var prop in properties)
                {
                    if (prop is not JArray)
                    {
                        continue;
                    }

                    var propName = prop[0] !.ToString();
                    var propValue = prop[1] !;

                    prop[1] = this.GetDefaultValue(bank, elementTypeULong, propName, propValue);
                }
            }

            var jsonString = bp.ToString();
            var result = Encoding.Default.GetBytes(jsonString);

            return BlueprintSanitationResult.Succeeded(result);
        }

        private JToken GetDefaultValue(IGameplayBank bank, ulong elementType, string propName, JToken value)
        {
            var obj = bank.GetBaseObject<Element>(elementType);
            var def = bank.GetDefinition(elementType);

            if (def == null || obj == null)
            {
                return value;
            }

            if (obj.hidden)
            {
                throw new InvalidOperationException("BP has hidden element");
            }

            var propVal = def.GetStaticPropertyOpt(propName);
            if (propVal != null)
            {
                return JObject.FromObject(new { type = (int)propVal.type, propVal.value });
            }

            return value;
        }
    }
}