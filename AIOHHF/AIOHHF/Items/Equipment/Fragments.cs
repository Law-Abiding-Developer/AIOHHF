using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UWE;
using Object = UnityEngine.Object;

namespace AIOHHF.Items;

public static class Fragments
{
    private static PrefabInfo[] _fragmentPIs = new PrefabInfo[3];
    private static CustomPrefab[] _fragmentCPs = new CustomPrefab[3];
    public static TechType FragmentsTechType;
    public static IEnumerator Initialize(WaitScreenHandler.WaitScreenTask task)
    {
        task.Status = "Registering All In One Hand Held Fabricator...\nRegistering Fragments\nCreating TechType";
        yield return task;
        var fragments = FragmentsTechType = EnumHandler.AddEntry<TechType>("AIOHHFFragment").Value;
        task.Status = "Registering All In One Hand Held Fabricator...\nRegistering Fragments\nCreating List of biomes that the fragments spawn in";
        yield return task;
        var biomesToSpawnIn = new List<LootDistributionData.BiomeData>()
        {
            new LootDistributionData.BiomeData()
            {
                biome = BiomeType.Kelp_Sand,
                probability = 0.05f,
                count = 1
            },
            new LootDistributionData.BiomeData()
            {
                biome = BiomeType.Kelp_GrassSparse,
                probability = 0.05f,
                count = 1
            }
        };
        foreach (BiomeType item in Enum.GetValues(typeof(BiomeType)))
        {
            if (!(item.ToString().Contains("Obsolete") || item.ToString().Contains("Unused"))
                && (item.ToString().Contains("Tech")
                    || item.ToString().Contains("EscapePod")
                    || item.ToString().Contains("Ship")
                    || item.ToString().Contains("Aurora")))
            {
                task.Status = "Registering All In One Hand Held Fabricator...\nRegistering Fragments\nCreating List of biomes that the fragments spawn in\nBiome: " + item.ToString();
                yield return task;
                biomesToSpawnIn.Add(new LootDistributionData.BiomeData()
                {
                    biome = item,
                    probability = 0.05f,
                    count = 1
                });
            }
        }
        for (var i = 0; i < 3; i++)
        {
            task.Status = $"Registering All In One Hand Held Fabricator...\nRegistering Fragments\n Fragment {i}: Creating PrefabInfo";
            yield return task;
            _fragmentPIs[i] = new PrefabInfo("AIOHHFF" + i, "aiohhffragprefab" + i, fragments);
            task.Status = $"Registering All In One Hand Held Fabricator...\nRegistering Fragments\n Fragment {i}: Creating CustomPrefab";
            yield return task;
            _fragmentCPs[i] = new CustomPrefab(_fragmentPIs[i]);
            task.Status = $"Registering All In One Hand Held Fabricator...\nRegistering Fragments\n Fragment {i}: Creating WorldEntityInfo";
            yield return task;
            var WEI = new WorldEntityInfo()
            {
                techType = fragments,
                localScale = Vector3.one*100,
                slotType = EntitySlot.Type.Small,
                cellLevel = LargeWorldEntity.CellLevel.Near,
                prefabZUp = false
            };
            task.Status = $"Registering All In One Hand Held Fabricator...\nRegistering Fragments\n Fragment {i}: Setting spawns";
            yield return task;
            _fragmentCPs[i].SetSpawns(WEI, biomesToSpawnIn.ToArray());
            task.Status = $"Registering All In One Hand Held Fabricator...\nRegistering Fragments\n Fragment {i}: Setting GameObject";
            yield return task;
            var i1 = i;
            _fragmentCPs[i].SetGameObject(() =>
            {
                GameObject fragment =
                    Object.Instantiate(Plugin.Aiohhf.Bundle.LoadAsset<GameObject>("aiohhffragprefab1"));
                fragment.SetActive(false);
                PrefabUtils.AddBasicComponents(fragment, _fragmentPIs[i1].ClassID, _fragmentPIs[i1].TechType,
                    LargeWorldEntity.CellLevel.Global);
                MaterialUtils.ApplySNShaders(fragment);
                return fragment;
            });
            task.Status = $"Registering All In One Hand Held Fabricator...\nRegistering Fragments\n Fragment {i}: Wrapping up by registering...";
            yield return task;
            _fragmentCPs[i].Register();
            task.Status = $"Registering All In One Hand Held Fabricator...\nRegistering Fragments\n Fragment {i}: Done!";
            yield return null;
        }
        task.Status = "Registering All In One Hand Held Fabricator...\nRegistering Fragments: Done!";
        yield return null;
    }
}
    